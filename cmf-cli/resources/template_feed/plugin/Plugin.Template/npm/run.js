#! /usr/bin/env node
"use strict";
const { spawn } = require('child_process');
const path = require('path');
const node_modules = require('node_modules-path');
const dbg = require('debug');
const { parsePackageJson, ARCH_MAPPING, PLATFORM_MAPPING } = require('./utils');

const opts = parsePackageJson(__dirname);
const debug = dbg("cmf:debug");

debug(`Executing ${opts.binBaseName}`);
let exePath = null;
debug(`Determining if ${opts.binBaseName} is installed globally or locally...`);
debug(`Getting binary from node_modules/.bin/${opts.binName}...`);
exePath = path.join(node_modules(), ".bin", opts.binBaseName, opts.binName);
debug("Obtained binary path: " + exePath);

debug(`Spawning ${opts.binName} from ${exePath} with args ${process.argv.slice(2)} and piping.`);
const child = spawn(exePath, process.argv.slice(2), {stdio: "inherit"});
child.on('close', (code) => {
  debug("Process exited with code " + code);
  process.exitCode = code;
});