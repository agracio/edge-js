const fs = require("fs");
const { execSync } = require('child_process');
const http = require('http');

getVersion();

function getVersion() {
	let url = 'http://nodejs.org/dist/index.json';
    let major = process.argv[2];
    if(!major){
        throw new Error('No Node.js version provided');
    }

	http.get(url,(res) => {
		let body = "";

		if (res.statusCode !== 200) {
			throw new Error(`Unable to get Node.js versions from ${url}`);
		}
	
		res.on("data", (chunk) => {
			body += chunk;
		});
	
		res.on("end", () => {
			try {
				let json = JSON.parse(body);

				for (const el of json.sort()) {
					let version = el.version.substring(1, el.version.length) ;
					if(version.startsWith(process.argv[2])){
						console.log(version);
                        fs.writeFileSync('node.txt', version);
						return version;
					}
				}
			} catch (error) {
				throw error;
			};
		});
	
	}).on("error", (error) => {
		throw new Error(error);
	});
}

//getVersion();

// function getVersion() {

//     function getVersionFromMajor(json, major){
//         const version = json.find((str) =>  str.startsWith(`${major}.`));
//         if(version) return version; 
//         throw new Error(`Unable to resolve latest version for Node.js ${major}`);
//     }

//     let json = JSON.parse(execSync('npm view node versions --json').toString())
//     .reverse();

//     let major = process.argv[2];
//     if(major){
//         version = getVersionFromMajor(json, major);
//         fs.writeFileSync('node.txt', version);
// 		return version;
//     }
// 	else{
// 		throw new Error('No Node.js version provided');
// 	}
// }

exports = module.exports = getVersion;

