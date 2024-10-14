using System;
using System.Linq;
using System.Security;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Loader;
using System.Linq.Expressions;
using System.Dynamic;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections;
using System.Threading.Tasks;
using System.IO;
using Microsoft.Extensions.DependencyModel;
// ReSharper disable InconsistentNaming

[StructLayout(LayoutKind.Sequential)]
// ReSharper disable once CheckNamespace
public struct V8ObjectData
{
    public int propertiesCount;
    public IntPtr propertyTypes;
    public IntPtr propertyNames;
    public IntPtr propertyValues;
}

[StructLayout(LayoutKind.Sequential)]
public struct V8ArrayData
{
    public int arrayLength;
    public IntPtr itemTypes;
    public IntPtr itemValues;
}

[StructLayout(LayoutKind.Sequential)]
public struct V8BufferData
{
    public int bufferLength;
    public IntPtr buffer;
}

public enum V8Type
{
    Function = 1,
    Buffer = 2,
    Array = 3,
    Date = 4,
    Object = 5,
    String = 6,
    Boolean = 7,
    Int32 = 8,
    UInt32 = 9,
    Number = 10,
    Null = 11,
    Task = 12,
    Exception = 13
}

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
public struct EdgeBootstrapperContext
{
    [MarshalAs(UnmanagedType.LPUTF8Str)]
    public string RuntimeDirectory;

    [MarshalAs(UnmanagedType.LPUTF8Str)]
    public string ApplicationDirectory;

    [MarshalAs(UnmanagedType.LPUTF8Str)]
    public string DependencyManifestFile;
}

public delegate void CallV8FunctionDelegate(IntPtr payload, int payloadType, IntPtr v8FunctionContext, IntPtr callbackContext, IntPtr callbackDelegate);
public delegate void TaskCompleteDelegate(IntPtr result, int resultType, int taskState, IntPtr context);



[SecurityCritical]
public class CoreCLREmbedding
{
    private class TaskState
    {
        public readonly TaskCompleteDelegate Callback;
        public readonly IntPtr Context;

        public TaskState(TaskCompleteDelegate callback, IntPtr context)
        {
            Callback = callback;
            Context = context;
        }
    }

    private class EdgeRuntimeEnvironment
    {
        public EdgeRuntimeEnvironment(EdgeBootstrapperContext bootstrapperContext)
        {
            ApplicationDirectory = bootstrapperContext.ApplicationDirectory;
            RuntimePath = bootstrapperContext.RuntimeDirectory;
            DependencyManifestFile = bootstrapperContext.DependencyManifestFile;
            StandaloneApplication = Path.GetDirectoryName(RuntimePath) == ApplicationDirectory;
        }

        public string RuntimePath
        {
            get;
        }

        public string ApplicationDirectory
        {
            get;
        }

        public string DependencyManifestFile
        {
            get;
        }

        public bool StandaloneApplication
        {
            get;
        }
    }

    private class EdgeAssemblyLoadContext : AssemblyLoadContext
    {
        protected override IntPtr LoadUnmanagedDll(string unmanagedDllName)
        {
            DebugMessage("EdgeAssemblyLoadContext::LoadUnmanagedDll (CLR) - Trying to resolve {0}", unmanagedDllName);

            string libraryPath = Resolver.GetNativeLibraryPath(unmanagedDllName);

            if (!String.IsNullOrEmpty(libraryPath))
            {
                DebugMessage("EdgeAssemblyLoadContext::LoadUnmanagedDll (CLR) - Successfully resolved to {0}", libraryPath);
                return LoadUnmanagedDllFromPath(libraryPath);
            }

            else
            {
                DebugMessage("EdgeAssemblyLoadContext::LoadUnmanagedDll (CLR) - Unable to resolve to any native library from the dependency manifest");
                return base.LoadUnmanagedDll(unmanagedDllName);
            }
        }

        protected override Assembly Load(AssemblyName assemblyName)
        {
            DebugMessage("EdgeAssemblyLoadContext::Load (CLR) - Trying to load {0}", assemblyName.Name);

            string assemblyPath = Resolver.GetAssemblyPath(assemblyName.Name);

            if (!String.IsNullOrEmpty(assemblyPath))
            {
                try
                {
                    DebugMessage("EdgeAssemblyLoadContext::Load (CLR) - Trying to load from {0}", assemblyPath);
                    Assembly assembly = LoadFromAssemblyPath(assemblyPath);

                    if (assembly != null)
                    {
                        DebugMessage("EdgeAssemblyLoadContext::Load (CLR) - Successfully resolved assembly to {0}", assemblyPath);
                        return assembly;
                    }
                }

                catch (Exception e)
                {
                    DebugMessage("EdgeAssemblyLoadContext::Load (CLR) - Error trying to load {0}: {1}{2}{3}", assemblyName.Name, e.Message, Environment.NewLine, e.StackTrace);

                    if (e.InnerException != null)
                    {
                        DebugMessage("EdgeAssemblyLoadContext::Load (CLR) - Inner exception: {0}{1}{2}", e.InnerException.Message, Environment.NewLine, e.InnerException.StackTrace);
                    }
                }
            }
            DebugMessage("EdgeAssemblyLoadContext::Load (CLR) - Assembly {0} was not found in EdgeAssemblyResolver", assemblyName.Name);
            return null;
        }
    }

    private class EdgeAssemblyResolver
    {
        internal readonly Dictionary<string, string> CompileAssemblies = new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, string> _libraries = new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, string> _nativeLibraries = new(StringComparer.OrdinalIgnoreCase);
        private readonly IList<string> _knownPaths = new List<string>();
        
        private readonly string _packagesPath;

        public EdgeAssemblyResolver()
        {
            DebugMessage("EdgeAssemblyResolver::ctor (CLR) - Starting");
            DebugMessage("EdgeAssemblyResolver::ctor (CLR) - Getting the packages path");

            _packagesPath = Environment.GetEnvironmentVariable("NUGET_PACKAGES");

            if (String.IsNullOrEmpty(_packagesPath))
            {
                string profileDirectory = Environment.GetEnvironmentVariable("USERPROFILE");

                if (String.IsNullOrEmpty(profileDirectory))
                {
                    profileDirectory = Environment.GetEnvironmentVariable("HOME");
                }
                
                if (!String.IsNullOrEmpty(profileDirectory))
                {
                    _packagesPath = Path.Combine(profileDirectory, ".nuget", "packages");
                }
            }

            DebugMessage("EdgeAssemblyResolver::ctor (CLR) - Packages path is {0}", _packagesPath);
            DebugMessage("EdgeAssemblyResolver::ctor (CLR) - Finished");
        }

