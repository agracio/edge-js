var spawn = require('child_process').spawn;
var path = require('path');
var testDir = path.resolve(__dirname, '../test');
var input = path.resolve(testDir, 'tests.cs');
var output = path.resolve(testDir, 'Edge.Tests.dll');
var buildParameters = ['-target:library', '/debug', '-out:' + output, input];
var mocha = path.resolve(__dirname, '../node_modules/mocha/bin/mocha');
var fs = require('fs');

if (!process.env.EDGE_USE_CORECLR) {
	if (process.platform !== 'win32') {
		buildParameters = buildParameters.concat(['-sdk:4.5']);
	}

	run(process.platform === 'win32' ? 'csc' : 'mcs', buildParameters, runOnSuccess);
}

else {
    run(process.platform === 'win32' ? 'dotnet.exe' : 'dotnet', ['restore'], function(code, signal) {
        if (code === 0) {
            run(process.platform === 'win32' ? 'dotnet.exe' : 'dotnet', ['build'], function(code, signal) {
                if (code === 0) {
                    run('cp', ['../test/bin/Debug/netcoreapp1.1/test.dll', '../test/Edge.Tests.CoreClr.dll'], runOnSuccess);
                }
            });
        }
    });
}

function run(cmd, args, onClose){

	var params = process.env.EDGE_USE_CORECLR ? {cwd: testDir} : {};
    var command = spawn(cmd, args, params);
    var result = '';
    var error = '';
    command.stdout.on('data', function(data) {
        result += data.toString();
    });
    command.stderr.on('data', function(data) {
        error += data.toString();
    });

    command.on('error', function(err) {
        console.log(error);
        console.log(err);
    });

    command.on('close', function(code){
        console.log(result);
        onClose(code, '');
	});
}

function runOnSuccess(code, signal) {
	if (code === 0) {
		process.env['EDGE_APP_ROOT'] = path.join(testDir, 'bin', 'Debug', 'netcoreapp1.1');
		spawn('node', [mocha, testDir, '-R', 'spec', '-t', '10000', '-gc'], { 
			stdio: 'inherit' 
		}).on('error', function(err) {
			console.log(err); 
		});
	}
}
