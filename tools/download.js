var http = require('https');

var urls;
if (process.argv[2] === 'x86') {
	urls = [
		'https://nodejs.org/dist/v' + process.argv[3] + '/node.exe',
		'https://nodejs.org/dist/v' + process.argv[3] + '/win-x86/node.exe'
	];
}
else {
	urls = [
		'https://nodejs.org/dist/v' + process.argv[3] + '/x64/node.exe',
		'https://nodejs.org/dist/v' + process.argv[3] + '/win-x64/node.exe'
	];
}

try_get(0);

function try_get(i) {
	console.log('Trying download from', urls[i]);
	http.get(urls[i], function (res) {
		console.log('HTTP', res.statusCode);
		if (res.statusCode !== 200) {
			if (++i === urls.length)
				throw new Error('Unable to download node.exe');
			else
				try_get(i);
		}

		var stream = require('fs').createWriteStream(process.argv[4] + '/node.exe');
		res.pipe(stream);
	});
}
