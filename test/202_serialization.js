var edge = require('../lib/edge.js'), assert = require('assert'), path = require('path');

var edgeTestDll = process.env.EDGE_USE_CORECLR ? 'test' : path.join(__dirname, 'Edge.Tests.dll');

describe('serialization', function () {

    if (!process.env.EDGE_USE_CORECLR) {
        it('complex exception serialization', function (done) {
            var func = edge.func({
                source: function () {/*
                 #r "System.Data.dll"

                 using System.Data;
                 using System.Data.SqlClient;

                 async (input) =>
                 {

                 using (SqlConnection connection = new SqlConnection("Data Source=my_localhost;Initial Catalog=catalog;Integrated Security=True;Connection Timeout=1"))
                 {
                 connection.Open();
                 }
                 return input.ToString();

                 }
                 */
                }
            });
            func("JavaScript", function (error, result) {
                var exception = error.toString();
                var contains = exception.indexOf('A network-related or instance-specific error occurred while establishing a connection to SQL Server') !== -1
                    || exception.indexOf('Server does not exist or connection refused') !== -1;

                assert.ok(contains);

                done();
            });
        });
    }

});