const fs = require("fs");
const { execSync } = require('child_process');

getVersion();

function getVersion() {

    function getVersionFromMajor(json, major){
        const version = json.find((str) =>  str.startsWith(`${major}.`));
        if(version) return version; 
        throw new Error(`Unable to resolve latest version for Node.js ${major}`);
    }

    let json = JSON.parse(execSync('npm view node versions --json').toString())
    .reverse();

    let major = process.argv[2];
    if(major){
        version = getVersionFromMajor(json, major);
        fs.writeFileSync('node.txt', version);
		return version;
    }
	else{
		throw new Error('No Node.js version provided');
	}
}

