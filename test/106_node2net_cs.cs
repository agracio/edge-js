using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.IO;
using System.Runtime.InteropServices;

public class Startup
{
    public async Task<object> Test(dynamic input)
    {
        return Edge.Tests.TestNodeToNet.Test();
    }

}
