var Path = require("path");
var fs = require('fs');
var fileContent = `{ "dependencies": { "graceful-fs": { "version": "4.2.2" }, "ttf2woff2": { "version": "3.0.0" } } }`;
var filePath = Path.join(__dirname, "package-lock");

// Check if package lock exists and then, create a backup
fs.exists(`${filePath}.json`, (exists) => {
	if (exists) {
		fs.copyFileSync(`${filePath}.json`, `${filePath}-original.json`, (err) => {
			if (err) return console.log(err);
		});
	} 
	fs.writeFileSync(`${filePath}.json`, fileContent, 'utf8', (err) => {
		if (err) return console.log(err);
	});
});