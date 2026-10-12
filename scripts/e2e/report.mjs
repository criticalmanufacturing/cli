import { access, mkdir, readFile, writeFile } from 'node:fs/promises';
import { randomUUID } from 'node:crypto';
import path from 'node:path';

const repeatOnResume = new Set(['preflight', 'login', 'devcontainer', 'container-login-sync',
  'stack-up', 'installation', 'serve', 'ui-login', 'stop-serve', 'stack-down']);

export async function createReporter(session) {
  const { workspace, state, config, resume } = session;
  let previous;
  if (resume) {
    previous = JSON.parse(await readFile(path.join(state, 'report.json'), 'utf8'));
    if (previous.status !== 'failed' || previous.mesVersion !== config.mesVersion || previous.workspace !== workspace)
      throw new Error('Resume requires a failed report for this workspace and MES version');
    if (!previous.runId || !Array.isArray(previous.steps) || !Array.isArray(previous.commands))
      throw new Error('This report predates resume support; choose a fresh workspace');
  } else {
    try {
      await access(workspace);
      throw new Error(`Workspace already exists: ${workspace}. Choose a fresh workspace in the configuration or move the previous run aside before retrying.`);
    } catch (error) { if (error.code !== 'ENOENT') throw error; }
  }
  const completed = new Set(previous?.steps.filter(step =>
    previous.steps.findLast(item => item.name === step.name)?.status === 'passed').map(step => step.name));
  const report = previous || { runId: randomUUID(), mesVersion: config.mesVersion, workspace,
    startedAt: new Date().toISOString(), steps: [], commands: [], cleanup: [] };
  report.status = 'running';
  report.ci = Boolean(session.ci);
  if (resume) {
    delete report.error; delete report.finishedAt; delete report.uiRecovery;
    (report.resumedAt ||= []).push(new Date().toISOString());
  }
  session.report = report;
  session.workspaceReady = resume;
  let writes = Promise.resolve();

  function save() {
    if (!session.workspaceReady) return Promise.resolve();
    const snapshot = JSON.stringify(report, null, 2);
    writes = writes.then(() => writeFile(path.join(state, 'report.json'), snapshot, { mode: 0o600 }));
    if (session.dashboard) session.dashboard.notify();
    return writes;
  }

  async function createWorkspace() {
    if (!resume) {
      await mkdir(path.dirname(workspace), { recursive: true });
      try { await mkdir(workspace); }
      catch (error) {
        if (error.code === 'EEXIST') throw new Error(`Workspace already exists: ${workspace}. Choose a fresh workspace.`);
        throw error;
      }
      await mkdir(state, { mode: 0o700 });
      session.workspaceReady = true;
      await writeFile(path.join(state, 'commands.log'), report.commands.map(record =>
        `[${record.context}] cwd: ${record.workingDirectory}\n$ ${record.command}\n`).join(''), { mode: 0o600 });
    }
    await save();
  }

  async function step(name, action) {
    if (completed.has(name) && !repeatOnResume.has(name)) {
      console.log(`\n[${name}] already passed; resuming`);
      return;
    }
    const result = { name, status: 'running', startedAt: new Date().toISOString() };
    report.steps.push(result);
    console.log(`\n[${name}]`);
    session.dashboard?.notify();
    await save();
    try { await action(); result.status = 'passed'; }
    catch (error) { result.status = 'failed'; result.error = error.message; throw error; }
    finally { result.finishedAt = new Date().toISOString(); await save(); }
  }
  return { save, step, createWorkspace };
}
