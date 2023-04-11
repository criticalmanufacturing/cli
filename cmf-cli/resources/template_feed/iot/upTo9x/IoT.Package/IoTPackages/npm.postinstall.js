var Path = require("path");
var fs = require('fs');
var fileContent = `{ "dependencies": { "graceful-fs": { "version": "4.2.2" }, "ttf2woff2": { "version": "3.0.0" } } }`;
var filePath = Path.join(__dirname, "package-lock");

// Clean package lock backup
fs.exists(`${filePath}-original.json`, (exists) => {
	if (exists) {
		fs.readFile(`${filePath}.json`, { encoding: 'utf-8' }, (err, data) => {
			if (!err) {
				if (data.length === fileContent.length) {
					// It means that the package-lock wasn't changed (npm ci)
					fs.unlinkSync(`${filePath}.json`);
					fs.renameSync(`${filePath}-original.json`, `${filePath}.json`);
				}
				else {
					fs.unlinkSync(`${filePath}-original.json`);
				}
			} else {
				console.log(err);
			}
		});
	}
});