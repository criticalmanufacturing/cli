#! /usr/bin/env node
"use strict";
const { spawn } = require('child_process');
const envPaths = require('env-paths');
const path = require('path');
const fs = require('fs');
const isInstalledGlobally = require('is-installed-globally');
const node_modules = require('node_modules-path');
const { parsePackageJson, ARCH_MAPPING, PLATFORM_MAPPING } = require('./utils');

const opts = parsePackageJson(__dirname);

let exePath = null;
if (isInstalledGlobally) {
  const paths = envPaths("cmf-cli", {suffix: ""});
  exePath = path.join(paths.data, opts.binName);
} else {
  exePath = path.join(node_modules(), ".bin", "cmf-cli", opts.binName);
}
const child = spawn(exePath, process.argv.slice(2), {stdio: "inherit"});
child.on('close', (code) => {
  process.exitCode = code;
});