var edge = require('../lib/edge.js'), assert = require('assert')
    , path = require('path');

const crypto = require('crypto');

var edgeTestDll = process.env.EDGE_USE_CORECLR ? 'test' : path.join(__dirname, 'Edge.Tests.dll');
var prefix = process.env.EDGE_USE_CORECLR ? '[CoreCLR]' : process.platform === 'win32' ? '[.NET]' : '[Mono]';

function generateUuidBySeed() {

    const hash = crypto.createHash('sha256').update('seed').digest('hex');

    // UUID version 4 consists of 32 hexadecimal digits in the form:
    // 8-4-4-4-12 (total 36 characters including hyphens)
    const uuid = [
        hash.substring(0, 8),
        hash.substring(8, 12),
        '4' + hash.substring(12, 15), // Set the version to 4
        '8' + hash.substring(15, 18), // Set the variant to 8 (RFC 4122)
        hash.substring(18, 30),
    ].join('-');

    return uuid;
}
describe('async call from node.js to .net', function () {

    it(prefix + ' succeeds for hello world', function (done) {
        var func = edge.func({
        	assemblyFile: edgeTestDll,
        	typeName: 'Edge.Tests.Startup',
        	methodName: 'Invoke'
        });
        func('Node.js', function (error, result) {
            assert.ifError(error);
            assert.equal(result, '.NET welcomes Node.js');
            done();
        });
    });

    it(prefix + ' successfuly marshals data from node.js to .net', function (done) {
        var func = edge.func({
            assemblyFile: edgeTestDll,
            typeName: 'Edge.Tests.Startup',
            methodName: 'MarshalIn'
        });
        var payload = {
            a: 1,
            b: 3.1415,
            c: 'fooåäö',
            d: true,
            e: false,
            f: Buffer.alloc(10),
            g: [ 1, 'fooåäö' ],
            h: { a: 'fooåäö', b: 12 },
            i: function (payload, callback) { },
            j: new Date(Date.UTC(2013, 07, 30)),
            k: 65535,
            l: 4294967295,
            m: 18446744073709551615,
            n: 'c',
            o: generateUuidBySeed(),
        };
        func(payload, function (error, result) {
            assert.ifError(error);
            assert.equal(result, 'yes');
            done();
        });
    });

    it(prefix + ' successfuly marshals data from .net to node.js', function (done) {
        var func = edge.func({
            assemblyFile: edgeTestDll,
            typeName: 'Edge.Tests.Startup',
            methodName: 'MarshalBack'
        });
        func(null, function (error, result) {
            assert.ifError(error);
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
            assert.equal(result.k, 65535);
            assert.equal(result.l, 4294967295);
            assert.equal(result.m, 18446744073709551615);
            assert.equal(result.n, 'c');
            assert.equal(result.o, generateUuidBySeed());
            
            done();
        });
    });

    it(prefix + ' successfuly marshals .net exception thrown on v8 thread from .net to node.js', function () {
        var func = edge.func({
            assemblyFile: edgeTestDll,
            typeName: 'Edge.Tests.Startup',
            methodName: 'NetExceptionTaskStart'
        });
        assert.throws(
            func,
            function (error) {
                if ((error instanceof Error) && error.Message.match(/Test .NET exception/)) {
                    return true;
                }
                return false;
            },
            'Unexpected result'
        );
    });

    it(prefix + ' successfuly marshals .net exception thrown on CLR thread from .net to node.js', function (done) {
        var func = edge.func({
            assemblyFile: edgeTestDll,
            typeName: 'Edge.Tests.Startup',
            methodName: 'NetExceptionCLRThread'
        });
        func(null, function (error, result) {
            assert.equal(result, undefined);
            assert.ok(error instanceof Error);
            assert.equal(typeof error.message, 'string');
            assert.ok(error.message.indexOf('Test .NET exception') > -1);
            done();
        });
    });

    it(prefix + ' successfuly marshals structured .net exception from .net to node.js', function (done) {
        var func = edge.func({
        	assemblyFile: edgeTestDll,
        	typeName: 'Edge.Tests.Startup',
        	methodName: 'NetStructuredExceptionCLRThread'
        });

        func(null, function (error, result) {
            assert.ok(error instanceof Error);
            assert.equal(error.message, 'Outer exception');
            assert.equal(error.name, 'System.InvalidOperationException');
            assert.ok(error.InnerException instanceof Error);
            assert.equal(typeof error.InnerException.message, 'string');
            assert.ok(error.InnerException.message.match(/Inner exception/));
            assert.equal(error.InnerException.name, 'System.ArgumentException');
            assert.equal(error.InnerException.ParamName, 'input');
            done();
        });
    });

    it(prefix + ' successfuly marshals empty buffer', function (done) {
        var func = edge.func({
        	assemblyFile: edgeTestDll,
        	typeName: 'Edge.Tests.Startup',
        	methodName: 'MarshalEmptyBuffer'
        });

        func(Buffer.alloc(0), function (error, result) {
            assert.ifError(error);
            assert.ok(result === true);
            done();
        })
    });

    it(prefix + ' successfuly roundtrips unicode characters', function (done) {
        var func = edge.func({
        	assemblyFile: edgeTestDll,
        	typeName: 'Edge.Tests.Startup',
        	methodName: 'ReturnInput'
        });

        var k = "ñòóôõöøùúûüýÿ";
        func(k, function (error, result) {
            assert.ifError(error);
            assert.ok(result === k);
            done();
        })
    });

    it(prefix + ' successfuly roundtrips empty string', function (done) {
        var func = edge.func({
        	assemblyFile: edgeTestDll,
        	typeName: 'Edge.Tests.Startup',
        	methodName: 'ReturnInput'
        });

        var k = "";
        func(k, function (error, result) {
            assert.ifError(error);
            assert.ok(result === k);
            done();
        })
    });

    // Note: This doesn't seem to be sufficient to force the repro of the hang,
    // but it's a good test to make sure works.
    it(prefix + ' successfuly handles process.nextTick in the callback', function (done) {
        var func = edge.func({
            assemblyFile: edgeTestDll,
            typeName: 'Edge.Tests.Startup',
            methodName: 'ReturnInput'
        });

        var k = "";
        func(k, function (error, result) {
            assert.ifError(error);
            assert.ok(result === k);
            process.nextTick(function() {
                done();
            });
        })
    });
});