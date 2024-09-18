module.exports = async ({github, context, core}) => {
	const {http} = require('follow-redirects');
    let url = 'http://nodejs.org/dist/index.json';

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
					if(version.startsWith('20')){
						console.log(version)
						core.setOutput("version", version);
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