        public void LoadDependencyManifest(string dependencyManifestFile)
        {
            DebugMessage("EdgeAssemblyResolver::LoadDependencyManifest (CLR) - Loading dependency manifest from {0}", dependencyManifestFile);
            
            DependencyContextJsonReader dependencyContextReader = new DependencyContextJsonReader();

            using (FileStream dependencyManifestStream = new FileStream(dependencyManifestFile, FileMode.Open, FileAccess.Read))
            {
                DebugMessage("EdgeAssemblyResolver::LoadDependencyManifest (CLR) - Reading dependency manifest file and merging in dependencies from the shared runtime");
                DependencyContext dependencyContext = dependencyContextReader.Read(dependencyManifestStream);

                string runtimeDependencyManifestFile = (string)AppContext.GetData("FX_DEPS_FILE");

                if (!String.IsNullOrEmpty(runtimeDependencyManifestFile) && runtimeDependencyManifestFile != dependencyManifestFile)
                {
                    DebugMessage("EdgeAssemblyResolver::LoadDependencyManifest (CLR) - Merging in the dependency manifest from the shared runtime at {0}", runtimeDependencyManifestFile);

                    using (FileStream runtimeDependencyManifestStream = new FileStream(runtimeDependencyManifestFile, FileMode.Open, FileAccess.Read))
                    {
                        dependencyContext = dependencyContext.Merge(dependencyContextReader.Read(runtimeDependencyManifestStream));
                    }
                }
                DebugMessage("EdgeAssemblyResolver::Runtime - ApplicationDirectory: {0}, RuntimePath: {1}, Standalone: {2}", RuntimeEnvironment.ApplicationDirectory, RuntimeEnvironment.RuntimePath, RuntimeEnvironment.StandaloneApplication);
                AddDependencies(dependencyContext, RuntimeEnvironment.StandaloneApplication);
            }

            DebugMessage("EdgeAssemblyResolver::LoadDependencyManifest (CLR) - Finished");
        }
        
        private void AddDependencies(DependencyContext dependencyContext, bool standalone)
        {
            DebugMessage("EdgeAssemblyResolver::AddDependencies (CLR) - Adding dependencies for: {0}, standalone: {1}", dependencyContext.Target.Framework, standalone);

            AddCompileDependencies(dependencyContext, standalone);

            Dictionary<string, string> supplementaryRuntimeLibraries = new Dictionary<string, string>();
            var runtimePath = Path.GetDirectoryName(RuntimeEnvironment.RuntimePath);

            foreach (RuntimeLibrary runtimeLibrary in dependencyContext.RuntimeLibraries)
            {
                DebugMessage("EdgeAssemblyResolver::AddDependencies (CLR) - Processing runtime dependency {1} {0}", runtimeLibrary.Name, runtimeLibrary.Type);
                
                if (!_libraries.ContainsKey(runtimeLibrary.Name) && CompileAssemblies.ContainsKey(runtimeLibrary.Name))
                {
                    DebugMessage("EdgeAssemblyResolver::AddDependencies (CLR) - Added runtime assembly {1} from {0}", CompileAssemblies[runtimeLibrary.Name], runtimeLibrary.Name);
                    _libraries[runtimeLibrary.Name] = CompileAssemblies[runtimeLibrary.Name];
                }
                
                if (_libraries.ContainsKey(runtimeLibrary.Name) && CompileAssemblies.ContainsKey(runtimeLibrary.Name))
                {
                    AddNativeAssemblies(dependencyContext, runtimeLibrary);
                    AddSupplementaryRuntime(runtimeLibrary);
                    continue;
                }
                

                List<string> assets = runtimeLibrary.RuntimeAssemblyGroups.GetRuntimeAssets(RuntimeInformation.RuntimeIdentifier).ToList();

                if (!assets.Any())
                {
                    assets = runtimeLibrary.RuntimeAssemblyGroups.GetDefaultAssets().ToList();
                }

                if (assets.Any())
                {
                    string assetPath = assets[0];

                    string assemblyPath;
                    if(runtimeLibrary.Type == "project")                    
                        assemblyPath = Path.Combine(RuntimeEnvironment.ApplicationDirectory, assetPath);
                    else if (standalone)
                        assemblyPath = Path.Combine(RuntimeEnvironment.ApplicationDirectory, Path.GetFileName(assetPath));
                    else 
                    {
                        assemblyPath = Path.Combine(_packagesPath, runtimeLibrary.Name.ToLower(), runtimeLibrary.Version, assetPath.Replace('/', Path.DirectorySeparatorChar).ToLower());
                        if(!File.Exists(assemblyPath))                                 
                            assemblyPath = Path.Combine(_packagesPath, runtimeLibrary.Name.ToLower(), runtimeLibrary.Version, assetPath.Replace('/', Path.DirectorySeparatorChar));
                    }
                    string libraryNameFromPath = Path.GetFileNameWithoutExtension(assemblyPath);

                    if (!File.Exists(assemblyPath) && !string.IsNullOrEmpty(runtimePath))
                    {
                        assemblyPath = Path.Combine(runtimePath, Path.GetFileName(assemblyPath));
                    }

                    if (!_libraries.ContainsKey(runtimeLibrary.Name))
                    {
                        if (File.Exists(assemblyPath))
                        {
                            _libraries[runtimeLibrary.Name] = assemblyPath;
                            DebugMessage("EdgeAssemblyResolver::AddDependencies (CLR) - Added runtime assembly {1} from {0}", assemblyPath, runtimeLibrary.Name);
                        }
                        else
                        {
                            DebugMessage("EdgeAssemblyResolver::AddDependencies (CLR) - Could not resolve runtime assembly {0}",
                                assemblyPath);
                        }
                    }

                    else
                    {
                        DebugMessage("EdgeAssemblyResolver::AddDependencies (CLR) - Already present in the runtime assemblies list, skipping");
                    }

                    if (runtimeLibrary.Name != libraryNameFromPath && !_libraries.ContainsKey(libraryNameFromPath))
                    {
                        if (File.Exists(assemblyPath))
                        {
                            supplementaryRuntimeLibraries[libraryNameFromPath] = assemblyPath;
                            DebugMessage("EdgeAssemblyResolver::AddDependencies (CLR) - Added supplementary runtime assembly {1} from {0}", assemblyPath, libraryNameFromPath);
                        }
                        else
                        {
                            DebugMessage("EdgeAssemblyResolver::AddDependencies (CLR) - Could not resolve supplementary runtime assembly {0}",
                                assemblyPath);
                        }
                    }

                    if (!CompileAssemblies.ContainsKey(runtimeLibrary.Name))
                    {
                        if (File.Exists(assemblyPath))
                        {
                            CompileAssemblies[runtimeLibrary.Name] = assemblyPath;
                            DebugMessage("EdgeAssemblyResolver::AddDependencies (CLR) - Added compile assembly {1} from {0}",  assemblyPath, runtimeLibrary.Name);
                        }
                        else
                        {
                            DebugMessage("EdgeAssemblyResolver::AddDependencies (CLR) - Could not resolve compile assembly {0}", assemblyPath);
                        }
                    }

                    else
                    {
                        DebugMessage("EdgeAssemblyResolver::AddDependencies (CLR) - Already present in the compile assemblies list, skipping");
                    }
                }
                else
                {
                    DebugMessage("EdgeAssemblyResolver::AddDependencies (CLR) - RuntimeAssemblyGroups does not have RuntimeAssets or DefaultAssets");
                    if (!CompileAssemblies.ContainsKey(runtimeLibrary.Name)) AddCompileDependencyFromRuntime(runtimeLibrary);
                }
                
                foreach (string libraryName in supplementaryRuntimeLibraries.Keys)
                {
                    if (!_libraries.ContainsKey(libraryName))
                    {
                        DebugMessage(
                            "EdgeAssemblyResolver::AddDependencies (CLR) - Filename in the dependency context did not match the package/project name, added additional resolver for {0}",
                            libraryName);
                        _libraries[libraryName] = supplementaryRuntimeLibraries[libraryName];
                    }

                    if (!CompileAssemblies.ContainsKey(libraryName))
                    {
                         CompileAssemblies[libraryName] = supplementaryRuntimeLibraries[libraryName];
                    }
                }

                AddNativeAssemblies(dependencyContext, runtimeLibrary);
            }
        }
        
