try {
	require('../lib/edge.js');
}
catch (e) {
	console.log('***************************************');
	console.log(e);
	console.log('***************************************');
}

console.log('Success: platform check for edge.js: ' + process.platform + '-' + process.arch + ' v' + process.versions.node);
