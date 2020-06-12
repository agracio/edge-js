var edge = require('../lib/edge.js'), assert = require('assert')
	, path = require('path');

var edgeTestDll = process.env.EDGE_USE_CORECLR ? 'test' : path.join(__dirname, 'Edge.Tests.dll');
var prefix = process.env.EDGE_USE_CORECLR ? '[CoreCLR]' : process.platform === 'win32' ? '[.NET]' : '[Mono]';

describe('edge.func', function () {

	it(prefix + ' exists', function () {
		assert.equal(typeof edge.func, 'function');
	});

	it(prefix + ' fails without parameters', function () {
		assert.throws(
			edge.func,
			/Specify the source code as string or provide an options object/
		);
	});

	it(prefix + ' fails with a wrong parameter', function () {
		assert.throws(
			function () { edge.func(12); },
			/Specify the source code as string or provide an options object/
		);
	});

	it(prefix + ' fails with a wrong language parameter', function () {
		assert.throws(
			function () { edge.func(12, 'somescript'); },
			/The first argument must be a string identifying the language compiler to use/
		);
	});

	it(prefix + ' fails with a unsupported language parameter', function () {
		assert.throws(
			function () { edge.func('idontexist', 'somescript'); },
			/Unsupported language 'idontexist'/
		);
	});

	it(prefix + ' fails with missing assemblyFile or source', function () {
		assert.throws(
			function () { edge.func({}); },
			/Provide DLL or source file name or .NET script literal as a string parmeter, or specify an options object/
		);
	});

	it(prefix + ' fails with both assemblyFile or source', function () {
		assert.throws(
			function () { edge.func({ assemblyFile: 'foo.dll', source: 'async (input) => { return null; }'}); },
			/Provide either an asseblyFile or source property, but not both/
		);
	});

	it(prefix + ' fails with nonexisting assemblyFile', function () {
		assert.throws(
			function () { edge.func('idontexist.dll'); },
			/System.IO.FileNotFoundException/
		);
	});

	if (!process.env.EDGE_USE_CORECLR) {
		it(prefix + ' succeeds with assemblyFile as string', function () {
			var func = edge.func(edgeTestDll);
			assert.equal(typeof func, 'function');
		});

		it(prefix + ' succeeds with assemblyFile as options property', function () {
			var func = edge.func({ assemblyFile: edgeTestDll });
			assert.equal(typeof func, 'function');
		});
	}

	it(prefix + ' succeeds with assemblyFile and type name', function () {
		var func = edge.func({
			assemblyFile: edgeTestDll,
			typeName: 'Edge.Tests.Startup'
		});
		assert.equal(typeof func, 'function');
	});

	it(prefix + ' fails with assemblyFile and nonexisting type name', function () {
		assert.throws(
			function () {
				edge.func({
					assemblyFile: edgeTestDll,
					typeName: 'Edge.Tests.idontexist'
				});
			},
			/Could not load type 'Edge.Tests.idontexist'/
		);
	});

	it(prefix + ' succeeds with assemblyFile, type name, and method name', function () {
		var func = edge.func({
			assemblyFile: edgeTestDll,
			typeName: 'Edge.Tests.Startup',
			methodName: 'Invoke'
		});
		assert.equal(typeof func, 'function');
	});

	it(prefix + ' fails with assemblyFile, type name and nonexisting method name', function () {
		assert.throws(
			function () {
				edge.func({
					assemblyFile: edgeTestDll,
					typeName: 'Edge.Tests.Startup',
					methodName: 'idontexist'
				});
			},
			/Unable to access the CLR method to wrap through reflection/
		);
	});
});