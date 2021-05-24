// Thanks to author of https://github.com/sanathkr/go-npm, we were able to modify his code to work with private packages
var _typeof = typeof Symbol === "function" && typeof Symbol.iterator === "symbol" ? function (obj) { return typeof obj; } : function (obj) { return obj && typeof Symbol === "function" && obj.constructor === Symbol && obj !== Symbol.prototype ? "symbol" : typeof obj; };

const path = require('path');
const fs = require('fs');

// Mapping from Node's `process.arch` to dotnet's `RID`
var ARCH_MAPPING = {
    "ia32": "x86",
    "x64": "x64",
    // "arm": "arm" - linux RID? can only find win-arm which is not useful
};

// Mapping between Node's `process.platform` to dotnet's RID
var PLATFORM_MAPPING = {
    "darwin": "osx",
    "linux": "linux",
    "win32": "win"
};

function validateConfiguration(packageJson) {

    if (!packageJson.version) {
        return "'version' property must be specified";
    }

    if (!packageJson.goBinary || _typeof(packageJson.goBinary) !== "object") {
        return "'goBinary' property must be defined and be an object";
    }

    if (!packageJson.goBinary.name) {
        return "'name' property is necessary";
    }

    if (!packageJson.goBinary.path) {
        return "'path' property is necessary";
    }
}

function parsePackageJson(root) {
    if (!(process.arch in ARCH_MAPPING)) {
        console.error("Installation is not supported for this architecture: " + process.arch);
        return;
    }

    if (!(process.platform in PLATFORM_MAPPING)) {
        console.error("Installation is not supported for this platform: " + process.platform);
        return;
    }

    var packageJsonPath = path.join(root, "package.json");
    if (!fs.existsSync(packageJsonPath)) {
        console.error("Unable to find package.json. " + "Please run this script at root of the package you want to be installed");
        return;
    }

    var packageJson = JSON.parse(fs.readFileSync(packageJsonPath));
    var error = validateConfiguration(packageJson);
    if (error && error.length > 0) {
        console.error("Invalid package.json: " + error);
        return;
    }

    // We have validated the config. It exists in all its glory
    var binName = packageJson.goBinary.name;
    var binPath = packageJson.goBinary.path;
    var version = packageJson.version;
    if (version[0] === 'v') version = version.substr(1); // strip the 'v' if necessary v0.0.1 => 0.0.1

    // Binary name on Windows has .exe suffix
    if (process.platform === "win32") {
        binName += ".exe";
    }


    return {
        binName: binName,
        binPath: binPath,
        version: version
    };
}

module.exports = {
    parsePackageJson,
    ARCH_MAPPING,
    PLATFORM_MAPPING
}