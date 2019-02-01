/**
 * Portions Copyright (c) Microsoft Corporation. All rights reserved.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *  http://www.apache.org/licenses/LICENSE-2.0
 *
 * THIS CODE IS PROVIDED *AS IS* BASIS, WITHOUT WARRANTIES OR CONDITIONS
 * OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING WITHOUT LIMITATION
 * ANY IMPLIED WARRANTIES OR CONDITIONS OF TITLE, FITNESS FOR A PARTICULAR
 * PURPOSE, MERCHANTABLITY OR NON-INFRINGEMENT.
 *
 * See the Apache Version 2.0 License for specific language governing
 * permissions and limitations under the License.
 */
var edge = require('../lib/edge.js'), assert = require('assert')
	, path = require('path');

var edgeTestDll = process.env.EDGE_USE_CORECLR ? 'test' : path.join(__dirname, 'Edge.Tests.dll');
var prefix = process.env.EDGE_USE_CORECLR ? '[CoreCLR]' : process.platform === 'win32' ? '[.NET]' : '[Mono]';

describe('async call from .net to node.js', function () {

	it(prefix + ' succeeds for hello world', function (done) {
		var func = edge.func({
			assemblyFile: edgeTestDll,
			typeName: 'Edge.Tests.Startup',
			methodName: 'InvokeBack'
		});
		var payload = {
			hello: function (payload, callback) {
				callback(null, 'Node.js welcomes ' + payload);
			}
		};
		func(payload, function (error, result) {
			assert.ifError(error);
			assert.equal(result, 'Node.js welcomes .NET');
			done();
		});
	});

	it(prefix + ' successfuly marshals data from .net to node.js', function (done) {
		var func = edge.func({
			assemblyFile: edgeTestDll,
			typeName: 'Edge.Tests.Startup',
			methodName: 'MarshalInFromNet'
		});
		var payload = {
			hello: function (result, callback) {
				assert.equal(typeof result, 'object');
				assert.ok(result.a === 1);
				assert.ok(result.b === 3.1415);
				assert.ok(result.c === 'fooåäö');
				assert.ok(result.d === true);
				assert.ok(result.e === false);
				assert.equal(typeof result.f, 'object');
				assert.ok(Buffer.isBuffer(result.f));
				assert.equal(result.f.length, 10);
				assert.ok(Array.isArray(result.g));
				assert.equal(result.g.length, 2);
				assert.ok(result.g[0] === 1);
				assert.ok(result.g[1] === 'fooåäö');
				assert.equal(typeof result.h, 'object');
				assert.ok(result.h.a === 'fooåäö');
				assert.ok(result.h.b === 12);
				assert.equal(typeof result.i, 'function');
				assert.equal(typeof result.j, 'object');
				assert.ok(result.j.valueOf() === Date.UTC(2013, 07, 30));

				callback(null, 'yes');
			}
		};
		func(payload, function (error, result) {
			assert.ifError(error);
			assert.equal(result, 'yes');
			done();
		});
	});

	it(prefix + ' successfuly marshals object hierarchy from .net to node.js', function (done) {
		var func = edge.func({
			assemblyFile: edgeTestDll,
			typeName: 'Edge.Tests.Startup',
			methodName: 'MarshalObjectHierarchy'
		});
		func(null, function (error, result) {
			assert.ifError(error);
			assert.equal(result.A_field, 'a_field');
			assert.equal(result.A_prop, 'a_prop');
			assert.equal(result.B_field, 'b_field');
			assert.equal(result.B_prop, 'b_prop');
			done();
		});
	});

	it(prefix + ' successfuly marshals data from node.js to .net', function (done) {
		var func = edge.func({
			assemblyFile: edgeTestDll,
			typeName: 'Edge.Tests.Startup',
			methodName: 'MarshalBackToNet'
		});
		var payload = {
			hello: function (result, callback) {
				var payload = {
					a: 1,
					b: 3.1415,
					c: 'fooåäö',
					d: true,
					e: false,
					f: new Buffer(10),
					g: [ 1, 'fooåäö' ],
					h: { a: 'fooåäö', b: 12 },
					j: new Date(Date.UTC(2013, 07, 30))
				};
				callback(null, payload);
			}
		};
		func(payload, function (error, result) {
			assert.ifError(error);
			assert.equal(result, 'yes');
			done();
		});
	});

	it(prefix + ' successfuly handles process.nextTick() in JS callback', function (done) {
		var func = edge.func({
			assemblyFile: edgeTestDll,
			typeName: 'Edge.Tests.Startup',
			methodName: 'InvokeBack'
		});
		var payload = {
			hello: function (payload, callback) {
				process.nextTick(function() {
					callback(null, 'Node.js welcomes ' + payload);
				});
			}
		};
		func(payload, function (error, result) {
			assert.ifError(error);
			assert.equal(result, 'Node.js welcomes .NET');
			done();
		});
	});

	it(prefix + ' successfuly marshals v8 exception on invoking thread', function (done) {
		var func = edge.func({
			assemblyFile: edgeTestDll,
			typeName: 'Edge.Tests.Startup',
			methodName: 'MarshalException'
		});
		var payload = {
			hello: function (result, callback) {
				throw new Error('Sample Node.js exception');
			}
		};
		func(payload, function (error, result) {
			assert.ifError(error);
			assert.equal(typeof result, 'string');
			assert.ok(result.indexOf('Sample Node.js exception') > 0);
			done();
		});
	});

	it(prefix + ' successfuly marshals v8 exception in callback', function (done) {
		var func = edge.func({
			assemblyFile: edgeTestDll,
			typeName: 'Edge.Tests.Startup',
			methodName: 'MarshalException'
		});
		var payload = {
			hello: function (result, callback) {
				var next = global.setImmediate || process.nextTick;
				next(function () {
					callback(new Error('Sample Node.js exception'));
				});
			}
		};
		func(payload, function (error, result) {
			assert.ifError(error);
			assert.equal(typeof result, 'string');
			assert.ok(result.indexOf('Sample Node.js exception') > 0);
			done();
		});
	});

	it(prefix + ' successfuly marshals empty buffer', function (done) {
		var func = edge.func({
			assemblyFile: edgeTestDll,
			typeName: 'Edge.Tests.Startup',
			methodName: 'ReturnEmptyBuffer'
		});

		func(null, function (error, result) {
			assert.ifError(error);
			assert.ok(Buffer.isBuffer(result));
			assert.ok(result.length === 0)
			done();
		})
	});
});

