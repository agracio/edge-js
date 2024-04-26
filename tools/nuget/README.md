## About

### With Edge.js you can script Node.js in a .NET and .NET in Node.js.  

Edge.js allows you to run Node.js and .NET code in one process. You can call Node.js functions from .NET and .NET functions from Node.js.  
Edge.js takes care of marshaling data between CLR and V8. Edge.js also reconciles threading models of single-threaded V8 and multi-threaded CLR. Edge.js ensures correct lifetime of objects on V8 and CLR heaps.  

Script CLR from Node.Js on Windows/Linux/macOS with .NET Framework, .Net Core and .NET Standard.

**NOTE** Scripting Node.Js from CLR is only supported on Windows .NET Framework 4.5

More documentation is available at [Edge.Js GitHub page](https://github.com/agracio/edge-js).

#### NuGet package compiled using Node.js v20.12.2. 

### How to use

```cs
using System;
using System.Threading.Tasks;
using EdgeJs;

class Program
{
    public static async Task Start()
    {
        var func = Edge.Func(@"
            return function (data, callback) {
                callback(null, 'Node.js welcomes ' + data);
            }
        ");

        Console.WriteLine(await func(".NET"));
    }

    static void Main(string[] args)
    {
        Start().Wait();
    }
}

```