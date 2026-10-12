// Exercises orchestration with fake CLI/Docker commands and a real headless browser.
import test from 'node:test';
import assert from 'node:assert/strict';
import { mkdtemp, mkdir, writeFile, readFile, rm } from 'node:fs/promises';
import { createServer } from 'node:http';
import { spawn } from 'node:child_process';
import path from 'node:path';
import os from 'node:os';

async function listen(server) {
  await new Promise(resolve => server.listen(0, '127.0.0.1', resolve));
  return server.address().port;
}
async function executable(file, source) {
  await writeFile(file, `#!/usr/bin/env node\n${source}`, { mode: 0o755 });
}

test('full flow uses the candidate, authenticates, packs all packages, and stops serve', { timeout: 180000 }, async () => {
  const root = await mkdtemp(path.join(os.tmpdir(), 'cmf-flow-smoke-'));
  let loginCount = 0;
  let requireReset = true;
  let resetCount = 0;
  let rejectScopes = false;
  const http = createServer((request, response) => {
    response.setHeader('Content-Type', 'text/html');
    if (request.url === '/deployment/status') {
      response.setHeader('Content-Type', 'application/json');
      response.end(JSON.stringify({ Message: 'Deployment finished successfully' }));
    } else if (request.method === 'POST') {
      let body = '';
      request.on('data', chunk => { body += chunk; });
      request.on('end', () => {
        assert.equal(new URLSearchParams(body).get('password'), 'admin');
        if (request.url === '/reset') {
          assert.equal(new URLSearchParams(body).get('password_confirmation'), 'admin');
          requireReset = false; resetCount++;
          response.writeHead(302, { Location: '/' }); response.end();
          return;
        }
        assert.equal(new URLSearchParams(body).get('username'), 'admin');
        loginCount++;
        response.writeHead(302, { Location: rejectScopes ? '/scope-error' : requireReset ? '/reset' : '/home' }); response.end();
      });
    } else if (request.url === '/reset') response.end('<form method="POST"><input name="password" type="password">' +
      '<input name="password_confirmation" type="password"><button>Reset</button></form>');
    else if (request.url === '/home') {
      // Matches the MES 10 sidebar, including a hidden duplicate label.
      setTimeout(() => response.end('<div style="display:none">Administration</div>' +
        '<div class="menu-sub-menu"><div data-id="Shell.Administration">Administration</div>' +
        '<div data-id="Shell.BusinessData">Business Data</div></div>'), 150);
    }
    else response.end('<form method="POST"><input name="username"><input name="password" type="password">' +
      '<button>Login</button></form>' + (request.url === '/scope-error' ?
        "<div>The user doesn't have all the required scopes.</div>" : ''));
  });
  const uiPort = await listen(http);
  const portProbe = createServer();
  const servePort = await listen(portProbe);
  await new Promise(resolve => portProbe.close(resolve));
  try {
    const bundle = path.join(root, 'bundle');
    const bin = path.join(root, 'bin');
    await mkdir(bundle); await mkdir(bin);
    await executable(path.join(bin, 'docker'), 'console.log("fixture Docker ready");');
    await executable(path.join(bin, 'devcontainer'), `
      const {spawnSync} = require('node:child_process');
      const fs = require('node:fs');
      const args = process.argv.slice(2);
      if (process.env.FIXTURE_FAIL_PREFLIGHT) {console.error('fixture missing prerequisite');process.exit(27);}
      if (args[0] === 'read-configuration') console.log(JSON.stringify({configuration:{image:'fixture',runArgs:[]}}));
      else if (args[0] === 'up') console.log(JSON.stringify({outcome:'success', containerId:'fixture', remoteWorkspaceFolder:args[2]}));
      else if (args[0] === 'exec') {
        const index = args.indexOf('node');
        const child = spawnSync(args[index], args.slice(index+1), {stdio:'inherit', env:process.env});
        process.exit(child.status ?? 1);
      } else console.log('fixture-devcontainer');
    `);
    await executable(path.join(bundle, 'cmf'), `
      const fs = require('node:fs'); const path = require('node:path');
      const args = process.argv.slice(2);
      fs.appendFileSync(process.env.FIXTURE_COMMANDS, JSON.stringify({args,cwd:process.cwd(),exe:process.argv[1]})+'\\n');
      const write = (file, value) => fs.writeFileSync(file, JSON.stringify(value));
      if (args[0] === '--version') console.log('fixture-cli-1.0.0');
      else if (args[0] === 'portal' && args[1] === 'login') {
        fs.mkdirSync(path.join(process.env.XDG_CONFIG_HOME,'cmfportal'),{recursive:true});
        fs.writeFileSync(path.join(process.env.XDG_CONFIG_HOME,'cmfportal/cmfportaltoken'),'fixture-secret-token');
      }
      else if (args[0] === 'init') {
        fs.mkdirSync('.devcontainer'); write('.devcontainer/devcontainer.json', {image:'fixture'});
        write('cmfpackage.json', {packageId:'Root',version:'1.0.0',packageType:'Root'});
        write('repositories.json', {ciRepo:'fixture'});
      } else if (args[0] === 'new') {
        const dir = 'Cmf.Custom.' + (args[1] === 'test' ? 'Tests' : args[1] === 'html' ? 'Html' : args[1][0].toUpperCase()+args[1].slice(1));
        fs.mkdirSync(dir);
        write(path.join(dir,'cmfpackage.json'), {packageId:dir,version:'1.0.0',packageType:args[1]});
        write(path.join(dir,'package.json'), {scripts:{serve:'node server.cjs'}});
        fs.writeFileSync(path.join(dir,'server.cjs'), "require('node:http').createServer((q,s)=>s.end('custom UI')).listen("+process.env.FIXTURE_SERVE_PORT+",'127.0.0.1');");
      } else if (args.includes('init') && args[0] === 'dev') fs.writeFileSync('compose.local.yaml','services: {}');
      else if (args[0] === 'dev' && args.includes('lbos')) {
        fs.mkdirSync('Libs/LBOs/NetStandard',{recursive:true});
        fs.mkdirSync('Libs/LBOs/TypeScript',{recursive:true});
        fs.writeFileSync('Libs/LBOs/NetStandard/Cmf.LightBusinessObjects.dll','fixture library');
        write('Libs/LBOs/TypeScript/package.json', {name:'cmf-lbos'});
      }
      else if (args.includes('down') && process.env.FIXTURE_FAIL_DOWN) process.exit(24);
      else if (args[0] === 'build' && process.env.FIXTURE_FAIL_BUILD) process.exit(23);
      else if (args[0] === 'pack') {
        fs.mkdirSync(args[2],{recursive:true}); fs.writeFileSync(path.join(args[2],'fixture.zip'),'archive');
      }
    `);
    async function run(name, fail = false, failUi = false, failPreflight = false, existing = false, resume = false, failDown = false) {
      const workspace = path.join(root, name);
      if (existing) {
        await mkdir(workspace);
        await writeFile(path.join(workspace,'keep.txt'),'existing project');
      }
      const configFile = path.join(root, `${name}.json`);
      await writeFile(configFile, JSON.stringify({ mesVersion: '11.0.0', licenseId: '123', portalPat:'fixture-secret-pat',
        ciRepo:'https://example.invalid/npm-ci/',
        releaseRepos:['https://example.invalid/releases-one/', 'https://example.invalid/releases-two/'],
        workspace, cliBundle:bundle,
        commandTimeoutSeconds:10, installationTimeoutSeconds:10, uiTimeoutSeconds:10,
        deploymentUrl:`http://127.0.0.1:${uiPort}/deployment/status`, uiUrl:`http://127.0.0.1:${uiPort}`,
        serveUrl:`http://127.0.0.1:${servePort}`,
        ...(failUi ? {login:{sidebarSelector:'.missing-sidebar'}} : {}) }));
      let output = '';
      const code = await new Promise(resolve => {
        const child = spawn(process.execPath, ['run.mjs','--config',configFile, ...(resume ? ['--resume'] : [])], {
          cwd: import.meta.dirname, env:{...process.env, PATH:`${bin}:${process.env.PATH}`,
            FIXTURE_COMMANDS:path.join(root,`${name}-commands.jsonl`), FIXTURE_SERVE_PORT:String(servePort),
            FIXTURE_FAIL_BUILD:fail ? '1' : '', FIXTURE_FAIL_PREFLIGHT:failPreflight ? '1' : '',
            XDG_CONFIG_HOME:path.join(root,'config'), CMF_E2E_PORTAL_TOKEN:'', CMF_E2E_PORTAL_PAT:'',
            FIXTURE_FAIL_DOWN:failDown ? '1' : ''},
          stdio:['ignore','pipe','pipe'],
        });
        child.stdout.on('data',chunk => {output+=chunk;}); child.stderr.on('data',chunk => {output+=chunk;});
        child.on('close',resolve);
      });
      let report = null;
      try { report = JSON.parse(await readFile(path.join(workspace,'.e2e/report.json'),'utf8')); }
      catch (error) {if(error.code !== 'ENOENT') throw error;}
      return {code, report, output, workspace};
    }
    const preflightFailure = await run('preflight-failure', false, false, true);
    assert.equal(preflightFailure.code, 1);
    assert.equal(preflightFailure.report, null);
    assert.match(preflightFailure.output, /No workspace was created/);
    await assert.rejects(readFile(path.join(preflightFailure.workspace,'.e2e/report.json')), {code:'ENOENT'});
    const preflightRetry = await run('preflight-failure', false, false, true);
    assert.doesNotMatch(preflightRetry.output, /Workspace already exists/);
    const existing = await run('existing', false, false, false, true);
    assert.equal(existing.code, 1);
    assert.match(existing.output, /Workspace already exists.*Choose a fresh workspace/);
    assert.doesNotMatch(existing.output, /\[preflight\]|EEXIST|UncaughtException/);
    assert.equal(await readFile(path.join(existing.workspace,'keep.txt'),'utf8'), 'existing project');
    const success = await run('success');
    assert.equal(success.code, 0, success.output);
    assert.equal(success.report.status, 'passed');
    assert.equal(success.report.steps.filter(step => step.name.startsWith('pack-')).length, 6);
    assert.equal(loginCount, 2);
    assert.equal(resetCount, 1);
    assert.match(success.output, /UI: initial password confirmation completed/);
    await readFile(path.join(success.workspace,'.e2e/homepage.png'));
    await assert.rejects(fetch(`http://127.0.0.1:${servePort}`));
    const commands = (await readFile(path.join(root,'success-commands.jsonl'),'utf8')).trim().split('\n').map(JSON.parse);
    const syncIndex = commands.findIndex(command => command.args[0] === 'login' && command.args[1] === 'sync');
    assert.ok(syncIndex >= 0 && syncIndex < commands.findIndex(command => command.args[0] === 'new'));
    assert.ok(commands[syncIndex].exe.startsWith(path.join(success.workspace,'.e2e/cli')));
    const commandLog = await readFile(path.join(success.workspace,'.e2e/commands.log'),'utf8');
    assert.match(commandLog, /\[devcontainer\] cwd: .*\n\$ .*cmf login sync/);
    assert.match(commandLog, /\$ .*cmf new business/);
    assert.match(commandLog, /\$ devcontainer --version/);
    assert.match(await readFile(path.join(success.workspace,'.e2e/serve.log'),'utf8'), /\$ npm run serve \(background\)/);
    const portalLogin = commands.find(command => command.args[0] === 'portal');
    assert.deepEqual(portalLogin.args, ['portal', 'login', '--pat', 'fixture-secret-pat']);
    for (const artifact of [success.output, commandLog, JSON.stringify(success.report)])
      assert.ok(!artifact.includes('fixture-secret-pat'));
    assert.ok(!success.output.includes('fixture-secret-token'));
    assert.ok(!commandLog.includes('fixture-secret-token'));
    assert.ok(!JSON.stringify(success.report).includes('fixture-secret-token'));
    const containerConfig = JSON.parse(await readFile(path.join(success.workspace,'.e2e/devcontainer.json'),'utf8'));
    assert.ok(containerConfig.runArgs.includes(`127.0.0.1:${uiPort}:80`));
    assert.ok(containerConfig.runArgs.includes(`127.0.0.1:${servePort}:7000`));
    const initArgs = commands.find(command => command.args[0] === 'init').args;
    assert.equal(initArgs[initArgs.indexOf('--ciRepo') + 1], 'https://example.invalid/npm-ci/');
    assert.deepEqual(initArgs.slice(initArgs.indexOf('--releaseRepos') + 1),
      ['https://example.invalid/releases-one/', 'https://example.invalid/releases-two/']);
    const stackArgs = commands.find(command => command.args.includes('up')).args;
    assert.equal(stackArgs[stackArgs.indexOf('--licenseId') + 1], '123');
    const builds = commands.filter(command => command.args[0] === 'build');
    const lbosIndex = commands.findIndex(command => command.args.includes('lbos'));
    assert.ok(lbosIndex >= 0 && lbosIndex < commands.findIndex(command => command.args[0] === 'build' && command.cwd.endsWith('Tests')));
    assert.equal(builds.length, 6);
    assert.ok(builds.every(command => command.exe.startsWith(path.join(success.workspace,'.e2e/cli'))));
    assert.ok(builds.every(command => path.basename(command.cwd).startsWith('Cmf.Custom.')));
    assert.equal(success.report.steps.at(-1).name, 'stack-down');
    assert.deepEqual(commands.at(-1).args, ['dev', 'local', 'stack', 'down', '--remove-volumes']);
    for (const command of success.report.commands.filter(command => command.context === 'devcontainer')) {
      if (command.command.includes('cmf new ')) assert.ok(command.step.startsWith('scaffold-'));
      if (command.command.includes('cmf build')) assert.ok(command.step.startsWith('build-'));
      if (command.command.includes('cmf pack')) assert.ok(command.step.startsWith('pack-'));
    }
    const failure = await run('failure', true);
    assert.equal(failure.code, 1);
    assert.equal(failure.report.steps.find(step => step.name === 'build-business').status, 'failed');
    assert.ok(!failure.report.steps.some(step => step.name === 'ui-login' || step.name === 'stack-down'));
    assert.equal(loginCount, 2, 'failed build must prevent login and packaging');
    const resumed = await run('failure', false, false, false, false, true);
    assert.equal(resumed.code, 0, resumed.output);
    assert.equal(resumed.report.status, 'passed');
    assert.equal(resumed.report.resumedAt.length, 1);
    const resumedCommands = (await readFile(path.join(root,'failure-commands.jsonl'),'utf8')).trim().split('\n').map(JSON.parse);
    assert.equal(resumedCommands.filter(command => command.args[0] === 'init').length, 1);
    assert.equal(resumedCommands.filter(command => command.args[0] === 'new').length, 6);
    const failedUi = await run('failed-ui', false, true);
    assert.equal(failedUi.code, 1);
    assert.equal(failedUi.report.steps.at(-1).name, 'ui-login');
    assert.deepEqual(failedUi.report.cleanup, ['Serve process stopped']);
    await readFile(path.join(failedUi.workspace,'.e2e/ui-failure.png'));
    await assert.rejects(fetch(`http://127.0.0.1:${servePort}`));
    const failedDown = await run('failed-down', false, false, false, false, false, true);
    assert.equal(failedDown.code, 1);
    assert.equal(failedDown.report.steps.at(-1).name, 'stack-down');
    assert.equal(failedDown.report.steps.at(-1).status, 'failed');
    assert.equal(failedDown.report.steps.filter(step => step.name.startsWith('pack-') && step.status === 'passed').length, 6);
    rejectScopes = true;
    const failedAuth = await run('failed-auth');
    assert.equal(failedAuth.code, 1);
    assert.match(failedAuth.report.error, /MES rejected login:.*required scopes/);
    assert.ok(!failedAuth.report.steps.some(step => step.name === 'stack-down'));
    assert.deepEqual(failedAuth.report.cleanup, ['Serve process stopped']);
    await readFile(path.join(failedAuth.workspace,'.e2e/ui-failure.png'));
  } finally {
    await new Promise(resolve => http.close(resolve));
    await rm(root, {recursive:true,force:true});
  }
});