describe('delayed call from node.js to .net', function () {

	it(prefix + ' succeeds for one callback after Task', function (done) {
		var expected = [
			'InvokeBackAfterCLRCallHasFinished#EnteredCLR',
			'InvokeBackAfterCLRCallHasFinished#LeftCLR',
			'InvokeBackAfterCLRCallHasFinished#ReturnedToNode',
			'InvokeBackAfterCLRCallHasFinished#CallingCallbackFromDelayedTask',
		];

		var func = edge.func({
			assemblyFile: edgeTestDll,
			typeName: 'Edge.Tests.Startup',
			methodName: 'InvokeBackAfterCLRCallHasFinished'
		});

		var ensureNodejsFuncIsCollected = edge.func({
			assemblyFile: edgeTestDll,
			typeName: 'Edge.Tests.Startup',
			methodName: 'EnsureNodejsFuncIsCollected'
		});

		var trace = [];
		var payload = {
			eventCallback: function(result, callback) {
				trace.push(result);
				callback();
				assert.ok(expected.length === trace.length);
				for (var i = 0; i < expected.length; i++) {
					assert.ok(expected[i] === trace[i]);
				}

				// Check for collections after the callback is completed
				// The func is still referenced by the callback context so it won't be collected if we run inline
				setTimeout(function() {
					ensureNodejsFuncIsCollected(null, function(error, result) {
						assert.ifError(error);
						assert.ok(result);
						done();
					});
				}, 10);
			}
		};

		func(payload, function (error, result) {
			assert.ifError(error);
			result.forEach(function(entry) {
				trace.push(entry);
			});
			trace.push('InvokeBackAfterCLRCallHasFinished#ReturnedToNode');
		});
	});
});

describe('.net returns Func to node.js', function () { 
	it(prefix + ' releases the func', function (done) {

		assert.ok(global.gc, 'This test must be run with --expose-gc set');

		var returnDotNetFunc = edge.func({
			assemblyFile: edgeTestDll,
			typeName: 'Edge.Tests.Startup',
			methodName: 'ReturnDotNetFunc'
		});

		var ensureDotNetFuncIsCollected = edge.func({
			assemblyFile: edgeTestDll,
			typeName: 'Edge.Tests.Startup',
			methodName: 'EnsureDotNetFuncIsCollected'
		});

		returnDotNetFunc(null, function(error, result) {

			assert.ifError(error);

			// Check for collections after the callback is completed
			// The func is still referenced by the callback context so it won't be collected if we run inline
			setTimeout(() => {

				// Force a GC to release the func returned by returnDotNetFunc is freed
				global.gc();

				ensureDotNetFuncIsCollected(null, function(error, result) {
					assert.ifError(error);
					assert.ok(result);
					done();
				});
			});
		});
	});
});