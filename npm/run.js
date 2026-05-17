#! /usr/bin/env node

"use strict";

const { spawn } = require('child_process');
const path = require('path');
const dbg = require('debug');
const { parsePackageJson } = require('./utils');

const opts = parsePackageJson(__dirname);
const debug = dbg("cmf:debug");

debug("Executing cmf-cli");
debug("Getting binary from node_modules/.bin/cmf-cli...");
// __dirname is always <node_modules>/@criticalmanufacturing/cli, so two levels
// up is the node_modules/ that owns .bin/ — correct for local and global installs.
const exePath = path.join(path.resolve(__dirname, '..', '..'), ".bin", "cmf-cli", opts.binName);
debug("Obtained binary path: " + exePath);

debug(`Spawning cmf-cli from ${exePath} with args ${process.argv.slice(2)} and piping.`);
const child = spawn(exePath, process.argv.slice(2), {stdio: "inherit"});
child.on('close', (code) => {
  debug("Process exited with code " + code);
  process.exitCode = code;
});
