var spawn = require('child_process').spawn;
var path = require('path');
var testDir = path.resolve(__dirname, '../test');
var input = path.resolve(testDir, 'tests.cs');
var output = path.resolve(testDir, 'Edge.Tests.dll');
var buildParameters = ['-target:library', '/debug', '-out:' + output, input];
var mocha = path.resolve(__dirname, '../node_modules/mocha/bin/mocha');
var xunit = path.resolve(__dirname, '../node_modules/xunit-viewer/bin/xunit-viewer');
var fs = require('fs');
const merge = require('junit-report-merger');

if (!process.env.EDGE_USE_CORECLR) {
    run(process.platform === 'win32' ? 'csc' : 'mcs', buildParameters, runOnSuccess, 'net');
}

else {
    run(process.platform === 'win32' ? 'dotnet.exe' : 'dotnet', ['restore'], function(code, signal) {
        if (code === 0) {
            run(process.platform === 'win32' ? 'dotnet.exe' : 'dotnet', ['build'], runOnSuccess, 'coreclr');
        }
    });
}

function run(cmd, args, onClose, signal){

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
        //console.log(result);
        onClose(code, signal);
    });
}

function runOnSuccess(code, framework) {
    if (code === 0) {
        process.env['EDGE_APP_ROOT'] = path.join(testDir, 'bin', 'Debug', 'netcoreapp1.1');

        createJunitReports(framework, false);
        createJunitReports(framework, true);
    }
}

function createJunitReports(framework, createHtml){
    let suffix = createHtml ? '-xunit-viewer' : '';
    spawn('node', [mocha, testDir, '-R', 'mocha-junit-reporter', '-t', '10000', '-gc', '--reporter-options', `mochaFile=./test-results-${framework}${suffix}.xml,testCaseSwitchClassnameAndName=${createHtml ? 'true' : ''}`], {
        stdio: 'inherit'
    }).on('close', function(code) {
        let source = [];
        if(fs.existsSync(`./test-results-coreclr${suffix}.xml`)){
            source.push(`./test-results-coreclr${suffix}.xml`);
        }
        if(fs.existsSync(`./test-results-net${suffix}.xml`)){
            source.push(`./test-results-net${suffix}.xml`);
        }
        merge.mergeFiles(`./test-results${suffix}.xml`, source, function(err) {
            if(err)
            {
                console.log(err)
            }else{
                if(createHtml){
                    spawn('node', [xunit, '--results=test-results-xunit-viewer.xml','--output=test-results-xunit-viewer.html'], {
                        stdio: 'inherit'
                    }).on('close', function(code) {
                    }).on('error', function(err) {
                        console.log(err);
                    });
                }


            }
        })
    }).on('error', function(err) {
        console.log(err);
    });
}

