
var edge = require('../lib/edge.js'), assert = require('assert')
    , path = require('path');

var edgeTestDll = process.env.EDGE_USE_CORECLR ? 'test' : path.join(__dirname, 'Edge.Tests.dll');

describe('async call from .net to node.js using external C# code', function () {

    it('succeeds for hello world', function (done) {
        var func = edge.func({
            source: __dirname + '/106_node2net_cs.cs',
            references: [
                edgeTestDll
            ],
            methodName: 'Test'
        });
        func({}, function (error, result) {
            assert.ifError(error);
            assert.equal(result, 'Node.js welcomes .NET');
            done();
        });
    });

});