        private void AddSupplementaryRuntime(RuntimeLibrary runtimeLibrary)
        {
            var libraryNameFromPath = Path.GetFileNameWithoutExtension(_libraries[runtimeLibrary.Name]);
            if (runtimeLibrary.Name != libraryNameFromPath)
            {
                _libraries.TryAdd(libraryNameFromPath, _libraries[runtimeLibrary.Name]);
                CompileAssemblies.TryAdd(libraryNameFromPath, _libraries[runtimeLibrary.Name]);
            }
        }

        private void AddNativeAssemblies(DependencyContext dependencyContext, RuntimeLibrary runtimeLibrary)
        {
            var runtimePath = Path.GetDirectoryName(RuntimeEnvironment.RuntimePath);
            List<string> nativeAssemblies = runtimeLibrary.GetRuntimeNativeAssets(dependencyContext, RuntimeInformation.RuntimeIdentifier).ToList();

            if (nativeAssemblies.Any())
            {
                DebugMessage("EdgeAssemblyResolver::AddDependencies (CLR) - Adding native dependencies for {0}", RuntimeInformation.RuntimeIdentifier);

                foreach (var nativeAssembly in nativeAssemblies)
                {
                    var nativeAssemblyPath = Path.Combine(_packagesPath, runtimeLibrary.Name, runtimeLibrary.Version, nativeAssembly.Replace('/', Path.DirectorySeparatorChar));
                    if (!File.Exists(nativeAssemblyPath) && !string.IsNullOrEmpty(runtimePath))
                    {
                        nativeAssemblyPath = Path.Combine(runtimePath, nativeAssembly.Replace('/', Path.DirectorySeparatorChar));
                    }
						
                    if (File.Exists(nativeAssemblyPath))
                    {
                        _nativeLibraries[Path.GetFileNameWithoutExtension(nativeAssembly)] = nativeAssemblyPath;
                        DebugMessage("EdgeAssemblyResolver::AddDependencies (CLR) - Adding native assembly {0} at {1}",
                            Path.GetFileNameWithoutExtension(nativeAssembly), nativeAssemblyPath);
                    }
                    else
                    {
                        DebugMessage("EdgeAssemblyResolver::AddDependencies (CLR) - Could not resolve native assembly {0} at {1}",
                            Path.GetFileNameWithoutExtension(nativeAssembly), nativeAssemblyPath);
                    }
                }
            }
        }

        private void AddCompileDependencyFromRuntime(RuntimeLibrary runtimeLibrary)
        {
            var runtimePath = Path.GetDirectoryName(RuntimeEnvironment.RuntimePath);
            DebugMessage("EdgeAssemblyResolver::AddDependencies (CLR) - Processing compile dependency {1} {0} using runtime path {2}", runtimeLibrary.Name, runtimeLibrary.Type, runtimePath);
            if (string.IsNullOrEmpty(runtimePath))
            {
                DebugMessage("EdgeAssemblyResolver::AddDependencies (CLR) - runtime path could not be resolved, skipping");
                return;
            }
            
            if (!CompileAssemblies.ContainsKey(runtimeLibrary.Name))
            {
                var asset = runtimeLibrary.Name;
                if (!asset.EndsWith(".dll"))
                {
                    asset += ".dll";
                }

                if (asset == "runtime.native.System.dll")
                {
                    asset = "System.dll";
                }
                
                var assemblyPath = Path.Combine(runtimePath, Path.GetFileName(asset));
                if (File.Exists(assemblyPath))
                {
                    CompileAssemblies[runtimeLibrary.Name] = assemblyPath;
                    DebugMessage("EdgeAssemblyResolver::AddDependencies (CLR) - Added compile dependency {1} from {0}", assemblyPath, runtimeLibrary.Name);
                }
                else
                {
                    DebugMessage("EdgeAssemblyResolver::AddDependencies (CLR) - Could not add compile dependency {0}", assemblyPath);
                }
            }
            else
            {
                DebugMessage("EdgeAssemblyResolver::AddDependencies (CLR) - Already present in compile dependency list, skipping");
            }
        }

        private void AddCompileDependencies(DependencyContext dependencyContext, bool standalone)
        {
            var runtimePath = Path.GetDirectoryName(RuntimeEnvironment.RuntimePath);
            foreach (CompilationLibrary compileLibrary in dependencyContext.CompileLibraries)
            {
                if (compileLibrary.Assemblies.Count == 0 || CompileAssemblies.ContainsKey(compileLibrary.Name))
                {
                    continue;
                }

                DebugMessage("EdgeAssemblyResolver::AddDependencies (CLR) - Processing compile assembly {1} {0} {2}", compileLibrary.Name, compileLibrary.Type, compileLibrary.Assemblies[0]);

                var assemblyPath = compileLibrary.Assemblies[0].Replace('/', Path.DirectorySeparatorChar);

                if (standalone)
                {
                    if (File.Exists(Path.Combine(RuntimeEnvironment.ApplicationDirectory, "refs", Path.GetFileName(compileLibrary.Assemblies[0].Replace('/', Path.DirectorySeparatorChar)))))
                    {
                        assemblyPath = Path.Combine(RuntimeEnvironment.ApplicationDirectory, "refs", Path.GetFileName(compileLibrary.Assemblies[0].Replace('/', Path.DirectorySeparatorChar)));
                    }
                    else if(!string.IsNullOrEmpty(runtimePath) && File.Exists(Path.Combine(runtimePath, Path.GetFileName(compileLibrary.Assemblies[0].Replace('/', Path.DirectorySeparatorChar)))))
                    {
                        assemblyPath = Path.Combine(runtimePath, Path.GetFileName(compileLibrary.Assemblies[0].Replace('/', Path.DirectorySeparatorChar)));
                    }
                }
                else 
                {
                    assemblyPath = Path.Combine(_packagesPath, compileLibrary.Name.ToLower(), compileLibrary.Version, compileLibrary.Assemblies[0].Replace('/', Path.DirectorySeparatorChar).ToLower());
                    if(!File.Exists(assemblyPath))                                 
                        assemblyPath = Path.Combine(_packagesPath, compileLibrary.Name.ToLower(), compileLibrary.Version, compileLibrary.Assemblies[0].Replace('/', Path.DirectorySeparatorChar));
                }
                
                if (!File.Exists(assemblyPath) && !string.IsNullOrEmpty(runtimePath))
                {
                    assemblyPath = Path.Combine(runtimePath, Path.GetFileName(assemblyPath));
                }

                if (!CompileAssemblies.ContainsKey(compileLibrary.Name))
                {
                    if (File.Exists(assemblyPath))
                    {
                        CompileAssemblies[compileLibrary.Name] = assemblyPath;
                        DebugMessage("EdgeAssemblyResolver::AddDependencies (CLR) - Added compile assembly {0}", assemblyPath);
                    }
                    else
                    {
                        DebugMessage("EdgeAssemblyResolver::AddDependencies (CLR) - Could not add compile assembly {0}", assemblyPath);
                    }
                }

                else
                {
                    DebugMessage("EdgeAssemblyResolver::AddDependencies (CLR) - Already present in the compile assemblies list, skipping");
                }
            }
        }

        public string GetAssemblyPath(string assemblyName)
        {
            DebugMessage("CoreCLREmbedding::GetAssemblyPath (CLR) - Resolving assembly {0}", assemblyName);
            
            if (!_libraries.ContainsKey(assemblyName))
            {
                if (!TryAddAssembly(assemblyName)) return null;
            }

            return _libraries[assemblyName];
        }

        public string GetNativeLibraryPath(string libraryName)
        {
            if (!_nativeLibraries.ContainsKey(libraryName))
            {
                return null;
            }

            return _nativeLibraries[libraryName];
        }

