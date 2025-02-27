var edge = require('../lib/edge.js'), assert = require('assert')
    , path = require('path');

const crypto = require('crypto');

var edgeTestDll = path.join(__dirname, '测试', 'Edge.Tests.CoreClr.dll');
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

describe('node.js to .net dll from path with asian chracters', function () {

    it(prefix + ' succeeds for hello world', function (done) {
        if (!process.env.EDGE_USE_CORECLR) {
            this.skip();
        }
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
        if (!process.env.EDGE_USE_CORECLR) {
            this.skip();
        }
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
        if (!process.env.EDGE_USE_CORECLR) {
            this.skip();
        }
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
});