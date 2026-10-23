import { parseArguments, loadConfig, usage } from './config.mjs';
import { createRuntime } from './runtime.mjs';
import { executeWorkflow } from './flow.mjs';

async function main() {
  if (process.argv.includes('--help')) {
    console.log(`${usage}\nRuns login, project initialization, packages, local stack, browser checks, and teardown.\nA live dashboard starts automatically; use --no-dashboard to disable it.\nSee README.md for configuration and prerequisites.`);
    return;
  }
  if (Number(process.versions.node.split('.')[0]) < 20)
    throw new Error(`The helper requires Node 20 or later (running ${process.versions.node}). Inside the CLI devcontainer, run "nvm use 20", then retry.`);
  const options = parseArguments(process.argv.slice(2));
  const config = await loadConfig(options);
  const runtime = await createRuntime(config, options);
  const report = await executeWorkflow(runtime);
  if (report.status !== 'passed') process.exitCode = 1;
}

main().catch(error => { console.error(error.message); process.exitCode = 1; });