        internal void AddAssemblyPath(string assemblyPath)
        {
            DebugMessage("CoreCLREmbedding::AddAssemblyPath (CLR) - Adding known assembly path {0}", assemblyPath);
            
            if (!_knownPaths.Contains(assemblyPath))
            {
                _knownPaths.Add(assemblyPath);
            }
        }

        internal void AddCompiler(string bootstrapDependencyManifest)
        {
            DebugMessage("EdgeAssemblyResolver::AddCompiler (CLR) - Adding compiler from dependency manifest file {0}", bootstrapDependencyManifest);

            DependencyContextJsonReader dependencyContextReader = new DependencyContextJsonReader();
            
            using (FileStream bootstrapDependencyManifestStream = new FileStream(bootstrapDependencyManifest, FileMode.Open, FileAccess.Read))
            {
                DependencyContext compilerDependencyContext = dependencyContextReader.Read(bootstrapDependencyManifestStream);

                DebugMessage("EdgeAssemblyResolver::AddCompiler (CLR) - Adding dependencies for compiler");

                AddDependencies(compilerDependencyContext, false);

                DebugMessage("EdgeAssemblyResolver::AddCompiler (CLR) - Finished");
            }
        }

        private bool TryAddAssembly(string assemblyName)
        {
            foreach (var path in _knownPaths)
            {
                var assembly = Path.Combine(path, assemblyName + ".dll");
                if (File.Exists(assembly))
                {
                    _libraries[assemblyName] = assembly;
                    return true;
                }
                else
                {
                    assembly = Path.Combine(path, assemblyName + ".exe");
                    if (File.Exists(assembly))
                    {
                        _libraries[assemblyName] = assembly;
                        return true;
                    }
                }
            }

            return false;
        }
    }
    
    // ReSharper disable InconsistentNaming
    private static EdgeRuntimeEnvironment RuntimeEnvironment;
    private static EdgeAssemblyResolver Resolver;
    private static AssemblyLoadContext LoadContext = new EdgeAssemblyLoadContext();
    // ReSharper enable InconsistentNaming

    private static readonly bool DebugMode = Environment.GetEnvironmentVariable("EDGE_DEBUG") == "1";
    private static readonly long MinDateTimeTicks = 621355968000000000;
    private static readonly ConcurrentDictionary<Type, List<Tuple<string, Func<object, object>>>> TypePropertyAccessors = new();
    private static readonly int PointerSize = Marshal.SizeOf<IntPtr>();
    private static readonly int V8BufferDataSize = Marshal.SizeOf<V8BufferData>();
    private static readonly int V8ObjectDataSize = Marshal.SizeOf<V8ObjectData>();
    private static readonly int V8ArrayDataSize = Marshal.SizeOf<V8ArrayData>();
    private static readonly ConcurrentDictionary<string, Tuple<Type, MethodInfo>> Compilers = new();
    private static readonly IList<string> _failedAssemblyResolver = new List<string>();

    public static void Initialize(IntPtr context, IntPtr exception)
    {
        try
        {
            DebugMessage("CoreCLREmbedding::Initialize (CLR) - Starting");

            // The call to Marshal.PtrToStructure should be working
            // This appears to be a .Net Core issue - https://github.com/dotnet/coreclr/issues/22394
            // Manually marshaling as a workaround
            //EdgeBootstrapperContext bootstrapperContext = Marshal.PtrToStructure<EdgeBootstrapperContext>(context);
            EdgeBootstrapperContext bootstrapperContext = new EdgeBootstrapperContext
            {
                RuntimeDirectory = Marshal.PtrToStringUTF8(Marshal.ReadIntPtr(context)),
                ApplicationDirectory = Marshal.PtrToStringUTF8(Marshal.ReadIntPtr(context + IntPtr.Size)),
                DependencyManifestFile = Marshal.PtrToStringUTF8(Marshal.ReadIntPtr(context + 2 * IntPtr.Size)),
            };

            RuntimeEnvironment = new EdgeRuntimeEnvironment(bootstrapperContext);
            Resolver = new EdgeAssemblyResolver();

            AssemblyLoadContext.Default.Resolving += Assembly_Resolving;

            if (!String.IsNullOrEmpty(RuntimeEnvironment.DependencyManifestFile))
            {
                Resolver.LoadDependencyManifest(RuntimeEnvironment.DependencyManifestFile);
            }
            
            DebugMessage("CoreCLREmbedding::Initialize (CLR) - Complete");
        }

        catch (Exception e)
        {
            DebugMessage("CoreCLREmbedding::Initialize (CLR) - Exception was thrown: {0}{1}{2}", e.Message, Environment.NewLine, e.StackTrace);

            Marshal.WriteIntPtr(exception, MarshalCLRToV8(e, out _));
        }
    }

    private static Assembly Assembly_Resolving(AssemblyLoadContext arg1, AssemblyName arg2)
    {
        if (arg2.Name == "System.Core" || _failedAssemblyResolver.Contains(arg2.Name))
        {
            return null;
        }

        DebugMessage("CoreCLREmbedding::Assembly_Resolving (CLR) - Starting resolve process for {0}", arg2.Name);

        if (!String.IsNullOrEmpty(Resolver.GetAssemblyPath(arg2.Name)))
        {
            return LoadContext.LoadFromAssemblyName(arg2);
        }
        _failedAssemblyResolver.Add(arg2.Name);
        DebugMessage("CoreCLREmbedding::Assembly_Resolving (CLR) - Unable to resolve the assembly using the manifest list, returning null");
        
        return null;
    }

