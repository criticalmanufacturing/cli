#!/usr/bin/env node

"use strict";

const path = require('path'),
    mkdirp = require('mkdirp'),
    rimraf = require('rimraf'),
    fs = require('fs'),
    axios = require('axios'),
    HttpsProxyAgent = require('https-proxy-agent'),
    HttpProxyAgent = require('http-proxy-agent'),
    proxyFromEnv = require('proxy-from-env'),
    AdmZip = require("adm-zip"),
    tmp = require('tmp'),
    dbg = require('debug'),
    node_modules = require('node_modules-path'),
    { parsePackageJson, PLATFORM_MAPPING, ARCH_MAPPING } = require('./utils');

const debug = dbg("cmf:debug");
const error = dbg("cmf:debug:error");

async function getInstallationPath() {
    debug("Getting installation path...");
    debug("Targeting node_modules/.bin/cmf-cli. Making sure path exists...");
    const dir = path.join(node_modules(), ".bin", "cmf-cli");
    await mkdirp(dir);
    debug(`Install path exists!`);
    return dir;
}

async function verifyAndPlaceBinary(binName, binPath, callback) {
    if (!fs.existsSync(path.join(binPath, binName))) return callback('Downloaded binary does not contain the binary specified in configuration - ' + binName);
    return callback(null);
}

/**
 * Downloads and extracts a zip file from the given URL to the destination path
 * @param {string} pkgUrl - The URL to download from
 * @param {string} dest - The destination path to extract to
 * @returns {Promise<void>}
 */
async function downloadAndExtract(pkgUrl, dest) {
    console.info(`Fetching release archive from ${pkgUrl}`);
    
    // support for http/s proxies through env vars
    const proxy = proxyFromEnv.getProxyForUrl(pkgUrl);
    let httpAgent, httpsAgent;
    if (proxy) {
        httpAgent = new HttpProxyAgent(proxy);
        httpsAgent = new HttpsProxyAgent(proxy);
    }

    // make req (override axios automatic proxy since it is not working properly)
    const response = await axios({
        url: pkgUrl,
        method: 'GET',
        proxy: false,
        httpAgent: httpAgent,
        httpsAgent: httpsAgent,
        responseType: 'arraybuffer',
    });
    
    const zip = tmp.tmpNameSync();
    debug(`Writing temporary zip file to ${zip}`);
    fs.writeFileSync(zip, response.data);
    debug(`Extracting zip file ${zip} to ${dest}`);
    (new AdmZip(zip)).extractAllTo(dest);
    rimraf.sync(zip);
}

/**
 * Reads the configuration from application's package.json,
 * validates properties, copied the binary from the package and stores at
 * ./bin in the package's root. NPM already has support to install binary files
 * specific locations when invoked with "npm install -g"
 *
 *  See: https://docs.npmjs.com/files/package.json#bin
 */
var INVALID_INPUT = "Invalid inputs";
async function install(callback) {
    var opts = parsePackageJson(".");
    if (!opts) return callback(INVALID_INPUT);
    console.info(`Copying the relevant binary for your platform ${process.platform}`);
    const src= `./dist/${PLATFORM_MAPPING[process.platform]}-${ARCH_MAPPING[process.arch]}`;

    if (!fs.existsSync("./dist")) {
        // download respective release zip from github or fallback repositories
        const primaryUrl = opts.binUrl.replace("{{version}}", opts.version).replace("{{platform}}", PLATFORM_MAPPING[process.platform]).replace("{{arch}}", ARCH_MAPPING[process.arch]);
        
        // Fallback URLs for alternative repositories
        const fallbackUrls = [
            `https://criticalmanufacturing.io/repository/tools/cmf-cli.${PLATFORM_MAPPING[process.platform]}-${ARCH_MAPPING[process.arch]}.zip`,
            `https://repository.criticalmanufacturing.com.cn/repository/tools/cmf-cli.${PLATFORM_MAPPING[process.platform]}-${ARCH_MAPPING[process.arch]}.zip`
        ];
        
        // Try downloading from primary URL and fallbacks
        const allUrls = [primaryUrl, ...fallbackUrls];
        let downloadSucceeded = false;
        
        for (const pkgUrl of allUrls) {
            try {
                await downloadAndExtract(pkgUrl, src);
                console.info(`Package downloaded successfully from ${pkgUrl}`);
                downloadSucceeded = true;
                break;
            } catch (e) {
                error(`Failed to download from ${pkgUrl}: ${e.message}`);
            }
        }
        
        if (!downloadSucceeded) {
            callback(`Could not install version ${opts.version} on your platform ${process.platform}/${process.arch} from any repository.`);
            return;
        }
    }

    const installPath = await getInstallationPath();
    if (process.platform === "win32") {
        debug("Installing for windows");
        await execShellCommand(`robocopy ${src.replace(/\//g, "\\")} "${installPath}" /e /is /it`, [1]);
    } else {
        debug("Installing for *NIX: " + process.platform);
        await execShellCommand(`cp -r ${src}/** "${installPath}"`);
        await execShellCommand(`chmod +x "${installPath}/cmf"`);
    }
    await verifyAndPlaceBinary(opts.binName, installPath, callback);
}

async function uninstall(callback) {
    try {
        const installPath = await getInstallationPath();
        debug("Deleting binaries from " + installPath);
        rimraf.sync(installPath);
    } catch (ex) {
        console.log(ex);
        callback(ex);
        // Ignore errors when deleting the file.
    }
    console.info("Uninstalled cli successfully");
    return callback(null);
}

// Parse command line arguments and call the right method
var actions = {
    "install": install,
    "uninstall": uninstall
};

/**
 * Executes a shell command and return it as a Promise.
 * @param cmd {string}
 * @return {Promise<string>}
 */
function execShellCommand(cmd, valid_error_codes = null) {
    const exec = require('child_process').exec;
    return new Promise((resolve, reject) => {
        exec(cmd, (error, stdout, stderr) => {
            if (error) {
                let shouldPrint = true;
                if (valid_error_codes != null) {
                    shouldPrint = valid_error_codes.indexOf(error.code) < 0;
                }
                shouldPrint && console.warn(error);
            }
            resolve(stdout? stdout : stderr);
        });
    });
}

var argv = process.argv;
if (argv && argv.length > 2) {
    var cmd = process.argv[2];
    if (!actions[cmd]) {
        console.log("Invalid command. `install` and `uninstall` are the only supported commands");
        process.exit(1);
    }

    actions[cmd](function (err) {
        if (err) {
            console.error(err);
            process.exit(1);
        } else {
            process.exit(0);
        }
    });
}
