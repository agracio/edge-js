const fs = require('fs')
	, path = require('path')
	, spawn = require('child_process').spawn
	, whereis = require('./whereis');

const checkMono = require('./checkMono');

if (process.platform === 'win32') {
	const libroot = path.resolve(__dirname, '../lib/native/win32')
		, lib32bit = path.resolve(libroot, 'ia32')
		, lib64bit = path.resolve(libroot, 'x64')
		, libarm64 = path.resolve(libroot, 'arm64');

	function copyFile(filePath, filename) {
		return function(copyToDir) {
			//console.log( 'copy '+filename+' from '+filePath+' to '+ copyToDir );
			let outFile = path.resolve(copyToDir, filename);
			if ( fs.existsSync( outFile ) ) {
				// clear readonly: add write permission to ogw (222 octal -> 92 hex -> 146 decimal)
				fs.chmodSync( outFile, fs.statSync(outFile).mode | 146 )
			}
			fs.writeFileSync(path.resolve(copyToDir, filename), fs.readFileSync(filePath));
		};
	}

	function isDirectory(info) {
		return info.isDirectory;
	}

	function getInfo(basedir) {
		return function(file) {
			var filepath = path.resolve(basedir, file);

			return {
				path: filepath,
				isDirectory: fs.statSync(filepath).isDirectory()
			};
		}
	}

	function getPath(info) {
		return info.path;
	}

	function getDestDirs(basedir){
		return fs.readdirSync(basedir)
		.map(getInfo(basedir))
		.filter(isDirectory)
		.map(getPath);
	}

	const redist = [
        'concrt140.dll',
        'msvcp140.dll',
        'vccorlib140.dll',
        'vcruntime140.dll',
	];

	const dest32dirs = getDestDirs(lib32bit);
	const dest64dirs = getDestDirs(lib64bit);
	const destarmdirs = getDestDirs(libarm64);

	function copyRedist(lib, destDirs){
		redist.forEach(function (dllname) {
			var dll = path.resolve(lib, dllname);
			destDirs.forEach(copyFile(dll, dllname));
		});
	}

	copyRedist(lib32bit, dest32dirs);
	copyRedist(lib64bit, dest64dirs);
	copyRedist(libarm64, destarmdirs);

	const dotnetPath = whereis('dotnet', 'dotnet.exe');

	if (dotnetPath) {
		spawn(dotnetPath, ['restore'], { stdio: 'inherit', cwd: path.resolve(__dirname, '..', 'lib', 'bootstrap') })
			.on('close', function() {
				spawn(dotnetPath, ['build', '--configuration', 'Release'], { stdio: 'inherit', cwd: path.resolve(__dirname, '..', 'lib', 'bootstrap') })
					.on('close', function() {
						require('./checkplatform');
					});
			});
	}

	else {
		require('./checkplatform');
	}
} 

else {
	if(process.platform === 'darwin'){
  
		const nodeVersion = process.versions.node.split(".")[0]
		const edjeNative = path.resolve(__dirname, '../lib/native/' + process.platform + '/' + process.arch + '/' + nodeVersion + '/' + 'edge_nativeclr.node');
		const edjeNativeClr = path.resolve(__dirname, '../lib/native/' + process.platform + '/' + process.arch + '/' + nodeVersion + '/' + 'edge_coreclr.node');

		const checkNative = checkMono() ? fs.existsSync(edjeNative) : true;

		if(checkNative && fs.existsSync(edjeNativeClr)){

			if(fs.existsSync(path.resolve(__dirname, '../build'))){
				fs.rmSync(path.resolve(__dirname, '../build'), { recursive: true, force: true });
			}
			
			spawn('dotnet', ['build', '--configuration', 'Release'], { stdio: 'inherit', cwd: path.resolve(__dirname, '..', 'lib', 'bootstrap') })

		}
		else{
			spawn('node-gyp', ['configure', 'build'], { stdio: 'inherit' });
		}
	}
	else{
		spawn('node-gyp', ['configure', 'build'], { stdio: 'inherit' });
	}
}
