var spawn = require('child_process').spawn;
var path = require('path');
var testDir = path.resolve(__dirname, '../test');
var input = path.resolve(testDir, 'tests.cs');
var output = path.resolve(testDir, 'Edge.Tests.dll');
var buildParameters = ['-target:library', '/debug', '-out:' + output, input];
var mocha = path.resolve(__dirname, '../node_modules/mocha/bin/mocha');
var fs = require('fs');
const merge = require('junit-report-merger');
const mochawesomeMerge = require('mochawesome-merge');
const marge = require('mochawesome-report-generator')

var runner = process.argv[2];

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
                    run('cp', ['../test/bin/Debug/net6.0/test.dll', '../test/Edge.Tests.CoreClr.dll'], runOnSuccess);
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
        onClose(code, '');
	});
}

function runOnSuccess(code, signal) {
	if (code === 0) {

		process.env['EDGE_APP_ROOT'] = path.join(testDir, 'bin', 'Debug', 'net6.0');

        if(!runner)
        {
            spawn('node', [mocha, testDir, '-R', 'spec', '-t', '10000', '-n', 'expose-gc'], { 
                stdio: 'inherit' 
            }).on('error', function(err) {
                console.log(err); 
            });
            return;
        }

        if(runner === 'all')
        {
            process.platform === 'win32' && !process.env.EDGE_USE_CORECLR ? delete process.env.EDGE_USE_CORECLR : process.env.EDGE_USE_CORECLR = 1
        }

        var framework = process.env.EDGE_USE_CORECLR ? 'coreclr' :'net';
        var config = runner === 'CI' ? 'configCI.json' : 'config.json'

		spawn('node', 
        [   mocha, 
            testDir, 
            '--reporter',  'mocha-multi-reporters',  
            '--reporter-options', `configFile=./test/${config},cmrOutput=mocha-junit-reporter+mochaFile+${framework}:mochawesome+reportFilename+${framework}`,
            '-t', '10000',
            '-n', 'expose-gc'
        ], { 
			stdio: 'inherit' 
		}).on('close', function(code) {
            if(runner === 'all')
            {
                if(!process.env.EDGE_USE_CORECLR){
                    process.env.EDGE_USE_CORECLR = 1;
                    runOnSuccess(code, signal);
                }
                else{
                    mergeFiles();
                }
            }
            else{
                mergeFiles();
            }
		}).on('error', function(err) {
			console.log(err); 
		});;
	}
}

function mergeFiles(){

    if(runner === 'CI' || runner === 'circleci')
    {
        let source = [];
        if(fs.existsSync(`./test-results-coreclr.xml`)){
            source.push(`./test-results-coreclr.xml`);
        }
        if(fs.existsSync(`./test-results-net.xml`)){
            source.push(`./test-results-net.xml`);
        }

        var dir = runner === 'circleci' ? 'junit/' : '';

        merge.mergeFiles(`./${dir}test-results.xml`, source, function(err) {
            if(err)
            {
                console.log(err)
            }
        })
    }

    const options = {
        files: [
            runner === 'CI' || runner === 'circleci' ? './mochawesome-report/*.json': './test/mochawesome-report/*.json',
        ],
    }

    const margeOptions = {
        reportFilename: 'mochawesome.html',
        reportDir: './test/mochawesome-report'
    }
      
    mochawesomeMerge.merge(options).then(report => {
        var file = runner === 'all' ? './test/mochawesome-report/mochawesome.json' : 'mochawesome.json';
        fs.writeFileSync(file, JSON.stringify(report, null, 2))
        console.log(`Mochawesome json created: ${file}`);
        if(runner === 'all')
        {
            marge.create(report, margeOptions).then(() => console.log(`Mochawesome report created: ${margeOptions.reportDir}/${margeOptions.reportFilename}`))
        }
    })
}
