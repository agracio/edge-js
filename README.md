# Edge.js: .NET and Node.js in-process
<!---[![Build Status](https://app.travis-ci.com/agracio/edge-js.svg?branch=master)](https://app.travis-ci.com/github/agracio/edge-js)--->
[![Build status][appveyor-image]][appveyor-url]
[![CircleCI](https://dl.circleci.com/status-badge/img/gh/agracio/edge-js.svg?style=shield )](https://dl.circleci.com/status-badge/redirect/gh/agracio/edge-js/tree/master)
[![Actions Status][github-img]][github-url]
[![Git Issues][issues-img]][issues-url]
[![Closed Issues][closed-issues-img]][closed-issues-url]
<!-- [![NPM Downloads][downloads-img]][downloads-url] -->
<!-- ![Node version](https://img.shields.io/node/v/edge-js.svg) -->
<!-- [![deps status][dependencies-img]][dependencies-url] -->
<!--[![MIT license][license-img]][license-url] -->
<!-- [![Codacy Badge][codacy-img]][codacy-url] -->

-----
### This library is based on https://github.com/tjanczuk/edge all credit for original work goes to Tomasz Janczuk. 
------

## Overview

**Edge.js allows you to run Node.js and .NET code in one process on Windows, macOS, and Linux**

You can call .NET functions from Node.js and Node.js functions from .NET.  
Edge.js takes care of marshaling data between CLR and V8. Edge.js also reconciles threading models of single-threaded V8 and multi-threaded CLR.  
Edge.js ensures correct lifetime of objects on V8 and CLR heaps.  
The CLR code can be pre-compiled or specified as C#, F#, Python (IronPython), or PowerShell source: Edge.js can compile CLR scripts at runtime.  
Edge can be extended to support other CLR languages or DSLs.

![Edge.js interop model](https://f.cloud.github.com/assets/822369/234085/b305625c-8768-11e2-8de0-e03ae98e7249.PNG)

Edge.js provides an asynchronous, in-process mechanism for interoperability between Node.js and .NET. You can use this mechanism to:

* script Node.js from a .NET application on Windows using .NET Framework
* script C# from a Node.js application on Windows, macOS, and Linux using .NET Framework/.NET Core
* use CLR multi-threading from Node.js for CPU intensive work [more...](http://tomasz.janczuk.org/2013/02/cpu-bound-workers-for-nodejs.html)  
* write native extensions to Node.js in C# instead of C/C++  
* integrate existing .NET components into Node.js applications
* access MS SQL from Node.js using ADO.NET 
* script F# from Node.js
* script Powershel from Node.js
* script Python (IronPython) from Node.js

Read more about the background and motivations of the project [here](http://tomasz.janczuk.org/2013/02/hosting-net-code-in-nodejs-applications.html). 

## Updates
* Support for new versions of Node.Js.
* Support for .NET Core 3.1 - 8.x on Windows/Linux/macOS.
* Fixes AccessViolationException when running Node.js code from C# [PR #573](https://github.com/tjanczuk/edge/pull/573).
* Fixes StackOverflowException [PR #566](https://github.com/tjanczuk/edge/pull/566) that occurs when underlying C# code throws complex exception.
* Fixes issues [#469](https://github.com/tjanczuk/edge/issues/469), [#713](https://github.com/tjanczuk/edge/issues/713)
* Other PRs: [PR #725](https://github.com/tjanczuk/edge/pull/725), [PR #640](https://github.com/tjanczuk/edge/pull/640)
* Multiple bug fixes and improvements to the original code.

----
### NPM package [`edge-js`](https://www.npmjs.com/package/edge-js)

### NuGet package [`EdgeJs`](https://www.nuget.org/packages/EdgeJs)
----

## Electron

For use with Electron **[`electron-edge-js`](https://github.com/agracio/electron-edge-js)**

## VS Code extensions

VS Code uses Electron shell, to write extensions for it using Edge.js use 
**[`electron-edge-js`](https://github.com/agracio/electron-edge-js)**

## Quick start

Sample app that shows how to work with .NET Core using inline code and compiled C# libraries.  
https://github.com/agracio/edge-js-quick-start

## Node.Js Support

### Windows

| Version | x86/x64            | arm64              |
| ------- | ------------------ | ------------------ |
| 16.x    | :heavy_check_mark: | :x:                |
| 18.x    | :heavy_check_mark: | :x:                |
| 20.x    | :heavy_check_mark: | :heavy_check_mark: |
| 22.x    | :heavy_check_mark: | :heavy_check_mark: |


### macOS

| Version        | x64                | arm64 (M1+)        |
| -------------- | ------------------ | ------------------ |
| 16.x - 22.x    | :heavy_check_mark: | :heavy_check_mark: |

### Linux

| Version        | x64                | arm64              |
| -------------- | ------------------ | ------------------ |
| 14.x - 22.x    | :heavy_check_mark: | :heavy_check_mark: |

## Scripting CLR from Node.js and Node.js from CRL 

<table>
<tr><th>Script CLR from Node.js </th><th>Script Node.js from CLR</th></tr>
<tr><td>

|         | .NET 4.5           | Mono 6.x           | CoreCLR            |
| ------- | ------------------ | ------------------ | ------------------ |
| Windows | :heavy_check_mark: | :heavy_check_mark: | :heavy_check_mark: |
| Linux   | :x:                | :x:                | :heavy_check_mark: |
| macOS   | :x:                | :heavy_check_mark: | :heavy_check_mark: |

</td><td>

|         | .NET 4.5           | Mono       | CoreCLR       |
| ------- | ------------------ | ---------- | ------------- |
| Windows | :heavy_check_mark: | :x:        | :x:           |
| Linux   | :x:                | :x:        | :x:           |
| macOS   | :x:                | :x:        | :x:           |

</td></tr> </table>

## Mono

Mono is no longer actively supported. Existing code will remain In Edge.Js but focus will be on .NET Core.  
Mono tests are excluded from CI.

## Node.js application packaging

When packaging your application using Webpack make sure that `edge-js` is specified as external module.

### Webpack
```js
  externals: {
    'edge-js': 'commonjs2 edge-js',
  },
  node: {
    __dirname: true,
    __filename: true,
  },
```

### Next.js

`next.config.mjs`
```js
  experimental: {
    serverComponentsExternalPackages: ['edge-js'],
  },
```

## Additional languages support

### Python (IronPython) scripting

**NOTE** This functionality requires IronPython 3.4

| Framework   | Platform      | NPM Package  | Language code | Documentation |
| ----------- | ------------  | ------------ |-------------- | ------------- |
| .NET 4.5    | Windows       | `edge-py`    | `py` | [Script Python in Node.js](https://github.com/agracio/edge-py) :link: |
| CoreCLR     | Any           | `edge-py`    | `py` | [Script Python in Node.js](https://github.com/agracio/edge-py) :link: |

### PowerShell scripting

**NOTE** CoreCLR requires dotnet 8

| Framework   | Platform      | NPM Package | Language code | Documentation |
| ----------- | ------------  | ----------- |-------------- | ------------- |
| .NET 4.5    | Windows       | `edge-ps`   | `ps` | [Script PowerShell in Node.js](https://github.com/agracio/edge-ps) :link: |
| CoreCLR     | Windows       | `edge-ps`   | `ps` | [Script PowerShell in Node.js](https://github.com/agracio/edge-ps) :link: |

### MS SQL scripting

Provides simple access to MS SQL without the need to write separate C# code.     

| Framework     | Platform      | NPM Package | Language code | Documentation |
| ------------- | ------------  | ----------- |-------------- | ------------- |
| .NET 4.5      | Windows       | `edge-sql`  | `sql`| [Script T-SQL in Node.js](https://github.com/agracio/edge-sql) :link: |
| CoreCLR       | Any           | `edge-sql`  | `sql`| [Script T-SQL in Node.js](https://github.com/agracio/edge-sql) :link: |

### F# scripting

| Framework   | Platform      | NPM Package | Language code | Documentation |
| ----------- | ------------  | ----------- |-------------- | ------------- |
| .NET 4.5    | Windows       | `edge-fs`   | `fs`          | [Script F# in Node.js](https://github.com/agracio/edge-fs) :link: |
| CoreCLR     | Windows       | `edge-fs`   | `fs`          | [Script F# in Node.js](https://github.com/agracio/edge-fs) :link: |

---------

## How to use

#### [Scripting CLR from Node.js](#scripting-clr-from-nodejs) - full documentation
#### [Scripting Node.js from CLR](#how-to-integrate-nodejs-code-into-clr-code) - full documentation

#### Scripting CLR from Node.js sample app https://github.com/agracio/edge-js-quick-start
----

### Scripting CLR from Node.js examples

### Inline C# code 

#### ES5

```js
var edge = require('edge-js');

var helloWorld = edge.func(function () {/*
    async (input) => { 
        return ".NET Welcomes " + input.ToString(); 
    }
*/});

helloWorld('JavaScript', function (error, result) {
    if (error) throw error;
    console.log(result);
});
```

#### ES6 with templated strings

```js
var edge = require('edge-js');

var helloWorld = edge.func(`
    async (input) => { 
        return ".NET Welcomes " + input.ToString(); 
    }
`);

helloWorld('JavaScript', function (error, result) {
    if (error) throw error;
    console.log(result);
});
```

#### CoreCLR
* If not set Edge.js will run as .NET 4.5 on Windows and as Mono on macOS/Linux
* Can be set using `js` code below or as an environment variable `SET EDGE_USE_CORECLR=1`
* Check [appveyor.yml](https://github.com/agracio/edge-js/blob/master/appveyor.yml) `test_script` for reference on setting env variables
* Must be set before `var edge = require('edge-js');`

```js
// set this variable before 
// var edge = require('edge-js');

process.env.EDGE_USE_CORECLR=1

var edge = require('edge-js');

var helloWorld = edge.func(function () {/*
    async (input) => { 
        return ".NET Welcomes " + input.ToString(); 
    }
*/});

helloWorld('JavaScript', function (error, result) {
    if (error) throw error;
    console.log(result);
});
```

### Passing parameters

```js
var edge = require('edge-js');

var helloWorld = edge.func(function () {/*
    async (dynamic input) => { 
        return "Welcome " + input.name + " " + input.surname; 
    }
*/});

helloWorld({name: 'John', surname: 'Smith'}, function (error, result) {
    if (error) throw error;
    console.log(result);
});
```

### Using C# class

```js
var getPerson = edge.func({
    source: function () {/* 
        using System.Threading.Tasks;
        using System;

        public class Person
        {
            public Person(string name, string email, int age)
            {
                Id =  Guid.NewGuid();
                Name = name;
                Email = email;
                Age = age;
            }
            public Guid Id {get;set;}
            public string Name {get;set;}
            public string Email {get;set;}
            public int Age {get;set;}
        }

        public class Startup
        {
            public async Task<object> Invoke(dynamic input)
            {
                return new Person(input.name, input.email, input.age);
            }
        }
    */}
});

getPerson({name: 'John Smith', email: 'john.smith@myemailprovider', age: 35}, function(error, result) {
    if (error) throw error;
    console.log(result);
});
```

**When using inline C# class code must include**

```cs
public class Startup
{
    public async Task<object> Invoke(object|dynamic input)
    {
        // code
        // return results
    }
}
```

### Using compiled assembly

```cs
// People.cs

using System;

namespace People
{
    public class Person
    {
        public Person(string name, string email, int age)
        {
            Id =  Guid.NewGuid();
            Name = name;
            Email = email;
            Age = age;
        }
        public Guid Id {get;}
        public string Name {get;}
        public string Email {get;}
        public int Age {get;}
    }
}

// EdgeJsMethods.cs

using System.Threading.Tasks;
using People;

namespace EdgeJsMethods
{
    class Methods
    {
        public async Task<object> GetPerson(dynamic input)
        {
            return await Task.Run(() => new Person(input.name, input.email, input.age));
        }
    }
}
```

```js

var edge = require('edge-js');

var getPerson = edge.func({
    assemblyFile: myDll, // path to .dll
    typeName: 'EdgeJsMethods.Methods',
    methodName: 'GetPerson'
});

getPerson({name: 'John Smith', email: 'john.smith@myemailprovider', age: 35}, function(error, result) {
    if (error) throw error;
    console.log(result);
});

```

### Edge.js C# method must have the following signature

```cs
public async Task<object> MyMethod(object|dynamic input)
{
    //return results sync/async;
}
```

### Executing synchronously without function callback

If your C# implementation will complete synchronously you can call this function as any synchronous JavaScript function as follows:

```js
var edge = require('edge-js');

var helloWorld = edge.func(function () {/*
    async (input) => { 
        return ".NET Welcomes " + input.ToString(); 
    }
*/});

var result = helloWorld('JavaScript', true);
```

Calling C# asynchronous implementation as a synchronous JavaScript function will fail 

```js
var edge = require('edge-js');

var helloWorld = edge.func(function () {/*
    async (input) => { 
        return await Task.Run(() => ".NET Welcomes " + input.ToString());
    }
*/});

// sync call will throw exception
var result = helloWorld('JavaScript', true);
```
----

### Scripting Node.js from CLR example

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
### More examples in tests [DoubleEdge.cs](https://github.com/agracio/edge-js/blob/master/test/double/double_test/DoubleEdge.cs) 
----  

### Docker

**Dockerfile: [Dockerfile](https://github.com/agracio/edge-js/blob/master/Dockerfile)**  
**Docker Hub image: [agracio/ubuntu-node-netcore](https://hub.docker.com/repository/docker/agracio/ubuntu-node-netcore)**

* Based on Ununtu 22.04
* User directory **`devvol`**

#### Pre-installed packages

* Node.js 20
* dotnet 8
* git
* build tools
* sudo, curl, wget
* node-gyp

#### Using container

* Run interactive starting in `devvol`, set `EDGE_USE_CORECLR=1` at container level
* Git clone `edge-js` and enter cloned repo directory
* `npm install`
* Run tests

```
docker run -w /devvol -e EDGE_USE_CORECLR=1 -it agracio/ubuntu-node-netcore:latest
git clone https://github.com/agracio/edge-js.git && cd edge-js
npm i
npm test
```

---------

<br/>Edge.js readme
==============================

### :exclamation: Some of the original Edge.js documentation is outdated :exclamation:  
 
## What problems does Edge.js solve?

> Ah, whatever problem you have. If you have this problem, this solves it.

*[--Scott Hanselman (@shanselman)](https://twitter.com/shanselman/status/461532471037677568)*

## Before you dive in

Read the [Edge.js introduction on InfoQ](http://www.infoq.com/articles/the_edge_of_net_and_node).  
Listen to the [Edge.js podcast on Herdingcode](http://herdingcode.com/herding-code-166-tomasz-janczuk-on-edge-js/). 

## Contents

[Scripting CLR from Node.js](#scripting-clr-from-nodejs)  
&nbsp;&nbsp;&nbsp;&nbsp;[What you need](#what-you-need)  
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;[Windows](#windows)  
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;[Linux](#linux)  
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;[OSX](#osx)  
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;[Docker](#docker)  
&nbsp;&nbsp;&nbsp;&nbsp;[How to: C# hello, world](#how-to-c-hello-world)  
&nbsp;&nbsp;&nbsp;&nbsp;[How to: integrate C# code into Node.js code](#how-to-integrate-c-code-into-nodejs-code)  
&nbsp;&nbsp;&nbsp;&nbsp;[How to: specify additional CLR assembly references in C# code](#how-to-specify-additional-clr-assembly-references-in-c-code)  
&nbsp;&nbsp;&nbsp;&nbsp;[How to: marshal data between C# and Node.js](#how-to-marshal-data-between-c-and-nodejs)  
&nbsp;&nbsp;&nbsp;&nbsp;[How to: call Node.js from C#](#how-to-call-nodejs-from-c)  
&nbsp;&nbsp;&nbsp;&nbsp;[How to: export C# function to Node.js](#how-to-export-c-function-to-nodejs)  
&nbsp;&nbsp;&nbsp;&nbsp;[How to: script Python in a Node.js application](#how-to-script-python-in-a-nodejs-application)  
&nbsp;&nbsp;&nbsp;&nbsp;[How to: script PowerShell in a Node.js application](#how-to-script-powershell-in-a-nodejs-application)  
&nbsp;&nbsp;&nbsp;&nbsp;[How to: script F# in a Node.js application](#how-to-script-f-in-a-nodejs-application)  
&nbsp;&nbsp;&nbsp;&nbsp;[How to: script Lisp in a Node.js application](#how-to-script-lisp-in-a-nodejs-application)  
&nbsp;&nbsp;&nbsp;&nbsp;[How to: script T-SQL in a Node.js application](#how-to-script-t-sql-in-a-nodejs-application)  
&nbsp;&nbsp;&nbsp;&nbsp;[How to: support for other CLR languages](#how-to-support-for-other-clr-languages)  
&nbsp;&nbsp;&nbsp;&nbsp;[How to: exceptions](#how-to-exceptions)  
&nbsp;&nbsp;&nbsp;&nbsp;[How to: app.config](#how-to-appconfig)  
&nbsp;&nbsp;&nbsp;&nbsp;[How to: debugging](#how-to-debugging)  
&nbsp;&nbsp;&nbsp;&nbsp;[Performance](#performance)  
&nbsp;&nbsp;&nbsp;&nbsp;[Building on Windows](#building-on-windows)  
&nbsp;&nbsp;&nbsp;&nbsp;[Building on OSX](#building-on-osx)  
&nbsp;&nbsp;&nbsp;&nbsp;[Building on Linux](#building-on-linux)  
&nbsp;&nbsp;&nbsp;&nbsp;[Running tests](#running-tests)  
[Scripting Node.js from CLR](#scripting-nodejs-from-clr)  
&nbsp;&nbsp;&nbsp;&nbsp;[What you need](#what-you-need-1)  
&nbsp;&nbsp;&nbsp;&nbsp;[How to: Node.js hello, world](#how-to-nodejs-hello-world)  
&nbsp;&nbsp;&nbsp;&nbsp;[How to: integrate Node.js into CLR code](#how-to-integrate-nodejs-code-into-clr-code)  
&nbsp;&nbsp;&nbsp;&nbsp;[How to: use Node.js built-in modules](#how-to-use-nodejs-built-in-modules)  
&nbsp;&nbsp;&nbsp;&nbsp;[How to: use external Node.js modules](#how-to-use-external-nodejs-modules)  
&nbsp;&nbsp;&nbsp;&nbsp;[How to: handle Node.js events in .NET](#how-to-handle-nodejs-events-in-net)  
&nbsp;&nbsp;&nbsp;&nbsp;[How to: expose Node.js state to .NET](#how-to-expose-nodejs-state-to-net)  
&nbsp;&nbsp;&nbsp;&nbsp;[How to: use Node.js in ASP.NET application](#how-to-use-nodejs-in-aspnet-web-applications)  
&nbsp;&nbsp;&nbsp;&nbsp;[How to: debug Node.js code running in a CLR application](#how-to-debug-nodejs-code-running-in-a-clr-application)  
&nbsp;&nbsp;&nbsp;&nbsp;[Building Edge.js NuGet package](#building-edgejs-nuget-package)  
&nbsp;&nbsp;&nbsp;&nbsp;[Running tests of scripting Node.js in C#](#running-tests-of-scripting-nodejs-in-c)  
[Use cases and other resources](#use-cases-and-other-resources)  
[Contribution and derived work](#contribution-and-derived-work)  

## Scripting CLR from Node.js

If you are writing a Node.js application, this section explains how you include and run CLR code in your app. It works on Windows, MacOS, and Linux.

### What you need

Edge.js runs on Windows, Linux, and OSX and requires supported version of Node.js 8.x, 7.x, 6.x, as well as .NET Framework 4.5 (Windows), Mono 4.2.4 (OSX, Linux), or .NET Core 1.0.0 Preview 2 (Windows, OSX, Linux). 

**NOTE** there is a known issue with Mono after 4.2.4 that will be addressed in Mono 4.6.

#### Windows

* Supported Node.js version
* [.NET 4.6.2](https://dotnet.microsoft.com/en-us/download/dotnet-framework/net462) and/or [.NET Core](https://www.microsoft.com/net/core)
* to use Python, you also need [IronPython 3.4 or later](https://ironpython.net/download/)  
* to use F#, read [Dave Thomas blog post](https://web.archive.org/web/20160323224525/http://7sharpnine.com/posts/i-node-something/)

If you have both desktop CLR and .NET Core installed, read [using .NET Core](#using-net-core) for how to configure Edge to use one or the other. 

#### Linux

* Supported Node.js version  
* .NET Core
* Follow [Linux setup instructions](#building-on-linux)

#### OSX  

* Supported Node.js version  
* Mono 4.x - 6.x and/or .NET Core
* Follow [OSX setup instructions](#building-on-osx)  

### How to: C# hello, world

Follow setup instructions [for your platform](#what-you-need). 

Install edge:

```
npm install edge-js
```

In your server.js:

```javascript
var edge = require('edge-js');

var helloWorld = edge.func(function () {/*
    async (input) => { 
        return ".NET Welcomes " + input.ToString(); 
    }
*/});

helloWorld('JavaScript', function (error, result) {
    if (error) throw error;
    console.log(result);
});
```

Run and enjoy:

```
$>node server.js
.NET welcomes JavaScript
```

If you want to use .NET Core as your runtime and are running in a dual runtime environment (i.e. Windows with .NET 4.5 installed as well or Linux with Mono installed), you will need to tell edge to use .NET Core by setting the `EDGE_USE_CORECLR` environment variable:

```
$>EDGE_USE_CORECLR=1 node server.js
.NET Welcomes JavaScript
```

### How to: integrate C# code into Node.js code

Edge provides several ways to integrate C# code into a Node.js application. Regardless of the way you choose, the entry point into the .NET code is normalized to a `Func<object,Task<object>>` delegate. This allows Node.js code to call .NET asynchronously and avoid blocking the Node.js event loop. 

Edge provides a function that accepts a reference to C# code in one of the supported representations, and returns a Node.js function which acts as a JavaScript proxy to the `Func<object,Task<object>>` .NET delegate:

```javascript
var edge = require('edge-js');

var myFunction = edge.func(...);
```

The function proxy can then be called from Node.js like any asynchronous function:

```javascript
myFunction('Some input', function (error, result) {
    //...
});
```

Alternatively, if you know the C# implementation will complete synchronously given the circumstances, you can call this function as any synchronous JavaScript function as follows:

```javascript
var result = myFunction('Some input', true);
```

The `true` parameter instead of a callback indicates that Node.js expects the C# implementation to complete synchronously. If the CLR function implementation does not complete synchronously, the call above will result in an exception. 

One representation of CLR code that Edge.js accepts is C# source code. You can embed C# literal representing a .NET async lambda expression implementing the `Func<object, Task<object>>` delegate directly inside Node.js code:

```javascript
var add7 = edge.func('async (input) => { return (int)input + 7; }');
``` 

In another representation, you can embed multi-line C# source code by providing a function with a body containing a multi-line comment. Edge extracts the C# code from the function body using regular expressions:

```javascript
var add7 = edge.func(function() {/*
    async (input) => {
        return (int)input + 7;
    }
*/});
```

Or if you use ES6 you can use [template strings](https://developer.mozilla.org/de/docs/Web/JavaScript/Reference/template_strings) to define a multiline string:

```javascript
var add7 = edge.func(`
    async (input) => {
        return (int)input + 7;
    }
`);
```

If your C# code is more involved than a simple lambda, you can specify entire class definition. By convention, the class must be named `Startup` and it must have an `Invoke` method that matches the `Func<object,Task<object>>` delegate signature. This method is useful if you need to factor your code into multiple methods:

```javascript
var add7 = edge.func(function() {/*
    using System.Threading.Tasks;

    public class Startup
    {
        public async Task<object> Invoke(object input)
        {
            int v = (int)input;
            return Helper.AddSeven(v);
        }
    }

    static class Helper
    {
        public static int AddSeven(int v) 
        {
            return v + 7;
        }
    }
*/});
```

If your C# code grows substantially, it is useful to keep it in a separate file. You can save it to a file with `*.csx` or `*.cs` extension, and then reference from your Node.js application:

```javascript
var add7 = edge.func(require('path').join(__dirname, 'add7.csx'));
```

If you integrate C# code into your Node.js application by specifying C# source using one of the methods above, edge will compile the code on the fly. If you prefer to pre-compile your C# sources to a CLR assembly, or if your C# component is already pre-compiled, you can reference a CLR assembly from your Node.js code. In the most generic form, you can specify the assembly file name, the type name, and the method name when creating a Node.js proxy to a .NET method:

```javascript
var clrMethod = edge.func({
    assemblyFile: 'My.Edge.Samples.dll',
    typeName: 'Samples.FooBar.MyType',
    methodName: 'MyMethod' // This must be Func<object,Task<object>>
});
```

If you don't specify methodName, `Invoke` is assumed. If you don't specify typeName, a type name is constructed by assuming the class called `Startup` in the namespace equal to the assembly file name (without the `.dll`). In the example above, if typeName was not specified, it would default to `My.Edge.Samples.Startup`.

The assemblyFile is relative to the working directory. If you want to locate your assembly in a fixed location relative to your Node.js application, it is useful to construct the assemblyFile using `__dirname`.  If you are using .NET Core, assemblyFile can also be a project name or NuGet package name that is specified in your `project.json` or `.deps.json` dependency manifest.

You can also create Node.js proxies to .NET functions specifying just the assembly name as a parameter:

```javascript
var clrMethod = edge.func('My.Edge.Samples.dll');
```

In that case the default typeName of `My.Edge.Samples.Startup` and methodName of `Invoke` is assumed as explained above. 

### How to: specify additional CLR assembly references in C# code

When you provide C# source code and let edge compile it for you at runtime, edge will by default reference only mscorlib.dll and System.dll assemblies.  If you're using .NET Core, we automatically reference the most recent versions of the System. Runtime, System.Threading.Tasks, and the compiler language packages, like Microsoft.CSharp. In applications that require additional assemblies you can specify them in C# code using a special hash pattern, similar to Roslyn. For example, to use ADO.NET you must reference System.Data.dll:

#### NOTE: `#r` and `references: [ 'MyDll.dll' ]` references only work when using .NET Framework 4.5

```javascript
var add7 = edge.func(function() {/*

    #r "System.Data.dll"

    using System.Data;
    using System.Threading.Tasks;

    public class Startup
    {
        public async Task<object> Invoke(object input)
        {
            // ...
        }
    }
*/});
```

If you prefer, instead of using comments you can specify references by providing options to the `edge.func` call:

```javascript
var add7 = edge.func({
    source: function() {/*

        using System.Data;
        using System.Threading.Tasks;

        public class Startup
        {
            public async Task<object> Invoke(object input)
            {
                // ...
            }
        }
    */},
    references: [ 'System.Data.dll' ]
});
```

If you are using .NET Core and are using the .NET Core SDK and CLI, you must have a `project.json` file (specification [here](https://github.com/aspnet/Home/wiki/Project.json-file)) that specifies the dependencies for the application.  This list of dependencies must also include the [Edge.js runtime package](https://www.nuget.org/packages/EdgeJs/) and, if you need to be able to dynamically compile your code, the package(s) for the compilers that you plan to use, like [Edge.js.CSharp](https://www.nuget.org/packages/Edge.js.CSharp/).  You must have run the `dotnet restore` (to restore the dependencies) and `dotnet build` (to build your project and generate the dependency manifest) commands in that project's directory to generate a `.deps.json` file under `bin/[configuration]/[framework]`, i.e. `bin/Release/netstandard1.6/MyProject.deps.json`.  This `.deps.json` file must either be in the current working directory that `node` is executed in or you must specify its directory by setting the `EDGE_APP_ROOT` environment variable.  For example, if for a `netstandard1.6` project in the `c:\DotNet\MyProject` directory, you would run something like:

```
set EDGE_APP_ROOT=c:\DotNet\MyProject\bin\Release\netstandard1.6
node app.js
```

Edge.js also supports running published .NET Core applications on servers that do not have the .NET Core SDK and CLI installed, which is a common scenario in production environments.  To do so, the `.csproj` for your application should meet the following requirements:

 1. It should target the `netcoreapp2.x` or `netstandard2.0` framework moniker.
 2. It should reference `Microsoft.NETCore.DotNetHost` and `Microsoft.NETCore.DotNetHostPolicy`.  This is required so that the publish process can provide all the native libraries required to create a completely standalone version of your application.
 3. `<PreserveCompilationContext>true</PreserveCompilationContext>` and `<CopyLocalLockFileAssemblies>true</CopyLocalLockFileAssemblies>` should be present under `<PropertyGroup>`.  You can add an empty `Main()` implementation to your project to accommodate it; this method will not be called, but is just a requirement in order for `dotnet publish` to generate a completely standalone app.

On your development machine, you would run `dotnet publish -r [target runtime for your production server]` (i.e. `dotnet publish -r ubuntu.14.04-x64`) to aggregate the package assemblies and native libraries necessary to run your application.  You can copy the contents of the publish directory up to your SDK- and CLI-less server and use them directly in Edge.js by setting the  `EDGE_APP_ROOT` environment variable to the directory on the server that you copied the published application to.

### How to: marshal data between C# and Node.js

Edge.js can marshal any JSON-serializable value between .NET and Node.js (although JSON serialization is not used in the process). Edge also supports marshalling between Node.js `Buffer` instance and a CLR `byte[]` array to help you efficiently pass binary data.

You can call .NET from Node.js and pass in a complex JavaScript object as follows:

```javascript
var dotNetFunction = edge.func('Edge.Sample.dll');

var payload = {
    anInteger: 1,
    aNumber: 3.1415,
    aString: 'foo',
    aBoolean: true,
    aBuffer: new Buffer(10),
    anArray: [ 1, 'foo' ],
    anObject: { a: 'foo', b: 12 }
};

dotNetFunction(payload, function (error, result) { });
```

In .NET, JavaScript objects are represented as dynamics (which can be cast to `IDictionary<string,object>` if desired), JavaScript arrays as `object[]`, and JavaScript `Buffer` as `byte[]`. Scalar JavaScript values have their corresponding .NET types (`int`, `double`, `bool`, `string`). Here is how you can access the data in .NET:

```c#
using System.Threading.Tasks;

public class Startup
{
    public async Task<object> Invoke(dynamic input)
    {
        int anInteger = (int)input.anInteger;
        double aNumber = (double)input.aNumber;
        string aString = (string)input.aString;
        bool aBoolean = (bool)input.aBoolean;
        byte[] aBuffer = (byte[])input.aBuffer;
        object[] anArray = (object[])input.anArray;
        dynamic anObject = (dynamic)input.anObject;

        return null;
    }
}

```

Similar type marshalling is applied when .NET code passes data back to Node.js code. In .NET code you can provide an instance of any CLR type that would normally be JSON serializable, including domain specific types like `Person` or anonymous objects. For example:

```c#
using System.Threading.Tasks;

public class Person
{
    public int anInteger = 1;
    public double aNumber = 3.1415;
    public string aString = "foo";
    public bool aBoolean = true;
    public byte[] aBuffer = new byte[10];
    public object[] anArray = new object[] { 1, "foo" };
    public object anObject = new { a = "foo", b = 12 };
}

public class Startup
{
    public async Task<object> Invoke(dynamic input)
    {
        Person person = new Person();
        return person;
    }
}
```

In your Node.js code that invokes this .NET method you can display the result object that the callback method receives:

```javascript
var edge = require('edge-js');

var getPerson = edge.func(function () {/*
    using System.Threading.Tasks;

    public class Person
    {
        public int anInteger = 1;
        public double aNumber = 3.1415;
        public string aString = "foo";
        public bool aBoolean = true;
        public byte[] aBuffer = new byte[10];
        public object[] anArray = new object[] { 1, "foo" };
        public object anObject = new { a = "foo", b = 12 };
    }

    public class Startup
    {
        public async Task<object> Invoke(dynamic input)
        {
            Person person = new Person();
            return person;
        }
    }
*/});

getPerson(null, function (error, result) {
    if (error) throw error;
    console.log(result);
});
```

Passing this .NET object to Node.js generates a JavaScript object as follows:

```
$>node sample.js
{ anInteger: 1,
  aNumber: 3.1415,
  aString: 'foo',
  aBoolean: true,
  aBuffer: <Buffer 00 00 00 00 00 00 00 00 00 00>,
  anArray: [ 1, 'foo' ],
  anObject: { a: 'foo', b: 12 } }
```

When data is marshalled from .NET to Node.js, no checks for circular references are made. They will typically result in stack overflows. Make sure the object graph you are passing from .NET to Node.js is a tree and does not contain any cycles. 

**WINDOWS ONLY** When marshalling strongly typed objects (e.g. Person) from .NET to Node.js, you can optionally tell Edge.js to observe the [System.Web.Script.Serialization.ScriptIgnoreAttribute](http://msdn.microsoft.com/en-us/library/system.web.script.serialization.scriptignoreattribute.aspx). You opt in to this behavior by setting the `EDGE_ENABLE_SCRIPTIGNOREATTRIBUTE` environment variable:

```
set EDGE_ENABLE_SCRIPTIGNOREATTRIBUTE=1
```

Edge.js by default does not observe the ScriptIgnoreAttribute to avoid the associated performance cost. 

### How to: call Node.js from C#  

In addition to marshalling data, edge can marshal proxies to JavaScript functions when invoking .NET code from Node.js. This allows .NET code to call back into Node.js. 

Suppose the Node.js application passes an `add` function to the .NET code as a property of an object. The function receives two numbers and returns the sum of them via the provided callback:

```javascript
var edge = require('edge-js');

var addAndMultiplyBy2 = edge.func(function () {/*
    async (dynamic input) => {
        var add = (Func<object, Task<object>>)input.add;
        var twoNumbers = new { a = (int)input.a, b = (int)input.b };
        var addResult = (int)await add(twoNumbers);
        return addResult * 2;
    }   
*/});

var payload = {
    a: 2,
    b: 3,
    add: function (data, callback) {
        callback(null, data.a + data.b);
    }
};

addAndMultiplyBy2(payload, function (error, result) {
    if (error) throw error;
    console.log(result);
});
```

The .NET code implements the addAndMultiplyBy2 function. It extracts the two numbers passed from Node.js, calls back into the `add` function exported from Node.js to add them, multiplies the result by 2 in .NET, and returns the result back to Node.js:

```
$>node sample.js
10
```

The Node.js function exported from Node.js to .NET must follow the prescriptive async pattern of accepting two parameters: payload and a callback. The callback function accepts two parameters. The first one is the error, if any, and the second the result of the operation:

```javascript
function (payload, callback) {
    var error;  // must be null or undefined in the absence of error
    var result; 

    // do something

    callback(error, result);
}
```

The proxy to that function in .NET has the following signature:

```c#
Func<object,Task<object>>
```

Using TPL in CLR to provide a proxy to an asynchronous Node.js function allows the .NET code to use the convenience of the `await` keyword when invoking the Node.js functionality. The example above shows the use of the `await` keyword when calling the proxy of the Node.js `add` method.  

### How to: export C# function to Node.js

Similarly to marshalling functions from Node.js to .NET, Edge.js can also marshal functions from .NET to Node.js. The .NET code can export a `Func<object,Task<object>>` delegate to Node.js as part of the return value of a .NET method invocation. For example:

```javascript
var createHello = edge.func(function () {/*
    async (input) =>
    {
        return (Func<object,Task<object>>)(async (i) => { 
            Console.WriteLine("Hello from .NET"); 
            return null; 
        });
    }
*/});

var hello = createHello(null, true); 
hello(null, true); // prints out "Hello from .NET"
```

This mechanism in conjunction with a closure can be used to expose CLR class instances or CLR state in general to JavaScript. For example:

```javascript
var createCounter = edge.func(function () {/*
    async (input) =>
    {
        var k = (int)input; 
        return (Func<object,Task<object>>)(async (i) => { return ++k; });
    }
*/});

var counter = createCounter(12, true); // create counter with 12 as initial state
console.log(counter(null, true)); // prints 13
console.log(counter(null, true)); // prints 14
```

### How to: script Python in a Node.js application

**NOTE** This functionality requires IronPython and has been tested on Windows only. 

Edge.js enables you to run Python and Node.js in-process.

In addition to [platform specific prerequisites](#what-you-need) you need [IronPython 2.7.3](http://ironpython.codeplex.com/releases/view/81726) to proceed.

#### Hello, world

Install edge and edge-py modules:

```
npm install edge-js
npm install edge-py
```

In your server.js:

```javascript
var edge = require('edge-js');

var hello = edge.func('py', function () {/*
    def hello(input):
        return "Python welcomes " + input

    lambda x: hello(x)
*/});

hello('Node.js', function (error, result) {
    if (error) throw error;
    console.log(result);
});
```

Run and enjoy:

```
$>node py.js
Python welcomes Node.js
```

#### The interop model

Your Python script must evaluate to a lambda expression that accepts a single parameter. The parameter represents marshalled input from the Node.js code. The return value of the lambda expression is passed back as the result to Node.js code. The Python script can contain constructs (e.g. Python functions) that are used in the closure of the lambda expression. The instance of the script with associated state is created when `edge.func` is called in Node.js. Each call to the function referes to that instance.

The simplest *echo* Python script you can embed in Node.js looks like this:

```python
lambda x: x
```

To say hello, you can use something like this:

```python
lambda: x: "Hello, " + x
```

To maintain a running sum of numbers:

```python
current = 0

def add(x):
    global current
    current = current + x
    return current

lambda x: add(x)
```

#### Python in its own file

You can reference Python script stored in a *.py file instead of embedding Python code in a Node.js script.

In your hello.py file:

```python
def hello(input):
    return "Python welcomes " + input

lambda x: hello(x)
```

In your hello.js file:

```javascript
var edge = require('edge-js');

var hello = edge.func('py', 'hello.py');

hello('Node.js', function (error, result) {
    if (error) throw error;
    console.log(result);
});
```

Run and enjoy:

```
$>node hello.js
Python welcomes Node.js
```

#### To sync or to async, that is the question

In the examples above Python script was executing asynchronously on its own thread without blocking the singleton V8 thread on which the Node.js event loop runs. This means your Node.js application remains responsive while the Python code executes in the background. 

If you know your Python code is non-blocking, or if you know what you are doing, you can tell Edge.js to execute Python code on the singleton V8 thread. This will improve performance for non-blocking Python scripts embedded in a Node.js application:

```javascript
var edge = require('edge-js');

var hello = edge.func('py', {
    source: function () {/*
        def hello(input):
            return "Python welcomes " + input

        lambda x: hello(x)
    */},
    sync: true
});

console.log(hello('Node.js', true));
```

The `sync: true` property in the call to `edge.func` tells Edge.js to execute Python code on the V8 thread as opposed to creating a new thread to run Python script on. The `true` parameter in the call to `hello` requests that Edge.js does in fact call the `hello` function synchronously, i.e. return the result as opposed to calling a callback function. 

### How to: script PowerShell in a Node.js application

**NOTE** This functionality only works on Windows. 

Edge.js enables you to run PowerShell and Node.js in-process on Windows. [Edge-PS](https://github.com/dfinke/edge-ps) connects the PowerShell ecosystem with Node.js.

You need Windows, [Node.js](http://nodejs.org), [.NET 4.5](http://www.microsoft.com/en-us/download/details.aspx?id=30653), and [PowerShell 3.0](http://www.microsoft.com/en-us/download/details.aspx?id=34595) to proceed.

### Hello, world

Install edge and edge-ps modules:

``` 
npm install edge-js
npm install edge-ps
```

In your server.js:

```javascript
var edge = require('edge-js');

var hello = edge.func('ps', function () {/*
"PowerShell welcomes $inputFromJS on $(Get-Date)"
*/});

hello('Node.js', function (error, result) {
    if (error) throw error;
    console.log(result[0]);
});
```

Run and enjoy:

```
C:\testEdgeps>node server
PowerShell welcomes Node.js on 05/04/2013 09:38:40
```

#### Tapping into PowerShell's ecosystem

Rather than embedding PowerShell directly, you can use PowerShell files, dot source them and even use *Import-Module*.

What you can do in native PowerShell works in Node.js.

#### Interop PowerShell and Python

Here you can reach out to IronPython from PowerShell from within Node.js on Windows. This holds true for working with JavaScript frameworks and C#.

```javascript
var edge = require('edge-js');

var helloPowerShell = edge.func('ps', function () {/*
	"PowerShell welcomes $inputFromJS"
*/});

var helloPython = edge.func('py', function () {/*
    def hello(input):
        return "Python welcomes " + input

    lambda x: hello(x)
*/});


helloPython('Node.js', function(error, result){
	if(error) throw error;

	helloPowerShell(result, function(error, result){
		if(error) throw error;
		console.log(result[0]);
	});
});
```

### How to: script F# in a Node.js application

**NOTE** This functionality has not been tested on non-Windows platforms. 

This section is coming up. In the meantime please refer to [Dave Thomas blog post](http://7sharpnine.com/posts/i-node-something/). This has been validated on Windows only. 

```javascript
var edge = require('edge-js');

var helloFs = edge.func('fs', function () {/*
    fun input -> async { 
        return "F# welcomes " + input.ToString()
    }
*/});

helloFs('Node.js', function (error, result) {
    if (error) throw error;
    console.log(result);
});
```

### How to: script Lisp in a Node.js application

**NOTE** This functionality has not been tested on non-Windows platforms. 

The [edge-lsharp](https://github.com/richorama/edge-lsharp) extension uses [LSharp](https://github.com/RobBlackwell/LSharp) to compile and run Lisp to .NET.

Install edge and edge-lsharp modules:

```
npm install edge-js
npm install edge-lsharp
```

In your server.js:

```javascript
var edge = require('edge-js');
var fact = edge.func('lsharp', function(){/*

;; Factorial
(def fact(n) 
    (if (is n 0) 1 (* n (fact (- n 1)))))

*/});

fact([5], function(err, answer){
    console.log(answer);
    // = 120
});
```

An LSharp filename can be passed in instead of the Lisp string/comment:

```js
var edge = require('edge-js');
var lisp = edge.func('lsharp', 'lisp-func.ls');

lisp(['arg1', 'arg2'], function(err, result){
    
});
```

In Lisp you can specify either a function (as shown in the first example) or just return a value:

```js
var edge = require('edge-js');
var lisp = edge.func('lsharp', '(+ 2 3)');

lisp([], function(err, answer){
    console.log(answer);
    // = 5
});
```

### How to: script T-SQL in a Node.js application

**Full documentation available at https://github.com/agracio/edge-sql**


### How to: support for other CLR languages

Edge.js can work with any pre-compiled CLR assembly that contains the `Func<object,Task<object>>` delegate. Out of the box, Edge.js also allows you to embed C# source code in a Node.js application and compile it on the fly. 

To enable compilation of other CLR languages (e.g. F#) at runtime, or to support domain specific languages (DSLs) like T-SQL, you can use the compiler composability model provided by Edge.js. Please read the [add support for a CLR language](https://github.com/tjanczuk/edge/wiki/Add-support-for-a-CLR-language) guide to get started. 

### How to: exceptions

Edge.js marshals Node.js errors and exceptions to .NET as well as .NET exceptions to Node.js. 

CLR exceptions thrown in .NET code invoked from Node.js are marshalled as the `error` parameter to the Node.js callback function. Consider this example:

```javascript
var edge = require('edge-js');

var clrFunc = edge.func(function () {/*
    async (dynamic input) => {
        throw new Exception("Sample exception");
    }
*/});

clrFunc(null, function (error, result) {
    if (error) {
		console.log('Is Error?', error instanceof Error);
		console.log('-----------------');
		console.log(util.inspect(error, showHidden=true, depth=99, colorize=false));
		return;
	}
});
```

Running this Node.js application shows that the CLR exception was indeed received by the Node.js callback. The `error` parameter contains an Error object having most of the properties of the Exceptions copied over:
```
Is Error? true
-----------------
{ [System.AggregateException: One or more errors occurred.]
  message: 'One or more errors occurred.',
  name: 'System.AggregateException',
  InnerExceptions: 'System.Collections.ObjectModel.ReadOnlyCollection`1[[System.Exception, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]]',
  Message: 'One or more errors occurred.',
  Data: 'System.Collections.ListDictionaryInternal',
  InnerException:
   { [System.Exception: Sample exception]
     message: 'Sample exception',
     name: 'System.Exception',
     Message: 'Sample exception',
     Data: 'System.Collections.ListDictionaryInternal',
     TargetSite: 'System.Reflection.RuntimeMethodInfo',
     StackTrace: '   at Startup.<<Invoke>b__0>d__2.MoveNext() in c:\\Users\\User.Name\\Source\\Repos\\eCash2\\test\\edge2.js:line 7\r\n--- End of stack trace from previous location where exception was thrown ---\r\n   at System.Runtime.CompilerServices.TaskAwaiter.ThrowForNonSuccess(Task task)\r\n   at System.Runtime.CompilerServices.TaskAwaiter.HandleNonSuccessAndDebuggerNotification(Task task)\r\n   at System.Runtime.CompilerServices.TaskAwaiter`1.GetResult()\r\n   at Startup.<Invoke>d__4.MoveNext() in c:\\Users\\User.Name\\Source\\Repos\\eCash2\\test\\edge2.js:line 5',
     Source: 'cp2luegt',
     HResult: -2146233088 },
  HResult: -2146233088 }
```
The exception is copied back as Error object like every normal result object from the .NET world to JavaScript. 
Therefore all properties and their values are available on the Error object.

Additionally, the following happens during the mapping:
* To represent the Exception type, its full name is stored as `name`.
* To follow the [JavaScript convention for Errors](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/Error), the `Message` is also stored as the property `message`.
* `System::Reflection::RuntimeMethodInfo`s are not copied to avoid stack overflows

```
$>node sample.js

Edge.js:58
    edge.callClrFunc(appId, data, callback);
                     ^
System.Reflection.TargetInvocationException: Exception has been thrown by the target of an invocation. 
---> System.Exception: Sample exception
   at Startup.Invoke(Object input) in sample.cs:line 12
``` 

JavaScript exceptions thrown in Node.js code invoked from .NET are wrapped in a CLR exception and cause the asynchronous `Task<object>` to complete with a failure. Errors passed by Node.js code invoked from .NET code to the callback function's `error` parameter have the same effect. Consider this example:

```javascript
var edge = require('edge-js');

var multiplyBy2 = edge.func(function () {/*
    async (dynamic input) => {
        var aFunctionThatThrows = (Func<object, Task<object>>)input.aFunctionThatThrows;
        try {
            var aResult = await aFunctionThatThrows(null);
        }
        catch(Exception e)
        {
            Console.WriteLine(e);
        }

        return null;
    }
*/});

var payload = {
    someParameter: 'arbitrary parameter',
    aFunctionThatThrows: function (data, callback) {
        throw new Error('Sample JavaScript error');
    }
};

multiplyBy2(payload, function (error, result) {
    if (error) throw error;
    console.log(result);
});
```

Running the code shows the .NET code receiving a CLR exception as a result of the Node.js function throwing a JavaScript error. The exception shows the complete stack trace, including the part that executed in the Node.js code:

```
$>node sample.js
System.Exception: Error: Sample JavaScript error
    at payload.aFunctionThatThrows (sample.js:7:11)
   at System.Runtime.CompilerServices.TaskAwaiter.ThrowForNonSuccess(Task task)
   at System.Runtime.CompilerServices.TaskAwaiter.HandleNonSuccessAndDebuggerNotification(Task task)
   at Edge.Sample.Startup.<Invoke>d__0.MoveNext()
```

### How to: app.config

When running C# code within Node.js app, the app config file is node.exe.config and should be located right next to the node.exe file.

### How to: debugging

**NOTE** This is Windows-only functionality.

On Windows, you can debug the .NET code running as part of your Node.js application by attaching a managed code debugger (e.g. Visual Studio) to node.exe. You can debug .NET code in a pre-compiled CLR assembly as well C# literals embedded in the application and compiled by Edge.js at runtime. 

#### Debugging pre-compiled .NET code

If you have integrated .NET code into a Node.js application using a pre-compiled CLR assembly like this:

```javascript
var hello = edge.func('My.Assembly.dll');
```

then the best way to debug your .NET code is to attach a managed code debugger (e.g. Visual Studio) to the node.exe process. Since the node.exe process runs both native and managed code, make sure to select managed code type as target:

![debug](https://f.cloud.github.com/assets/822369/190564/a41bab2c-7efb-11e2-878f-82ae2325876c.PNG)

From there, you can set breakpoints in your .NET code and the debugger will stop when they are reached.

#### Debugging embedded C# code

Debugging embedded C# code (on Windows) requires that `EDGE_CS_DEBUG` environment variable is set in the environment of the node.exe process:

```
set EDGE_CS_DEBUG=1
```

Without this setting (the default), Edge.js will not generate debugging information when compiling embedded C# code.

You can debug C# code embedded into a Node.js application using a reference to a *.cs or *.csx file:

```javascript
var hello = edge.func('MyClass.cs');
```

You can also debug C# code embedded directly into a *.js file using the function comment syntax:

```javscript
var hello = edge.func(function () {/*
    async (input) =>
    {
        System.Diagnostics.Debugger.Break();
        var result = ".NET welcomes " + input.ToString();
        return result;
    }
*/});
```

You *cannot* debug C# code embedded as a simple string literal:

```javascript
var hello = edge.func('async (input) => { return 2 * (int)input; }');
```

After setting `EDGE_CS_DEBUG=1` environment variable before starting node.exe and attaching the managed debugger to the node.exe process, you can set breakpoints in C# code (which may appear as a JavaScript comment), or use `System.Diagnostics.Debugger.Break()` to break into the debugger from .NET code. 

![debug-inline](https://f.cloud.github.com/assets/822369/326781/923d870c-9b4a-11e2-8f45-201a6431afbf.PNG)

### Performance

Read more about [performance of Edge.js on the wiki](https://github.com/tjanczuk/edge/wiki/Performance). Here is the gist of the latency (smaller is better):

![edgejs-performance1](https://f.cloud.github.com/assets/822369/486393/645f696a-b920-11e2-8a20-9fa6932bb092.png)

### Building on Windows

You must have Visual Studio 2019* toolset, Python 3.6.x, and node-gyp installed for building.

To build and test the project against all supported versions of Node.js in x86 and x64 flavors, run the following:

```
tools\buildall.bat
test\testall.bat
```

To build one of the versions of Node.js officially released by [Node.js](http://nodejs.org/dist), do the following:

```
cd tools
build.bat release 8.10.0
```

Note: the Node.js version number you provide must be version number corresponding to one of the subdirectories of http://nodejs.org/dist. The command will build both x32 and x64 architectures (assuming you use x64 machine). The command will also copy the edge\_\*.node executables to appropriate locations under lib\native directory where they are looked up from at runtime. The `npm install` step copies the C standard library shared DLL to the location of the edge\_\*.node files for the component to be ready to go.

To build the C++\CLI native extension using the version of Node.js installed on your machine, issue the following command:

```
npm install -g node-gyp
node-gyp configure --msvs_version=2015
node-gyp build -debug
```

You can then set the `EDGE_NATIVE` environment variable to the fully qualified file name of the built edge_\*.node binary (edge\_nativeclr.node if you're using the native CLR runtime or edge\_coreclr.node if you're using .NET Core). It is useful during development, for example:

```
set EDGE_NATIVE=C:\projects\edge\build\Debug\edge_nativeclr.node
``` 

You can also set the `EDGE_DEBUG` environment variable to 1 to have the edge module generate debug traces to the console when it runs.

### Running tests

You must have mocha installed on the system. Then:

```
npm test
```

or, from the root of the enlistment:

```
mocha -R spec
```

**NOTE** in environments with both desktop CLR/Mono and .NET Core installed, tests will by default use desktop CLR/Mono. To run tests against .NET Core, use: 

```
EDGE_USE_CORECLR=1 npm test
```

#### Node.js version targeting on Windows

**NOTE** this is Windows only functionality.

If you want to run tests after building against a specific version of Node.js that one of the previous builds used, issue the following command:

```
cd test
test.bat ia32 0.10.0
```

Which will run the tests using Node.js x86 v0.10.0. Similarly:

```
cd test
test.bat x64 0.8.22
```

Would run tests against Node.js 0.8.22 on x64 architecture.

### Building on OSX

Prerequisities:

* [Homebrew](http://brew.sh/)  
* Mono and/or .NET Core - see below  
* Node.js

You can use Edge.js on OSX with either Mono or .NET Core installed, or both.

If you choose to follow steps here [install Mono](https://www.mono-project.com/download/stable/#download-mac) or install using Homebrew.  If you choose to install .NET Core, follow the steps [here](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)

Then install and build Edge.js:

```bash
brew install pkg-config # Only needed if using Mono
npm install edge-js
```

**NOTE** if the build process complains about being unable to locate Mono libraries, you may need to specify the search path explicitly. This may be installation dependent, but in most cases will look like: 

```bash
PKG_CONFIG_PATH=/Library/Frameworks/Mono.framework/Versions/Current/lib/pkgconfig \
  npm install edge-js
```

If you installed both Mono and .NET Core, by default Edge will use Mono. You opt in to using .NET Core with the `EDGE_USE_CORECLR` environment variable: 

```bash
EDGE_USE_CORECLR=1 node myapp.js
```

#### Building on OSX (advanced)

To build edge from a clone of the repository or source code:

```bash
node-gyp configure build
```

To build a debug build instead of release, you need to:

```bash
node-gyp configure build -debug
export EDGE_NATIVE=/Users/tomek/edge/build/Debug/edge_nativeclr.node
```

### Building on Linux 

For a normative set of steps to set up Edge.js on Linux with CoreCLR please refer to the [Dockerfile](https://github.com/agracio/edge-js/blob/master/Dockerfile). You can also use the ready-made [Docker image](#docker). 

### Using .NET Core

If you have only .NET Core installed on your system and not Mono, you can run Edge with no changes.  However, if you have both runtimes installed, Edge will automatically use Mono unless directed otherwise.  To use .NET Core in a dual-runtime environment, set the `EDGE_USE_CORECLR=1` environment variable when starting node, i.e.

```bash
EDGE_USE_CORECLR=1 node sample.js
```

Edge will try to find the .NET Core runtime in the following locations:

 * The path in the `CORECLR_DIR` environment variable, if provided
 * The current directory
 * The directory containing `edge_*.node`
 * Directories in the `PATH` environment variable.  Once a directory containing the `dotnet` executable is located, we then do the following to decide which version of the framework (you can have several installed at once) to load
	 * If the `CORECLR_VERSION` environment variable was specified, we try to load that version
	 * Else, if the project.json/*.deps.json has a reference to `Microsoft.NETCore.App`, indicating that it was built for a specific framework version, we try to load that version
	 * Otherwise, we pick the maximum installed version
  
So, if the CLR is another location or you want to use a version of the CLR other than the default that you've set, the best way to specify that is through the `CORECLR_DIR` or `CORECLR_VERSION` environment variables, i.e.

```bash
EDGE_USE_CORECLR=1 \
CORECLR_DIR=/usr/share/dotnet/dnx-coreclr-linux-x64.1.0.0-beta6-11944 \
node sample.js
```

## Scripting Node.js from CLR

If you are writing a CLR application (e.g. a C# console application or ASP.NET web app), this section explains how you include and run Node.js code in your app. It only works on Windows using desktop CLR.

### What you need

You need Windows with:

* [.NET 4.5](http://www.microsoft.com/en-us/download/details.aspx?id=30653)  
* [Edge.js NuGet package](https://www.nuget.org/packages/EdgeJs)
* [Node.js](http://nodejs.org) (optional, if you want to use additional NPM packages)

Edge.js support for scripting Node.js ships as a NuGet Package called `EdgeJs`. It comes with everything you need to get started writing applications for x86 and x64 architectures. However, if you want to use additional Node.js packages from NPM, you must separately install Node.js runtime to access the NPM package manager. The latest Edge.js NuGet package has been developed and tested with Node.js v8.10.0. Older Edge.js packages exist for prior versions of Node.js. If you choose a different version of Node.js to install NPM packages, your mileage can vary.

**NOTE** you cannot use native Node.js extensions when scripting Node.js from CLR using Edge. 

You can install the [Edge.js NuGet package](https://www.nuget.org/packages/EdgeJs) using the Visual Studio built-in NuGet package management functionality or using the stand-alone [NuGet client](http://docs.nuget.org/docs/start-here/installing-nuget).

### How to: Node.js hello, world

Create a .NET 4.5 Console Application in Visual Studio. Add the Edge.js NuGet package to the project. Then in your code:

```c#
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

Compile and run:

```
C:\project\sample\bin\Debug> sample.exe
Node.js welcomes .NET
```

### How to: integrate Node.js code into CLR code

The Edge.js NuGet package contains a single managed assembly `EdgeJs.dll` with a single class `EdgeJs.Edge` exposing a single static function `Func`. The function accepts a string containing code in Node.js that constructs and *returns* a JavaScript function. The JavaScript function must have the signature required by Edge.js's prescriptive interop pattern: it must accept one parameter and a callback, and the callback must be called with an error and one return value: 

```c#
var func = Edge.Func(@"
    return function (data, callback) {
        callback(null, 'Hello, ' + data);
    }
");
```

Edge.js creates a `Func<object,Task<object>>` delegate in CLR that allows .NET code to call the Node.js function asynchronously. You can use the standard TPL mechanisms or the async/await keywords to conveniently await completion of the asynchornous Node.js function:

```c#
var result = await func(".NET");
// result == "Hello, .NET"
```

Note that the Node.js code snippet is not a function *definition*. Instead it must create and *return* a function instance. This allows you to initialize and maintain encapsulated Node.js state associated with the instance of the created function. The initialization code will execute only once when you call `Edge.Func`. Conceptually this is similar to defining a Node.js module that exports a single function (by returning it to the caller).  For example:

```c#
var increment = Edge.Func(@"
    var current = 0;

    return function (data, callback) {
        current += data;
        callback(null, current);
    }
");

Console.WriteLine(await increment(4)); // outputs 4
Console.WriteLine(await increment(7)); // outputs 11
```

Using multiline C# string literals is convenient for short Node.js code snippets, but you may want to store larger Node.js code in its own `*.js` file or files. 

One pattern is to store your Node.js code in a `myfunc.js` file:

```javascript
return function (data, callback) {
    callback(null, 'Node.js welcomes ' + data);
}
```

And then load such file into memory with `File`:

```c#
var func = Edge.Func(File.ReadAllText("myfunc.js"));
```

Another pattern is to define a Node.js module that itself is a function:

```javascript
module.exports = function (data, callback) {
    callback(null, 'Node.js welcomes ' + data);
};
```

And then load and return this module with a short snippet of Node.js:

```c#
var func = Edge.Func(@"return require('./../myfunc.js')");
```

(Note the relative location of the file).

### How to: use Node.js built-in modules

You can use Node.js built-in modules out of the box. For example, you can set up a Node.js HTTP server hosted in a .NET application and call it from C#:

```c#
var createHttpServer = Edge.Func(@"
    var http = require('http');

    return function (port, cb) {
        var server = http.createServer(function (req, res) {
            res.end('Hello, world! ' + new Date());
        }).listen(port, cb);
    };
");

await createHttpServer(8080);
Console.WriteLine(await new WebClient().DownloadStringTaskAsync("http://localhost:8080"));
```

### How to: use external Node.js modules

You can use external Node.js modules, for example modules installed from NPM. 

Note: Most Node.js modules are written in JavaScript and will execute in Edge as-is. However, some Node.js external modules are native binary modules, rebuilt by NPM on module installation to suit your local execution environment. Native binary modules will not run in Edge unless they are rebuilt to link against the NodeJS dll that Edge uses.

To install modules from NPM, you must first [install Node.js](http://nodejs.org) on your machine and use the `npm` package manager that comes with the Node.js installation. NPM modules must be installed in the directory where your build system places the Edge.js NuGet package (most likely the same location as the rest of your application binaries), or any ancestor directory. Alternatively, you can install NPM modules globally on the machine using `npm install -g`:

```
C:\projects\websockets> npm install ws
...
ws@0.4.31 node_modules\ws
+-- tinycolor@0.0.1
+-- options@0.0.5
+-- nan@0.3.2
+-- commander@0.6.1
```

You can then use the installed `ws` module to create a WebSocket server inside of a .NET application:

```c#
class Program
{
    public static async void Start()
    {
        var createWebSocketServer = Edge.Func(@"
            var WebSocketServer = require('ws').Server;

            return function (port, cb) {
                var wss = new WebSocketServer({ port: port });
                wss.on('connection', function (ws) {
                    ws.on('message', function (message) {
                        ws.send(message.toString().toUpperCase());
                    });
                    ws.send('Hello!');
                });
                cb();
            };
        ");

        await createWebSocketServer(8080);
    }

    static void Main(string[] args)
    {
        Task.Run((Action)Start);
        new ManualResetEvent(false).WaitOne();
    }
}
```

This WebSocket server sends a *Hello* message to the client when a new connection is established, and then echos a capitalized version of every message it receives back to the client. You can test this webserver with the `wscat` tool, first install the `wscat` module globally:

```
npm install -g wscat
```

Then start the .NET application containing the WebSocket server and establish a connection to it with `wscat`:

```
C:\projects\websockets> wscat -c ws://localhost:8080/

connected (press CTRL+C to quit)

< Hello!
> foo
< FOO
> bar
< BAR
```

A self-contained Node.js WebSocket server, even if running within a .NET application, is rather unexciting. After all, the same could be accomplished with a stand-alone Node.js process. Ideally you could establish a WebSocket server in Node.js, but handle the messages in .NET. Let's do it - read on.

### How to: handle Node.js events in .NET

It is often useful to handle certain events raised by the Node.js code within .NET. For example, you may want to establish a WebSocket server in Node.js, and handle the incoming messages in the .NET part of your application. This can be accomplished by passig a .NET callback function to Node.js when the WebSocket server is created:

```c#
class Program
{
    public static async void Start()
    {
        // Define an event handler to be called for every message from the client

        var onMessage = (Func<object, Task<object>>)(async (message) =>
        {
            return "Received string of length " + ((string)message).Length;
        });

        // The WebSocket server delegates handling of messages from clients
        // to the supplied .NET handler

        var createWebSocketServer = Edge.Func(@"
            var WebSocketServer = require('ws').Server;

            return function (options, cb) {
                var wss = new WebSocketServer({ port: options.port });
                wss.on('connection', function (ws) {
                    ws.on('message', function (message) {
                        options.onMessage(message, function (error, result) {
                            if (error) throw error;
                            ws.send(result);
                        });
                    });
                    ws.send('Hello!');
                });
                cb();
            };
        ");

        // Create a WebSocket server on a specific TCP port and using the .NET event handler

        await createWebSocketServer(new
        {
            port = 8080,
            onMessage = onMessage
        });
    }

    static void Main(string[] args)
    {
        Task.Run((Action)Start);
        new ManualResetEvent(false).WaitOne();
    }
}
```

Using `wscat`, you can verify the .NET handler is indeed invoked for every websocket message:

```
C:\projects\websockets> wscat -c ws://localhost:8080/

connected (press CTRL+C to quit)

< Hello!
> Foo
< Received string of length 3
> FooBar
< Received string of length 6
```

This example shows how Edge.js can create JavaScript proxies to .NET functions and marshal calls across the V8/CLR boundary in-process. Read more about [data marshaling between Node.js and CLR](#how-to-marshal-data-between-c-and-nodejs).

### How to: expose Node.js state to .NET

In the previous example [a Node.js HTTP server was created and started from .NET](#how-to-use-nodejs-built-in-modules). Suppose at some point you want to stop the HTTP server from your .NET code. Given that all references to it are embedded within Node.js code, it is not possible. However, just as Edge.js can [pass a .NET function to Node.js](#how-to-handle-nodejs-events-in-net), it also can export a Node.js function to .NET. Moreover, that function can be implemented as a closure over Node.js state. This is how it would work:

```c#
var createHttpServer = Edge.Func(@"
    var http = require('http');

    return function (port, cb) {
        var server = http.createServer(function (req, res) {
            res.end('Hello, world! ' + new Date());
        }).listen(port, function (error) {
            cb(error, function (data, cb) {
                server.close();
                cb();
            });
        });
    };
");

var closeHttpServer = (Func<object,Task<object>>)await createHttpServer(8080);
Console.WriteLine(await new WebClient().DownloadStringTaskAsync("http://localhost:8080"));
await closeHttpServer(null);
```

Notice how the `createHttpServer` function, in addition to starting an HTTP server in Node.js, is also returning a .NET proxy to a JavaScript function that allows that server to be stopped. 

### How to: use Node.js in ASP.NET web applications

Using Node.js via Edge.js in ASP.NET web applications is no different than in a .NET console application. The Edge.js NuGet package must be referenced in your ASP.NET web application. If you are using any external Node.js modules, the entire `node_modules` subdirectory structure must be binplaced to the `bin` folder of you web application, and deployed that way to the server. 

### How to: debug Node.js code running in a CLR application

The `EDGE_NODE_PARAMS` environment variable allows you to specify any options that are normally passed via command line to the node executable. This includes the `--debug` options necessary to use [node-inspector](https://github.com/node-inspector/node-inspector) to debug Node.js code. 

### Building Edge.js NuGet package

**Note** This mechanism requires hardening, expect the road ahead to be bumpy. 

These are instructions for building the Edge.js NuGet package on Windows. The package will support running apps in both x86 and x64 architectures using a selected version of Node.js. The resulting NuGet package is all-inclusive with the only dependency being .NET 4.5. 

Preprequisties:

* Visual Studio 2019
* Node.js (tested with v8.10.0)
* Python 3.6.x  
* node-gyp (latest)  
* NASM (for opn-ssl) https://www.nasm.us/

To build the NuGet package, open the Visual Studio 2019 Developer Command Prompt and call:

```
tools\build_double.bat 20.12.2
```

(you can substitute another version of Node.js).

The script takes several minutes to complete and does the following:

* builds a few helper tools in C#  
* downloads sources of the selected Node.js version  
* downloads nuget.exe from http://nuget.org  
* builds Node.js shared library for the x86 and x64 flavor
* builds Edge.js module for the x86 and x64 flavor
* builds managed EdgeJs.dll library that bootstraps running Node.js in a CLR process and provides the Edge.Func programming model  
* packs everything into a NuGet package

If everything goes well, the resulting NuGet package is located in the `tools\nuget` directory.

### Running tests of scripting Node.js in C#

There are functional tests in `test\double\double_test` and stress tests in `test\double\double_stress`. Before you can compile these tests, you must register the location of the built NuGet package as a local NuGet feed through the NuGet configuration manager in Visual Studio. 

After you have compiled the function tests, the best way to run them is from the command line:

```
C:\projects\edge\test\double\double_tests\bin\Release> mstest /testcontainer:double_test.dll /noisolation
```

After you have compiled the stress tests, simply launch the executable, attach resource monitor to the process, and leave it running to observe stability:

```
C:\projects\edge\test\double\double_stress\bin\Release> double_stress.exe
```

## Use cases and other resources

[Accessing MS SQL from Node.js via Edge.js](https://blog.codeship.com/node-js-sql-server-edge-js/) by [David Neal](https://twitter.com/reverentgeek)  
[Using ASP.NET and React on the server via Edge.js](http://navigation4asp.net/2015/10/13/progressive-enhancement-enhanced/) by [Graham Mendick](https://twitter.com/grahammendick)  

## Contribution and derived work

I do welcome contributions via pull request and derived work. 

The edge module is intended to remain a very small component with core functionality that supports interop between .NET and Node.js. Domain specific functionality (e.g. access to SQL, writing to ETW, writing connect middleware in .NET) should be implemented as separate modules with a dependency on edge. When you have a notable derived work, I would love to know about it to include a pointer here.  

## More

Issues? Feedback? You [know what to do](https://github.com/agracio/edge-js/issues/new). Pull requests welcome.

[dependencies-url]: https://www.npmjs.com/package/edge-js?activeTab=dependencies
[dependencies-img]: https://img.shields.io/librariesio/release/npm/edge-js.svg?style=flat-square

[downloads-img]: https://img.shields.io/npm/dw/edge-js.svg?style=flat-square
[downloads-url]: https://img.shields.io/npm/dw/edge-js.svg

[appveyor-image]:https://ci.appveyor.com/api/projects/status/3hs8xq7jieufw507/branch/master?svg=true
[appveyor-url]:https://ci.appveyor.com/project/agracio/edge-js/branch/master

[license-url]: https://github.com/agracio/edge-js/blob/master/LICENSE
[license-img]: https://img.shields.io/badge/license-MIT-blue.svg?style=flat-square

[issues-img]: https://img.shields.io/github/issues/agracio/edge-js.svg?style=flat-square
[issues-url]: https://github.com/agracio/edge-js/issues
[closed-issues-img]: https://img.shields.io/github/issues-closed-raw/agracio/edge-js.svg?style=flat-square&color=brightgreen
[closed-issues-url]: https://github.com/agracio/edge-js/issues?q=is%3Aissue+is%3Aclosed

[codacy-img]: https://app.codacy.com/project/badge/Grade/3833e15b273d4add8d2030764e8977d9
[codacy-url]: https://app.codacy.com/gh/agracio/edge-js/dashboard?utm_source=gh&utm_medium=referral&utm_content=&utm_campaign=Badge_grade

[github-img]: https://github.com/agracio/edge-js/workflows/Test/badge.svg
[github-url]: https://github.com/agracio/edge-js/actions/workflows/main.yml


