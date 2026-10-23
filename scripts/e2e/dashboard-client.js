let state = INITIAL;
let selected;
let followActive = true;
let lastScreenshot = '';
const laneViews = new Map();
const esc = value => String(value ?? '').replace(/[&<>"']/g, character =>
  ({ '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#39;' })[character]);
const latest = name => state.steps.find(step => step.name === name);
function stepPrerequisites() {
  const prerequisites = {};
  const [setup, packages, installation, browser] = state.flow;
  for (let index = 1; index < setup.steps.length; index++)
    prerequisites[setup.steps[index]] = [setup.steps[index - 1]];

  const scaffoldSteps = packages.steps.filter(name => name.startsWith('scaffold-'));
  for (let index = 0; index < scaffoldSteps.length; index++)
    prerequisites[scaffoldSteps[index]] = [index ? scaffoldSteps[index - 1] : setup.steps.at(-1)];

  const packageSteps = packages.steps.filter(name => !name.startsWith('scaffold-'));
  for (let index = 0; index < packageSteps.length; index++) {
    const name = packageSteps[index];
    prerequisites[name] = index ? [packageSteps[index - 1]] : [scaffoldSteps.at(-1)];
    if (name === 'build-test') prerequisites[name].push('generate-lbos');
  }

  prerequisites[installation.steps[0]] = [setup.steps.at(-1)];
  for (let index = 1; index < installation.steps.length; index++)
    prerequisites[installation.steps[index]] = [installation.steps[index - 1]];
  prerequisites['generate-lbos'] = [scaffoldSteps.at(-1), 'installation'];
  prerequisites[browser.steps[0]] = ['build-html'];
  for (let index = 1; index < browser.steps.length; index++)
    prerequisites[browser.steps[index]] = [browser.steps[index - 1]];
  prerequisites['stack-down'] = [packageSteps.at(-1), 'generate-lbos', browser.steps.at(-1)];
  return prerequisites;
}
const visualStatus = step => state.report.status === 'failed' && step?.status === 'running' ? 'cancelled' : step?.status || 'waiting';
const icon = status => ({ passed: '✓', failed: '×', running: '◉', cancelled: '—', waiting: '◷' })[status] || '◷';
const formatElapsed = seconds => seconds < 60 ? `${seconds}s` : seconds < 3600
  ? `${Math.floor(seconds / 60)}m ${seconds % 60}s`
  : `${Math.floor(seconds / 3600)}h ${Math.floor(seconds % 3600 / 60)}m ${seconds % 60}s`;
function totalRunTime(report) {
  const startedAt = report.resumedAt?.at(-1) || report.startedAt;
  if (!startedAt) return '0s';
  const end = report.finishedAt ? new Date(report.finishedAt) : new Date();
  return formatElapsed(Math.max(0, Math.round((end - new Date(startedAt)) / 1000)));
}
const duration = step => {
  if (!step?.startedAt) return 'Waiting for prerequisites';
  const end = step.finishedAt ? new Date(step.finishedAt) : new Date();
  const seconds = Math.max(0, Math.round((end - new Date(step.startedAt)) / 1000));
  return `${visualStatus(step)} · ${formatElapsed(seconds)}`;
};

function createStepButton(name, className = 'step') {
  const step = latest(name);
  const status = visualStatus(step);
  const button = document.createElement('button');
  button.type = 'button';
  button.className = `${className} ${status}${selected === name ? ' selected' : ''}`;
  button.dataset.step = name;
  button.innerHTML = `<span class="row"><span>${esc(state.labels[name] || name)}</span><span class="icon">${icon(status)}</span></span><span class="detail">${esc(duration(step))}</span>`;
  button.addEventListener('click', () => { selected = name; followActive = false; render(); });
  return button;
}

function render() {
  const report = state.report || {};
  const active = [...state.steps].reverse().find(step => step.status === 'running');
  if (followActive && active) selected = active.name;

  document.getElementById('meta').textContent =
    `MES ${report.mesVersion || '—'} · run ${report.runId?.slice(0, 8) || '—'} · ${report.workspace || 'workspace setup pending'}`;
  const status = document.getElementById('status');
  status.className = `status${report.status === 'failed' ? ' failed' : ''}`;
  status.textContent = `${report.status === 'passed' ? '✓ Passed' : report.status === 'failed' ? '× Failed' : '◉ Running'} · total ${totalRunTime(report)}`;

  const allSteps = state.flow.flatMap(lane => lane.steps);
  const passedSteps = state.steps.filter(step => step.status === 'passed').length;
  const percent = Math.min(100, Math.round(passedSteps / allSteps.length * 100));
  const activeCommand = [...(report.commands || [])].reverse().find(command => command.status === 'running');
  const activeOutput = activeCommand && state.outputs[activeCommand.dashboardId];
  const lastOutput = activeOutput?.trim().split(/\r?\n/).at(-1);
  const latestNotice = state.notices.at(-1)?.message;
  document.getElementById('progress-label').textContent = active
    ? `${state.labels[active.name] || active.name} · ${duration(active)}`
    : report.status === 'passed' ? 'Workflow complete' : report.status === 'failed' ? 'Run stopped' : 'Preparing workflow…';
  document.getElementById('progress-count').textContent = `${passedSteps} / ${allSteps.length} steps`;
  const progressFill = document.getElementById('progress-fill');
  progressFill.style.width = `${report.status === 'passed' ? 100 : percent}%`;
  progressFill.classList.toggle('active', report.status === 'running');
  document.getElementById('progress-detail').textContent = activeCommand
    ? `${activeCommand.context} · ${activeCommand.command}`
    : lastOutput || latestNotice || 'Waiting for setup to start';

  const setup = state.flow[0];
  const setupDone = setup.steps.every(name => latest(name)?.status === 'passed');
  document.getElementById('setup-title').textContent = `Setup ${setupDone ? 'complete' : 'steps'}`;
  document.getElementById('setup-state').textContent = active && setup.steps.includes(active.name)
    ? `Working on ${state.labels[active.name] || active.name}` : setupDone ? 'Ready for workflow' : 'Waiting for preflight checks';
  const setupSteps = document.getElementById('setup-steps');
  setupSteps.replaceChildren(...setup.steps.map(name => createStepButton(name, 'setup-step')));

  const lanes = document.getElementById('lanes');
  lanes.replaceChildren(...state.flow.slice(1, 4).map(lane => {
    const section = document.createElement('section');
    section.className = 'lane';
    section.dataset.lane = lane.title;
    const title = document.createElement('h3');
    title.textContent = lane.title;
    const note = document.createElement('p');
    note.textContent = lane.title === 'Packages' ? 'Builds and archives · scroll for more' :
      lane.title === 'Installation' ? 'Local MES stack and libraries · scroll for more' : 'Serve and authenticate the UI';
    const list = document.createElement('div');
    list.className = 'lane-steps';
    list.setAttribute('aria-label', `${lane.title} steps`);
    list.replaceChildren(...lane.steps.map(name => createStepButton(name)));

    const activeName = lane.steps.find(name => latest(name)?.status === 'running') || null;
    const oldSection = [...lanes.children].find(item => item.dataset.lane === lane.title);
    const oldList = oldSection?.querySelector('.lane-steps');
    const oldView = laneViews.get(lane.title) || { activeName: null, scrollTop: 0, userScrolled: false };
    if (oldList) oldView.scrollTop = oldList.scrollTop;
    const activeChanged = activeName !== oldView.activeName;
    const view = { ...oldView, activeName, userScrolled: activeChanged ? false : oldView.userScrolled };
    const rememberManualScroll = () => { view.userScrolled = true; };
    for (const eventName of ['wheel', 'touchstart', 'pointerdown', 'keydown'])
      list.addEventListener(eventName, rememberManualScroll, { passive: true });
    section.append(title, note, list);
    if (view.userScrolled && !activeChanged) {
      list.scrollTop = view.scrollTop;
    } else if (activeName) {
      const activeRow = [...list.children].find(button => button.dataset.step === activeName);
      const activeIndex = lane.steps.indexOf(activeName);
      const previousRow = activeIndex > 0 ? list.children[activeIndex - 1] : null;
      const activeOffset = activeRow.offsetTop - list.firstElementChild.offsetTop;
      list.scrollTop = Math.max(0, activeOffset - (previousRow?.offsetHeight || 0) - 8);
    } else {
      list.scrollTop = activeChanged ? 0 : view.scrollTop;
    }
    view.scrollTop = list.scrollTop;
    laneViews.set(lane.title, view);
    return section;
  }));

  const chosen = selected && report.commands?.filter(command => command.step === selected).at(-1);
  const selectedStatus = latest(selected)?.status || 'waiting';
  const pendingPrerequisites = (stepPrerequisites()[selected] || []).filter(name => latest(name)?.status !== 'passed');
  document.getElementById('log-title').textContent = state.labels[selected] || selected || 'Selected step output';
  document.getElementById('log-context').textContent = chosen ? chosen.context : 'Waiting for command';
  const notices = state.notices.filter(notice => notice.step === selected).map(notice => notice.message).join('\n');
  document.getElementById('terminal').textContent = selected && selectedStatus === 'waiting'
    ? pendingPrerequisites.length
      ? `Waiting for prerequisites:\n${pendingPrerequisites.map(name => `• ${state.labels[name] || name} — ${visualStatus(latest(name))}`).join('\n')}`
      : 'All prerequisites are complete. Waiting for the workflow runner to start this step.'
    : chosen
    ? state.outputs[chosen.dashboardId] || 'Command started · waiting for output…'
    : notices || (latest(selected)?.status === 'running' ? 'Step started · waiting for its first command…' : 'No command output for this step yet.');
  document.getElementById('follow').textContent = followActive ? '● Following active step' : 'Follow active step';
  document.getElementById('follow').classList.toggle('selected', followActive);
  document.getElementById('follow').onclick = () => { followActive = !followActive; render(); };

  const uiProgress = report.uiProgress;
  document.getElementById('browser-state').textContent = uiProgress?.message || 'Waiting for UI check';
  document.getElementById('browser-location').textContent = uiProgress?.url || 'Local MES homepage';
  const screenshot = LIVE ? (uiProgress?.at || '') : SCREENSHOT;
  const screenshotUrl = LIVE && screenshot ? `/asset/browser.png?${encodeURIComponent(screenshot)}` : screenshot;
  const capture = document.getElementById('capture');
  if (screenshotUrl && screenshotUrl !== lastScreenshot) {
    lastScreenshot = screenshotUrl;
    capture.innerHTML = `<img src="${esc(screenshotUrl)}" alt="Latest browser capture" onerror="this.remove();this.parentElement.textContent='No browser capture available yet.'">`;
  } else if (!screenshotUrl) {
    let preview = capture.querySelector('.activity-preview');
    if (!preview) {
      preview = document.createElement('div');
      preview.className = 'activity-preview';
      preview.innerHTML = '<span class="activity-ring"></span><strong class="activity-title"></strong><span class="activity-time"></span><span class="activity-output"></span>';
      capture.replaceChildren(preview);
    }
    preview.querySelector('.activity-title').textContent = active
      ? state.labels[active.name] || active.name : 'Waiting for workflow activity';
    preview.querySelector('.activity-time').textContent = active ? `${duration(active)} · live` : 'The active step will appear here';
    const previewOutput = preview.querySelector('.activity-output');
    if (lastOutput || latestNotice) {
      previewOutput.textContent = (lastOutput || latestNotice).slice(-150);
      previewOutput.hidden = false;
    } else {
      previewOutput.hidden = true;
    }
  }
  document.getElementById('events').replaceChildren(...state.notices.filter(notice => notice.step === 'ui-login').slice(-5).map(event => {
    const row = document.createElement('div');
    row.className = 'event';
    const message = document.createElement('span');
    message.textContent = event.message;
    const time = document.createElement('time');
    time.textContent = new Date(event.at).toLocaleTimeString();
    row.innerHTML = '<i>✓</i>';
    row.append(message, time);
    return row;
  }));
  const teardown = latest('stack-down');
  document.getElementById('finish').textContent = teardown
    ? `${icon(teardown.status)} Final teardown · ${teardown.status}` : 'Final teardown waits for all branches.';
  document.getElementById('updated').textContent = `Updated ${new Date(state.updatedAt).toLocaleTimeString()}`;
}

function tick() {
  const active = [...state.steps].reverse().find(step => step.status === 'running');
  const report = state.report || {};
  const runStatus = document.getElementById('status');
  runStatus.textContent = `${report.status === 'passed' ? '✓ Passed' : report.status === 'failed' ? '× Failed' : '◉ Running'} · total ${totalRunTime(report)}`;
  for (const button of document.querySelectorAll('[data-step]')) {
    const step = latest(button.dataset.step);
    const detail = button.querySelector('.detail');
    if (detail) detail.textContent = duration(step);
  }
  document.getElementById('progress-label').textContent = active
    ? `${state.labels[active.name] || active.name} · ${duration(active)}`
    : state.report.status === 'passed' ? 'Workflow complete' : state.report.status === 'failed' ? 'Run stopped' : 'Preparing workflow…';
  const previewTime = document.querySelector('.activity-time');
  if (previewTime) previewTime.textContent = active ? `${duration(active)} · live` : 'The active step will appear here';
}

render();
if (LIVE) {
  const events = new EventSource('/events');
  events.onmessage = event => { state = JSON.parse(event.data); render(); };
  events.onerror = () => { document.getElementById('updated').textContent = 'Connection lost · retrying…'; };
  setInterval(tick, 1000);
}
