#!/usr/bin/env node

"use strict";

const path = require('path'),
    mkdirp = require('mkdirp'),
    rimraf = require('rimraf'),
    fs = require('fs'),
    axios = require('axios'),
    HttpsProxyAgentModule = require('https-proxy-agent'),
    HttpProxyAgentModule = require('http-proxy-agent'),
    proxyFromEnv = require('proxy-from-env'),
    AdmZip = require("adm-zip"),
    tmp = require('tmp'),
    dbg = require('debug'),
    { parsePackageJson, PLATFORM_MAPPING, ARCH_MAPPING } = require('./utils');

const HttpsProxyAgent = HttpsProxyAgentModule.HttpsProxyAgent || HttpsProxyAgentModule;
const HttpProxyAgent = HttpProxyAgentModule.HttpProxyAgent || HttpProxyAgentModule;

const debug = dbg("cmf:debug");
const error = dbg("cmf:debug:error");

async function getInstallationPath() {
    debug("Getting installation path...");
    debug("Targeting node_modules/.bin/cmf-cli. Making sure path exists...");
    // __dirname is always <node_modules>/@criticalmanufacturing/cli, so two
    // levels up is always the node_modules/ dir that owns .bin/ — whether this
    // is a local install (with or without shrinkwrap) or a global install.
    const dir = path.join(path.resolve(__dirname, '..', '..'), ".bin", "cmf-cli");
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
    var opts = parsePackageJson(__dirname);
    if (!opts) return callback(INVALID_INPUT);
    console.info(`Copying the relevant binary for your platform ${process.platform}`);
    const src = path.join(__dirname, "dist", `${PLATFORM_MAPPING[process.platform]}-${ARCH_MAPPING[process.arch]}`);
    const sourceBinaryPath = path.join(src, opts.binName);

    // If dist exists but does not contain the expected binary for this platform,
    // fetch the release archive and hydrate the platform folder.
    if (!fs.existsSync(sourceBinaryPath)) {
        // download respective release zip from github or fallback repositories
        const primaryUrl = opts.binUrl.replace("{{version}}", opts.version).replace("{{platform}}", PLATFORM_MAPPING[process.platform]).replace("{{arch}}", ARCH_MAPPING[process.arch]);
        const versionedFallbackName = `cmf-cli.${PLATFORM_MAPPING[process.platform]}-${ARCH_MAPPING[process.arch]}-${opts.version}.zip`;
        const legacyFallbackName = `cmf-cli.${PLATFORM_MAPPING[process.platform]}-${ARCH_MAPPING[process.arch]}.zip`;
        
        // Fallback URLs for alternative repositories
        const fallbackUrls = [
            `https://criticalmanufacturing.io/repository/tools/${versionedFallbackName}`,
            `https://criticalmanufacturing.io/repository/tools/${legacyFallbackName}`,
            `https://repository.criticalmanufacturing.com.cn/repository/tools/${versionedFallbackName}`,
            `https://repository.criticalmanufacturing.com.cn/repository/tools/${legacyFallbackName}`
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
    try {
        replaceInstallation(src, installPath, opts.binName);
    } catch (err) {
        return callback(err);
    }
    await verifyAndPlaceBinary(opts.binName, installPath, callback);
}

function replaceInstallation(src, installPath, binName) {
    // Stage a complete package before touching the installed version. Merging
    // directories leaves deleted/renamed templates behind after an upgrade.
    const stagingPath = fs.mkdtempSync(path.join(path.dirname(installPath), ".cmf-cli-"));
    const nextPath = path.join(stagingPath, "next");
    const previousPath = path.join(stagingPath, "previous");
    let preserveBackup = false;
    try {
        fs.cpSync(src, nextPath, { recursive: true });
        const binaryPath = path.join(nextPath, binName);
        if (!fs.existsSync(binaryPath)) {
            throw new Error('Downloaded binary does not contain the binary specified in configuration - ' + binName);
        }
        if (process.platform !== "win32") {
            fs.chmodSync(binaryPath, fs.statSync(binaryPath).mode | 0o111);
        }

        fs.renameSync(installPath, previousPath);
        try {
            fs.renameSync(nextPath, installPath);
        } catch (err) {
            try {
                fs.renameSync(previousPath, installPath);
            } catch (restoreError) {
                preserveBackup = true;
                err.message += ` Previous installation preserved at ${previousPath}: ${restoreError.message}`;
            }
            throw err;
        }
    } finally {
        if (!preserveBackup) rimraf.sync(stagingPath);
    }
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

module.exports = { install };

var argv = process.argv;
if (require.main === module && argv && argv.length > 2) {
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
