#! /usr/bin/env node

"use strict";

const { spawn } = require('child_process');
const path = require('path');
const dbg = require('debug');
const { parsePackageJson } = require('./utils');

const opts = parsePackageJson(__dirname);
const debug = dbg("cmf:debug");

debug("Executing cmf-cli");
const exePath = path.join(__dirname, "cmf-cli", opts.binName);
debug("Obtained binary path: " + exePath);

debug(`Spawning cmf-cli from ${exePath} with args ${process.argv.slice(2)} and piping.`);
const child = spawn(exePath, process.argv.slice(2), {stdio: "inherit"});
child.on('close', (code) => {
  debug("Process exited with code " + code);
  process.exitCode = code;
});