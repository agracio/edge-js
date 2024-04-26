using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EdgeJs
{
    public class Edge
    {
        static object syncRoot = new object();
        static bool initialized;
        static Func<object, Task<object>> compileFunc;
        static ManualResetEvent waitHandle = new ManualResetEvent(false);
        private static string edgeDirectory;
        private static List<Func<object, Task<object>>> compiledFuncs = new List<Func<object, Task<object>>>();

        static Edge()
        {
            // needed because we *must* load the edgedll from the original location else we'll freeze up.
            // 
            // CodeBase is preferred over Location if available. When running inside an AppDomain that 
            // is using shadow copying, Location points to the shadow-copied file, but CodeBase points 
            // at the original location. Calling back into EdgeJs.dll from node hangs if the shadow 
            // copy path is specified instead of the original path.
            var asm = typeof(Edge).Assembly;

            Uri codeBase;
            edgeDirectory = string.IsNullOrWhiteSpace(asm.CodeBase) || !Uri.TryCreate(asm.CodeBase, UriKind.Absolute, out codeBase) || !codeBase.IsFile
                ? Path.GetDirectoryName(asm.Location)
                : Path.GetDirectoryName(codeBase.LocalPath);
        }

        static string assemblyDirectory;
        static string AssemblyDirectory
        {
            get
            {
                if (assemblyDirectory == null)
                {
                    assemblyDirectory = Environment.GetEnvironmentVariable("EDGE_BASE_DIR");
                    if (string.IsNullOrEmpty(assemblyDirectory))
                    {
                        string codeBase = typeof(Edge).Assembly.CodeBase;
                        UriBuilder uri = new UriBuilder(codeBase);
                        string path = Uri.UnescapeDataString(uri.Path);
                        assemblyDirectory = Path.GetDirectoryName(path);
                    }
                }

                return assemblyDirectory;
            }
        }

        // in case we want to set this path and not use an enviroment var
        public static void SetAssemblyDirectory(string folder)
	    {
			assemblyDirectory = folder;
	    }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public Task<object> InitializeInternal(object input)
        {
            compileFunc = (Func<object, Task<object>>)input;
            initialized = true;
            waitHandle.Set();

            return Task<object>.FromResult((object)null);
        }
		[EditorBrowsable(EditorBrowsableState.Never)]
		public Task<object> IsInitializedInternal(object input)
		{
			return Task<object>.FromResult((object)(initialized ? "1" : "0"));
		}
		public static void Uninitialize()
		{
			lock (syncRoot)
			{
				initialized = false;
			}
		}

        [DllImport("libnode.dll", EntryPoint = "?Start@node@@YAHHQAPAD@Z", CallingConvention = CallingConvention.Cdecl)]
        static extern int NodeStartx86(int argc, string[] argv);

        [DllImport("libnode.dll", EntryPoint = "?Start@node@@YAHHQEAPEAD@Z", CallingConvention = CallingConvention.Cdecl)]
        static extern int NodeStartx64(int argc, string[] argv);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        private static extern int GetShortPathName(string LongPath, StringBuilder ShortPath, int BufferSize);

        [DllImport("kernel32.dll", EntryPoint = "LoadLibrary")]
        static extern int LoadLibrary([MarshalAs(UnmanagedType.LPStr)] string lpLibFileName);

        public static Func<object,Task<object>> Func(string code)
        {
            if (!initialized)
            {
                lock (syncRoot)
                {
                    if (!initialized)
                    {
                        Func<int, string[], int> nodeStart;
                        if (IntPtr.Size == 4)
                        {
                            LoadLibrary(AssemblyDirectory + @"\edge\x86\libnode.dll");
                            nodeStart = NodeStartx86;
                        }
                        else if (IntPtr.Size == 8)
                        {
                            LoadLibrary(AssemblyDirectory + @"\edge\x64\libnode.dll");
                            nodeStart = NodeStartx64;
                        }
                        else
                        {
                            throw new InvalidOperationException(
                                "Unsupported architecture. Only x86 and x64 are supported.");
                        }

                        Thread v8Thread = new Thread(() =>
                        {
                            List<string> argv = new List<string>();
                            argv.Add("node");
                            string node_params = Environment.GetEnvironmentVariable("EDGE_NODE_PARAMS");
                            if (!string.IsNullOrEmpty(node_params))
                            {
                                foreach (string p in node_params.Split(' '))
                                {
                                    argv.Add(p);
                                }
                            }

                            // Workaround for unicode characters in path
                            string path = AssemblyDirectory + "\\edge\\double_edge.js";
                            StringBuilder shortPath = new StringBuilder(255);
                            int result = GetShortPathName(path, shortPath, shortPath.Capacity);
                            argv.Add(shortPath.ToString());
                            // End workaround for unicode characters in path
                            
                            argv.Add(string.Format("-EdgeJs:{0}", Path.Combine(edgeDirectory, "EdgeJs.dll")));
                            nodeStart(argv.Count, argv.ToArray());
                        }, 1048576); // Force typical Windows stack size because less is liable to break

                        v8Thread.IsBackground = true;
                        v8Thread.Start();
                        waitHandle.WaitOne();

                        if (!initialized)
                        {
                            throw new InvalidOperationException("Unable to initialize Node.js runtime.");
                        }
                    }
                }
            }

            if (compileFunc == null)
            {
                throw new InvalidOperationException("Edge.Func cannot be used after Edge.Close had been called.");
            }

            var task = compileFunc(code);
            task.Wait();
            // aborted?
            if (true.Equals(task.Result))
                return null;
            // add compiled func to list
            var f = (Func<object, Task<object>>)task.Result;
            lock(syncRoot)
            {
                compiledFuncs.Add(f);
            }
            return f;
        }

        public static void Close()
        {
            if (compileFunc != null)
            {
                lock (syncRoot)
                {
                    // dispose all generated funcs
                    foreach (var f in compiledFuncs)
                        DisposeFunc(f);
                    compiledFuncs.Clear();

                    // dispose main compile func
                    Edge.Func("ABORT");
                    DisposeFunc(compileFunc);
                    compileFunc = null;
                }
            }
        }

        private static void DisposeFunc(Func<object, Task<object>> func)
        {
            if (func == null)
                return;
            var d = func.Target as IDisposable;
            if (d != null)
                d.Dispose();
        }
    }
}