    [SecurityCritical]
    public static IntPtr GetFunc(string assemblyFile, string typeName, string methodName, IntPtr exception)
    {
        try
        {
            Marshal.WriteIntPtr(exception, IntPtr.Zero);

            Assembly assembly;

            if (assemblyFile.EndsWith(".dll", StringComparison.OrdinalIgnoreCase) || assemblyFile.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
            {
                if (!Path.IsPathRooted(assemblyFile))
                {
                    assemblyFile = Path.Combine(Directory.GetCurrentDirectory(), assemblyFile);
                    Resolver.AddAssemblyPath(Directory.GetCurrentDirectory());
                }
                else
                {
                    Resolver.AddAssemblyPath(Path.GetDirectoryName(assemblyFile));
                }

                assembly = LoadContext.LoadFromAssemblyPath(assemblyFile);
            }

            else
            {
                assembly = Assembly.Load(new AssemblyName(assemblyFile));
            }

            DebugMessage("CoreCLREmbedding::GetFunc (CLR) - Assembly {0} loaded successfully", assemblyFile);

            ClrFuncReflectionWrap wrapper = ClrFuncReflectionWrap.Create(assembly, typeName, methodName);
            DebugMessage("CoreCLREmbedding::GetFunc (CLR) - Method {0}.{1}() loaded successfully", typeName, methodName);

            Func<object, Task<object>> wrapperFunc = wrapper.Call;
            GCHandle wrapperHandle = GCHandle.Alloc(wrapperFunc);

            return GCHandle.ToIntPtr(wrapperHandle);
        }

        catch (Exception e)
        {
            DebugMessage("CoreCLREmbedding::GetFunc (CLR) - Exception was thrown: {0}{1}{2}", e.Message, Environment.NewLine, e.StackTrace);

            Marshal.WriteIntPtr(exception, MarshalCLRToV8(e, out _));

            return IntPtr.Zero;
        }
    }

    [SecurityCritical]
    public static IntPtr CompileFunc(IntPtr v8Options, int payloadType, IntPtr exception)
    {
        try
        {
            Marshal.WriteIntPtr(exception, IntPtr.Zero);

            IDictionary<string, object> options = (IDictionary<string, object>)MarshalV8ToCLR(v8Options, (V8Type)payloadType);
            string compiler = (string)options["compiler"];

            MethodInfo compileMethod;
            Type compilerType;
            DebugMessage("CoreCLREmbedding::CompileFunc (CLR) - Compiler loading {0} assembly", compiler);
            
            if (!Compilers.ContainsKey(compiler))
            {
                if (DependencyContext.Default ==null || !DependencyContext.Default.RuntimeLibraries.Any(l => l.Name == compiler))
                {
                    if (!File.Exists(options["bootstrapDependencyManifest"].ToString()))
                    {
                        throw new Exception(
                            String.Format(
                                "Compiler package {0} was not found in the application dependency manifest and no bootstrap dependency manifest was found for the compiler.  Make sure that you either have {0} in your project.json or, if you do not have a project.json, that you have the .NET Core SDK installed.", compiler));
                    }

                    Resolver.AddCompiler(options["bootstrapDependencyManifest"].ToString());
                }

                Assembly compilerAssembly;
                if (File.Exists(compiler))
                {
                    compilerAssembly = Assembly.LoadFrom(compiler);
                }
                else
                {
                    compilerAssembly = Assembly.Load(new AssemblyName(compiler));
                }
                DebugMessage("CoreCLREmbedding::CompileFunc (CLR) - Compiler assembly {0} loaded successfully", compiler);

                compilerType = compilerAssembly.GetType("EdgeCompiler");

                if (compilerType == null)
                {
                    throw new TypeLoadException("Could not load type 'EdgeCompiler'");
                }

                compileMethod = compilerType.GetMethod("CompileFunc", BindingFlags.Instance | BindingFlags.Public);

                if (compileMethod == null)
                {
                    throw new Exception("Unable to find the CompileFunc() method on " + compilerType.FullName + ".");
                }

                MethodInfo setAssemblyLoader = compilerType.GetMethod("SetAssemblyLoader", BindingFlags.Static | BindingFlags.Public);

                setAssemblyLoader?.Invoke(null, new object[]
                {
                    new Func<Stream, Assembly>(assemblyStream => AssemblyLoadContext.Default.LoadFromStream(assemblyStream, null))
                });

                Compilers[compiler] = new Tuple<Type, MethodInfo>(compilerType, compileMethod);
            }

            else
            {
                compilerType = Compilers[compiler].Item1;
                compileMethod = Compilers[compiler].Item2;
            }

            object compilerInstance = Activator.CreateInstance(compilerType);

            DebugMessage("CoreCLREmbedding::CompileFunc (CLR) - Starting compilation");
            var parameters =  compileMethod.GetParameters();

            Func<object, Task<object>> compiledFunction;
            // edge-cs compiler expects 2 parameters while most other compilers only expect 'options'
            // not ideal if there are more compilers that need different arguments, should be revisited in the future
            if (parameters.Length == 1)
            {
                compiledFunction = (Func<object, Task<object>>)compileMethod.Invoke(compilerInstance, new object[]
                {
                    options
                });
            }
            else
            {
                compiledFunction = (Func<object, Task<object>>)compileMethod.Invoke(compilerInstance, new object[]
                {
                    options,
                    Resolver.CompileAssemblies
                });
                
            }
            DebugMessage("CoreCLREmbedding::CompileFunc (CLR) - Compilation complete");

            GCHandle handle = GCHandle.Alloc(compiledFunction);

            return GCHandle.ToIntPtr(handle);
        }

        catch (TargetInvocationException e)
        {
            DebugMessage("CoreCLREmbedding::CompileFunc (CLR) - Exception was thrown: {0}\n{1}", e.InnerException.Message, e.InnerException.StackTrace);

            Marshal.WriteIntPtr(exception, MarshalCLRToV8(e, out _));

            return IntPtr.Zero;
        }

        catch (Exception e)
        {
            DebugMessage("CoreCLREmbedding::CompileFunc (CLR) - Exception was thrown: {0}{1}{2}", e.Message, Environment.NewLine, e.StackTrace);

            Marshal.WriteIntPtr(exception, MarshalCLRToV8(e, out _));

            return IntPtr.Zero;
        }
    }

    [SecurityCritical]
    public static void FreeHandle(IntPtr gcHandle)
    {
        GCHandle actualHandle = GCHandle.FromIntPtr(gcHandle);
        actualHandle.Free();
    }

    [SecurityCritical]
    public static void CallFunc(IntPtr function, IntPtr payload, int payloadType, IntPtr taskState, IntPtr result, IntPtr resultType)
    {
        try
        {
            DebugMessage("CoreCLREmbedding::CallFunc (CLR) - Starting");

            GCHandle wrapperHandle = GCHandle.FromIntPtr(function);
            Func<object, Task<object>> wrapperFunc = (Func<object, Task<object>>)wrapperHandle.Target;

            DebugMessage("CoreCLREmbedding::CallFunc (CLR) - Marshalling data of type {0} and calling the .NET method", ((V8Type)payloadType).ToString("G"));
            Task<Object> functionTask = wrapperFunc(MarshalV8ToCLR(payload, (V8Type)payloadType));

            
            // Read the task status only once - you can't assume that an asychronous task's state won't change between reads
            TaskStatus taskStatus = functionTask.Status;
            Marshal.WriteInt32(taskState, (int)taskStatus);

            switch (taskStatus)
            {
                case TaskStatus.Faulted:
                {
                    DebugMessage("CoreCLREmbedding::CallFunc (CLR) - .NET method ran synchronously and faulted, marshalling exception data for V8");

                    Marshal.WriteIntPtr(result, MarshalCLRToV8(functionTask.Exception, out _));
                    Marshal.WriteInt32(resultType, (int)V8Type.Exception);
                    break;
                }
                case TaskStatus.RanToCompletion:
                {
                    DebugMessage("CoreCLREmbedding::CallFunc (CLR) - .NET method ran synchronously, marshalling data for V8");

                    IntPtr marshalData = MarshalCLRToV8(functionTask.Result, out var taskResultType);

                    DebugMessage("CoreCLREmbedding::CallFunc (CLR) - Method return data is of type {0}", taskResultType.ToString("G"));

                    Marshal.WriteIntPtr(result, marshalData);
                    Marshal.WriteInt32(resultType, (int)taskResultType);
                    break;
                }
                default:
                {
                    DebugMessage("CoreCLREmbedding::CallFunc (CLR) - .NET method ran asynchronously, returning task handle and status");

                    GCHandle taskHandle = GCHandle.Alloc(functionTask);

                    Marshal.WriteIntPtr(result, GCHandle.ToIntPtr(taskHandle));
                    Marshal.WriteInt32(resultType, (int)V8Type.Task);
                    break;
                }
            }

            DebugMessage("CoreCLREmbedding::CallFunc (CLR) - Finished");
        }

        catch (Exception e)
        {
            DebugMessage("CoreCLREmbedding::CallFunc (CLR) - Exception was thrown: {0}{1}{2}", e.Message, Environment.NewLine, e.StackTrace);

            V8Type v8Type;

            Marshal.WriteIntPtr(result, MarshalCLRToV8(e, out v8Type));
            Marshal.WriteInt32(resultType, (int)v8Type);
            Marshal.WriteInt32(taskState, (int)TaskStatus.Faulted);
        }
    }

    private static void TaskCompleted(Task<object> task, object state)
    {
        DebugMessage("CoreCLREmbedding::TaskCompleted (CLR) - Task completed with a state of {0}", task.Status.ToString("G"));
        DebugMessage("CoreCLREmbedding::TaskCompleted (CLR) - Marshalling data to return to V8", task.Status.ToString("G"));

        V8Type v8Type;
        TaskState actualState = (TaskState)state;
        IntPtr resultObject;
        TaskStatus taskStatus;

        if (task.IsFaulted)
        {
            taskStatus = TaskStatus.Faulted;

            try
            {
                resultObject = MarshalCLRToV8(task.Exception, out v8Type);
            }

            catch (Exception e)
            {
                taskStatus = TaskStatus.Faulted;
                resultObject = MarshalCLRToV8(e, out v8Type);
            }
        }

        else
        {
            taskStatus = TaskStatus.RanToCompletion;

            try
            {
                resultObject = MarshalCLRToV8(task.Result, out v8Type);
            }

            catch (Exception e)
            {
                taskStatus = TaskStatus.Faulted;
                resultObject = MarshalCLRToV8(e, out v8Type);
            }
        }

        DebugMessage("CoreCLREmbedding::TaskCompleted (CLR) - Invoking unmanaged callback");
        actualState.Callback(resultObject, (int)v8Type, (int)taskStatus, actualState.Context);
    }

    [SecurityCritical]
    public static void ContinueTask(IntPtr task, IntPtr context, IntPtr callback, IntPtr exception)
    {
        try
        {
            Marshal.WriteIntPtr(exception, IntPtr.Zero);

            DebugMessage("CoreCLREmbedding::ContinueTask (CLR) - Starting");

            GCHandle taskHandle = GCHandle.FromIntPtr(task);
            Task<Object> actualTask = (Task<Object>)taskHandle.Target;

            TaskCompleteDelegate taskCompleteDelegate = Marshal.GetDelegateForFunctionPointer<TaskCompleteDelegate>(callback);
            DebugMessage("CoreCLREmbedding::ContinueTask (CLR) - Marshalled unmanaged callback successfully");

            actualTask.ContinueWith(TaskCompleted, new TaskState(taskCompleteDelegate, context));

            DebugMessage("CoreCLREmbedding::ContinueTask (CLR) - Finished");
        }

        catch (Exception e)
        {
            DebugMessage("CoreCLREmbedding::ContinueTask (CLR) - Exception was thrown: {0}{1}{2}", e.Message, Environment.NewLine, e.StackTrace);

            Marshal.WriteIntPtr(exception, MarshalCLRToV8(e, out _));
        }
    }

    [SecurityCritical]
    public static void SetCallV8FunctionDelegate(IntPtr callV8Function, IntPtr exception)
    {
        try
        {
            Marshal.WriteIntPtr(exception, IntPtr.Zero);
            NodejsFuncInvokeContext.CallV8Function = Marshal.GetDelegateForFunctionPointer<CallV8FunctionDelegate>(callV8Function);
        }

        catch (Exception e)
        {
            DebugMessage("CoreCLREmbedding::SetCallV8FunctionDelegate (CLR) - Exception was thrown: {0}{1}{2}", e.Message, Environment.NewLine, e.StackTrace);

            Marshal.WriteIntPtr(exception, MarshalCLRToV8(e, out _));
        }
    }

    [SecurityCritical]
    public static void FreeMarshalData(IntPtr marshalData, int v8Type)
    {
        switch ((V8Type)v8Type)
        {
            case V8Type.String:
            case V8Type.Int32:
            case V8Type.Boolean:
            case V8Type.Number:
            case V8Type.Date:
                Marshal.FreeCoTaskMem(marshalData);
                break;

            case V8Type.Object:
            case V8Type.Exception:
                V8ObjectData objectData = Marshal.PtrToStructure<V8ObjectData>(marshalData);

                for (int i = 0; i < objectData.propertiesCount; i++)
                {
                    int propertyType = Marshal.ReadInt32(objectData.propertyTypes, i * sizeof(int));
                    IntPtr propertyValue = Marshal.ReadIntPtr(objectData.propertyValues, i * PointerSize);
                    IntPtr propertyName = Marshal.ReadIntPtr(objectData.propertyNames, i * PointerSize);

                    FreeMarshalData(propertyValue, propertyType);
                    Marshal.FreeCoTaskMem(propertyName);
                }

                Marshal.FreeCoTaskMem(objectData.propertyTypes);
                Marshal.FreeCoTaskMem(objectData.propertyValues);
                Marshal.FreeCoTaskMem(objectData.propertyNames);
                Marshal.FreeCoTaskMem(marshalData);

                break;

            case V8Type.Array:
                V8ArrayData arrayData = Marshal.PtrToStructure<V8ArrayData>(marshalData);

                for (int i = 0; i < arrayData.arrayLength; i++)
                {
                    int itemType = Marshal.ReadInt32(arrayData.itemTypes, i * sizeof(int));
                    IntPtr itemValue = Marshal.ReadIntPtr(arrayData.itemValues, i * PointerSize);

                    FreeMarshalData(itemValue, itemType);
                }

                Marshal.FreeCoTaskMem(arrayData.itemTypes);
                Marshal.FreeCoTaskMem(arrayData.itemValues);
                Marshal.FreeCoTaskMem(marshalData);

                break;

            case V8Type.Buffer:
                V8BufferData bufferData = Marshal.PtrToStructure<V8BufferData>(marshalData);

                Marshal.FreeCoTaskMem(bufferData.buffer);
                Marshal.FreeCoTaskMem(marshalData);

                break;

            case V8Type.Null:
            case V8Type.Function:
                break;

            default:
                throw new Exception("Unsupported marshalled data type: " + v8Type);
        }
    }

    // ReSharper disable once InconsistentNaming
    public static IntPtr MarshalCLRToV8(object clrObject, out V8Type v8Type)
    {
        if (clrObject == null)
        {
            v8Type = V8Type.Null;
            return IntPtr.Zero;
        }

        else if (clrObject is string)
        {
            v8Type = V8Type.String;
            return Marshal.StringToCoTaskMemUTF8((string) clrObject);
        }

        else if (clrObject is char)
        {
            v8Type = V8Type.String;
            return Marshal.StringToCoTaskMemUTF8(clrObject.ToString());
        }

        else if (clrObject is bool)
        {
            v8Type = V8Type.Boolean;
            IntPtr memoryLocation = Marshal.AllocCoTaskMem(sizeof (int));

            Marshal.WriteInt32(memoryLocation, ((bool) clrObject)
                ? 1
                : 0);
            return memoryLocation;
        }

        else if (clrObject is Guid)
        {
            v8Type = V8Type.String;
            return Marshal.StringToCoTaskMemUTF8(clrObject.ToString());
        }

        else if (clrObject is DateTime)
        {
            v8Type = V8Type.Date;
            DateTime dateTime = (DateTime) clrObject;

            if (dateTime.Kind == DateTimeKind.Local)
            {
                dateTime = dateTime.ToUniversalTime();
            }

            else if (dateTime.Kind == DateTimeKind.Unspecified)
            {
                dateTime = new DateTime(dateTime.Ticks, DateTimeKind.Utc);
            }

            long ticks = (dateTime.Ticks - MinDateTimeTicks)/10000;
            IntPtr memoryLocation = Marshal.AllocCoTaskMem(sizeof (double));

            WriteDouble(memoryLocation, ticks);
            return memoryLocation;
        }

        else if (clrObject is DateTimeOffset)
        {
            v8Type = V8Type.String;
            return Marshal.StringToCoTaskMemUTF8(clrObject.ToString());
        }

        else if (clrObject is Uri)
        {
            v8Type = V8Type.String;
            return Marshal.StringToCoTaskMemUTF8(clrObject.ToString());
        }

        else if (clrObject is short)
        {
            v8Type = V8Type.Int32;
            IntPtr memoryLocation = Marshal.AllocCoTaskMem(sizeof (int));

            Marshal.WriteInt32(memoryLocation, Convert.ToInt32(clrObject));
            return memoryLocation;
        }

        else if (clrObject is int)
        {
            v8Type = V8Type.Int32;
            IntPtr memoryLocation = Marshal.AllocCoTaskMem(sizeof (int));

            Marshal.WriteInt32(memoryLocation, (int) clrObject);
            return memoryLocation;
        }

        else if (clrObject is long)
        {
            v8Type = V8Type.Number;
            IntPtr memoryLocation = Marshal.AllocCoTaskMem(sizeof (double));

            WriteDouble(memoryLocation, Convert.ToDouble((long) clrObject));
            return memoryLocation;
        }

        else if (clrObject is double)
        {
            v8Type = V8Type.Number;
            IntPtr memoryLocation = Marshal.AllocCoTaskMem(sizeof (double));

            WriteDouble(memoryLocation, (double) clrObject);
            return memoryLocation;
        }

        else if (clrObject is float)
        {
            v8Type = V8Type.Number;
            IntPtr memoryLocation = Marshal.AllocCoTaskMem(sizeof (double));

            WriteDouble(memoryLocation, Convert.ToDouble((Single) clrObject));
            return memoryLocation;
        }

        else if (clrObject is decimal)
        {
            v8Type = V8Type.String;
            return Marshal.StringToCoTaskMemUTF8(clrObject.ToString());
        }

        else if (clrObject is Enum)
        {
            v8Type = V8Type.String;
            return Marshal.StringToCoTaskMemUTF8(clrObject.ToString());
        }

        else if (clrObject is byte[] || clrObject is IEnumerable<byte>)
        {
            v8Type = V8Type.Buffer;

            V8BufferData bufferData = new V8BufferData();
            byte[] buffer;

            if (clrObject is byte[])
            {
                buffer = (byte[]) clrObject;
            }

            else
            {
                buffer = ((IEnumerable<byte>) clrObject).ToArray();
            }

            bufferData.bufferLength = buffer.Length;
            bufferData.buffer = Marshal.AllocCoTaskMem(buffer.Length*sizeof (byte));

            Marshal.Copy(buffer, 0, bufferData.buffer, bufferData.bufferLength);

            IntPtr destinationPointer = Marshal.AllocCoTaskMem(V8BufferDataSize);
            Marshal.StructureToPtr(bufferData, destinationPointer, false);

            return destinationPointer;
        }

        else if (clrObject is IDictionary || clrObject is ExpandoObject)
        {
            v8Type = V8Type.Object;

            IEnumerable keys;
            int keyCount;
            Func<object, object> getValue;

            if (clrObject is ExpandoObject)
            {
                IDictionary<string, object> objectDictionary = (IDictionary<string, object>) clrObject;

                keys = objectDictionary.Keys;
                keyCount = objectDictionary.Keys.Count;
                getValue = index => objectDictionary[index.ToString()];
            }

            else
            {
                IDictionary objectDictionary = (IDictionary) clrObject;

                keys = objectDictionary.Keys;
                keyCount = objectDictionary.Keys.Count;
                getValue = index => objectDictionary[index];
            }

            V8ObjectData objectData = new V8ObjectData();
            int counter = 0;

            objectData.propertiesCount = keyCount;
            objectData.propertyNames = Marshal.AllocCoTaskMem(PointerSize*keyCount);
            objectData.propertyTypes = Marshal.AllocCoTaskMem(sizeof (int)*keyCount);
            objectData.propertyValues = Marshal.AllocCoTaskMem(PointerSize*keyCount);

            foreach (object key in keys)
            {
                Marshal.WriteIntPtr(objectData.propertyNames, counter*PointerSize, Marshal.StringToCoTaskMemUTF8(key.ToString()));
                V8Type propertyType;
                Marshal.WriteIntPtr(objectData.propertyValues, counter*PointerSize, MarshalCLRToV8(getValue(key), out propertyType));
                Marshal.WriteInt32(objectData.propertyTypes, counter*sizeof (int), (int) propertyType);

                counter++;
            }

            IntPtr destinationPointer = Marshal.AllocCoTaskMem(V8ObjectDataSize);
            Marshal.StructureToPtr(objectData, destinationPointer, false);

            return destinationPointer;
        }

        else if (clrObject is IEnumerable)
        {
            v8Type = V8Type.Array;

            V8ArrayData arrayData = new V8ArrayData();
            List<IntPtr> itemValues = new List<IntPtr>();
            List<int> itemTypes = new List<int>();

            foreach (object item in (IEnumerable) clrObject)
            {
                V8Type itemType;

                itemValues.Add(MarshalCLRToV8(item, out itemType));
                itemTypes.Add((int) itemType);
            }

            arrayData.arrayLength = itemValues.Count;
            arrayData.itemTypes = Marshal.AllocCoTaskMem(sizeof (int)*arrayData.arrayLength);
            arrayData.itemValues = Marshal.AllocCoTaskMem(PointerSize*arrayData.arrayLength);

            Marshal.Copy(itemTypes.ToArray(), 0, arrayData.itemTypes, arrayData.arrayLength);
            Marshal.Copy(itemValues.ToArray(), 0, arrayData.itemValues, arrayData.arrayLength);

            IntPtr destinationPointer = Marshal.AllocCoTaskMem(V8ArrayDataSize);
            Marshal.StructureToPtr(arrayData, destinationPointer, false);

            return destinationPointer;
        }

        else if (clrObject.GetType().GetTypeInfo().IsGenericType && clrObject.GetType().GetGenericTypeDefinition() == typeof (Func<,>))
        {
            Func<object, Task<object>> funcObject = clrObject as Func<object, Task<object>>;

            if (funcObject == null)
            {
                throw new Exception("Properties that return Func<> instances must return Func<object, Task<object>> instances");
            }

            v8Type = V8Type.Function;
            return GCHandle.ToIntPtr(GCHandle.Alloc(funcObject));
        }

        else
        {
            v8Type = clrObject is Exception
                ? V8Type.Exception
                : V8Type.Object;

            if (clrObject is Exception)
            {
                AggregateException aggregateException = clrObject as AggregateException;

                if (aggregateException?.InnerExceptions != null && aggregateException.InnerExceptions.Count > 0)
                {
                    clrObject = aggregateException.InnerExceptions[0];
                }

                else
                {
                    TargetInvocationException targetInvocationException = clrObject as TargetInvocationException;

                    if (targetInvocationException?.InnerException != null)
                    {
                        clrObject = targetInvocationException.InnerException;
                    }
                }
            }

            List<Tuple<string, Func<object, object>>> propertyAccessors = GetPropertyAccessors(clrObject.GetType());
            V8ObjectData objectData = new V8ObjectData();
            int counter = 0;

            objectData.propertiesCount = propertyAccessors.Count;
            objectData.propertyNames = Marshal.AllocCoTaskMem(PointerSize*propertyAccessors.Count);
            objectData.propertyTypes = Marshal.AllocCoTaskMem(sizeof (int)*propertyAccessors.Count);
            objectData.propertyValues = Marshal.AllocCoTaskMem(PointerSize*propertyAccessors.Count);

            foreach (Tuple<string, Func<object, object>> propertyAccessor in propertyAccessors)
            {
                Marshal.WriteIntPtr(objectData.propertyNames, counter*PointerSize, Marshal.StringToCoTaskMemUTF8(propertyAccessor.Item1));

                V8Type propertyType;
                if(clrObject.GetType().FullName.StartsWith("System.Reflection"))
                {
                    propertyType = V8Type.String;
                    Marshal.WriteIntPtr(objectData.propertyValues, counter*PointerSize, Marshal.StringToCoTaskMemUTF8(string.Empty));
                }else
                {
                    Marshal.WriteIntPtr(objectData.propertyValues, counter*PointerSize, MarshalCLRToV8(propertyAccessor.Item2(clrObject), out propertyType));
                }
                Marshal.WriteInt32(objectData.propertyTypes, counter*sizeof (int), (int) propertyType);
                counter++;
            }

            IntPtr destinationPointer = Marshal.AllocCoTaskMem(V8ObjectDataSize);
            Marshal.StructureToPtr(objectData, destinationPointer, false);

            return destinationPointer;
        }
    }

    public static object MarshalV8ToCLR(IntPtr v8Object, V8Type objectType)
    {
        switch (objectType)
        {
            case V8Type.String:
                return Marshal.PtrToStringUTF8(v8Object);

            case V8Type.Object:
                return V8ObjectToExpando(Marshal.PtrToStructure<V8ObjectData>(v8Object));

            case V8Type.Boolean:
                return Marshal.ReadByte(v8Object) != 0;

            case V8Type.Number:
                return ReadDouble(v8Object);

            case V8Type.Date:
                double ticks = ReadDouble(v8Object);
                return new DateTime(Convert.ToInt64(ticks) * 10000 + MinDateTimeTicks, DateTimeKind.Utc);

            case V8Type.Null:
                return null;

            case V8Type.Int32:
                return Marshal.ReadInt32(v8Object);

            case V8Type.UInt32:
                return (uint)Marshal.ReadInt32(v8Object);

            case V8Type.Function:
                NodejsFunc nodejsFunc = new NodejsFunc(v8Object);
                return nodejsFunc.GetFunc();

            case V8Type.Array:
                V8ArrayData arrayData = Marshal.PtrToStructure<V8ArrayData>(v8Object);
                object[] array = new object[arrayData.arrayLength];

                for (int i = 0; i < arrayData.arrayLength; i++)
                {
                    int itemType = Marshal.ReadInt32(arrayData.itemTypes, i * sizeof(int));
                    IntPtr itemValuePointer = Marshal.ReadIntPtr(arrayData.itemValues, i * PointerSize);

                    array[i] = MarshalV8ToCLR(itemValuePointer, (V8Type)itemType);
                }

                return array;

            case V8Type.Buffer:
                V8BufferData bufferData = Marshal.PtrToStructure<V8BufferData>(v8Object);
                byte[] buffer = new byte[bufferData.bufferLength];

                Marshal.Copy(bufferData.buffer, buffer, 0, bufferData.bufferLength);

                return buffer;

            case V8Type.Exception:
                string message = Marshal.PtrToStringUTF8(v8Object);
                return new Exception(message);

            default:
                throw new Exception("Unsupported V8 object type: " + objectType + ".");
        }
    }

    private static unsafe void WriteDouble(IntPtr pointer, double value)
    {
        try
        {
            byte* address = (byte*)pointer;

            if ((unchecked((int)address) & 0x7) == 0)
            {
                *((double*)address) = value;
            }

            else
            {
                byte* valuePointer = (byte*)&value;

                address[0] = valuePointer[0];
                address[1] = valuePointer[1];
                address[2] = valuePointer[2];
                address[3] = valuePointer[3];
                address[4] = valuePointer[4];
                address[5] = valuePointer[5];
                address[6] = valuePointer[6];
                address[7] = valuePointer[7];
            }
        }

        catch (NullReferenceException)
        {
            throw new Exception("Access violation.");
        }
    }

    private static unsafe double ReadDouble(IntPtr pointer)
    {
        try
        {
            byte* address = (byte*)pointer;

            if ((unchecked((int)address) & 0x7) == 0)
            {
                return *((double*)address);
            }

            else
            {
                double value;
                byte* valuePointer = (byte*)&value;

                valuePointer[0] = address[0];
                valuePointer[1] = address[1];
                valuePointer[2] = address[2];
                valuePointer[3] = address[3];
                valuePointer[4] = address[4];
                valuePointer[5] = address[5];
                valuePointer[6] = address[6];
                valuePointer[7] = address[7];

                return value;
            }
        }

        catch (NullReferenceException)
        {
            throw new Exception("Access violation.");
        }
    }

    private static ExpandoObject V8ObjectToExpando(V8ObjectData v8Object)
    {
        ExpandoObject expando = new ExpandoObject();
        IDictionary<string, object> expandoDictionary = expando;

        for (int i = 0; i < v8Object.propertiesCount; i++)
        {
            int propertyType = Marshal.ReadInt32(v8Object.propertyTypes, i * sizeof(int));
            IntPtr propertyNamePointer = Marshal.ReadIntPtr(v8Object.propertyNames, i * PointerSize);
            string propertyName = Marshal.PtrToStringUTF8(propertyNamePointer);
            IntPtr propertyValuePointer = Marshal.ReadIntPtr(v8Object.propertyValues, i * PointerSize);

            expandoDictionary.Add(propertyName, MarshalV8ToCLR(propertyValuePointer, (V8Type)propertyType));
        }

        return expando;
    }
    
    internal static void DebugMessage(string message, params object[] parameters)
    {
        if (DebugMode)
        {
            Console.WriteLine(message, parameters);
        }
    }

    private static List<Tuple<string, Func<object, object>>> GetPropertyAccessors(Type type)
    {
        if (TypePropertyAccessors.ContainsKey(type))
        {
            return TypePropertyAccessors[type];
        }

        List<Tuple<string, Func<object, object>>> propertyAccessors = new List<Tuple<string, Func<object, object>>>();

        foreach (PropertyInfo propertyInfo in type.GetProperties(BindingFlags.Instance | BindingFlags.Public))
        {
            ParameterExpression instance = Expression.Parameter(typeof(object));
            UnaryExpression instanceConvert = Expression.TypeAs(instance, type);
            MemberExpression property = Expression.Property(instanceConvert, propertyInfo);
            UnaryExpression propertyConvert = Expression.TypeAs(property, typeof(object));

            propertyAccessors.Add(new Tuple<string, Func<object, object>>(propertyInfo.Name, (Func<object, object>)Expression.Lambda(propertyConvert, instance).Compile()));
        }

        foreach (FieldInfo fieldInfo in type.GetFields(BindingFlags.Instance | BindingFlags.Public))
        {
            ParameterExpression instance = Expression.Parameter(typeof(object));
            UnaryExpression instanceConvert = Expression.TypeAs(instance, type);
            MemberExpression field = Expression.Field(instanceConvert, fieldInfo);
            UnaryExpression fieldConvert = Expression.TypeAs(field, typeof(object));

            propertyAccessors.Add(new Tuple<string, Func<object, object>>(fieldInfo.Name, (Func<object, object>)Expression.Lambda(fieldConvert, instance).Compile()));
        }

        if (typeof(Exception).IsAssignableFrom(type) && !propertyAccessors.Any(a => a.Item1 == "Name"))
        {
            propertyAccessors.Add(new Tuple<string, Func<object, object>>("Name", o => type.FullName));
        }

        TypePropertyAccessors[type] = propertyAccessors;

        return propertyAccessors;
    }
}
