var {http} = require('follow-redirects');


console.log('Trying download from', process.argv[2]);
http.get(process.argv[2], function (res) {
	console.log('HTTP', res.statusCode);
	if (res.statusCode !== 200) {
		throw new Error(`Unable to download ${process.argv[2]}`);
	}

	var stream = require('fs').createWriteStream(process.argv[3]);
	res.pipe(stream);
});

