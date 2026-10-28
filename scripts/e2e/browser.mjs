import path from 'node:path';
import { writeFile } from 'node:fs/promises';

// Browser authentication is independent of containers and package commands.
export async function verifyHomepage({ config, state, report, ci, uiTimeout, setBrowser = () => {}, onProgress = () => {} }) {
  const { chromium } = await import('playwright');
  const browser = await chromium.launch({ headless: true, channel: 'chromium', handleSIGINT: false, handleSIGTERM: false });
  setBrowser(browser);
  let page;
  let progressTimer;
  let capturingProgress = false;
  try {
    page = await browser.newPage();
    page.setDefaultTimeout(uiTimeout);
    const captureProgress = async () => {
      if (capturingProgress || page.isClosed()) return;
      capturingProgress = true;
      try {
        const location = new URL(page.url());
        const url = `${location.origin}${location.pathname}`;
        console.log(`UI: waiting at ${url}`);
        await writeFile(path.join(state, 'ui-progress.json'), JSON.stringify({
          url, title: await page.title(), capturedAt: new Date().toISOString(),
        }, null, 2), { mode: 0o600 });
        await page.screenshot({ path: path.join(state, 'ui-progress.png'), fullPage: true,
          mask: [page.locator('input[type="password"]')], maskColor: '#007ac9' });
        onProgress('Browser capture updated', { url, title: await page.title() });
      } catch { /* A document may be navigating while diagnostics are captured. */ }
      finally { capturingProgress = false; }
    };
    progressTimer = setInterval(() => { void captureProgress(); }, 5000);
    page.on('pageerror', error => console.log(`Browser error: ${error.message}`));
    const response = await page.goto(config.uiUrl || 'http://localhost:80', { timeout: uiTimeout });
    if (!response?.ok()) throw new Error(`UI returned HTTP ${response?.status()}`);
    await captureProgress();
    const login = config.login || {};
    // SecurityPortal submits full-page forms. Inspect the new document, not
    // the login form that remains visible while its request is in flight.
    const submitForm = async selector => {
      await Promise.all([
        page.waitForEvent('framenavigated', { predicate: frame => frame === page.mainFrame() }),
        page.locator(selector).click(),
      ]);
      await page.waitForLoadState('domcontentloaded');
    };
    console.log('UI: login page opened');
    onProgress('Login page opened');
    await page.locator(login.usernameSelector || "input[name='username']").fill(process.env.CMF_E2E_UI_USER || 'admin');
    await page.locator(login.passwordSelector || "input[name='password']").fill(process.env.CMF_E2E_UI_PASSWORD || 'admin');
    await submitForm(login.submitSelector || "button:has-text('Login')");
    console.log('UI: credentials submitted');
    onProgress('Credentials submitted');
    const sidebar = page.locator(login.sidebarSelector || '.menu-sub-menu').filter({ visible: true });
    const username = page.locator(login.usernameSelector || "input[name='username']").filter({ visible: true });
    // Fresh local MES backups can require an initial password confirmation.
    const confirmation = page.locator(login.resetConfirmationSelector || "input[name='password_confirmation']")
      .filter({ visible: true });
    await sidebar.or(confirmation).or(username).first().waitFor({ state: 'visible' });
    let resetCompleted = false;
    if (await confirmation.isVisible()) {
      await page.locator(login.resetPasswordSelector || "input[name='password']")
        .fill(process.env.CMF_E2E_UI_PASSWORD || 'admin');
      await confirmation.fill(process.env.CMF_E2E_UI_PASSWORD || 'admin');
      await submitForm(login.resetSubmitSelector || "button:has-text('Reset')");
      console.log('UI: initial password confirmation completed');
      onProgress('Initial password confirmation completed');
      resetCompleted = true;
      await captureProgress();
    }
    try {
      await sidebar.or(username).first().waitFor({ state: 'visible',
        timeout: resetCompleted ? Math.min(30000, uiTimeout) : uiTimeout });
    } catch (error) {
      const location = new URL(page.url());
      const entry = new URL(config.uiUrl || 'http://localhost:80');
      const blankResetPage = resetCompleted && error.name === 'TimeoutError' &&
        location.origin === entry.origin && location.pathname === '/' &&
        !(await page.locator('body').innerText()).trim();
      if (!blankResetPage) throw error;
      if (ci) throw new Error('UI remained blank after initial password confirmation; recovery is disabled in CI');
      console.log('UI: blank page after password confirmation; reopening the configured UI entry point once');
      report.uiRecovery = { reason: 'Blank page after initial password confirmation',
        action: 'Reopened the configured UI entry point once', automated: true };
      const retry = await page.goto(entry.href, { timeout: uiTimeout });
      if (!retry?.ok()) throw new Error(`UI retry returned HTTP ${retry?.status()}`);
      await sidebar.or(username).first().waitFor({ state: 'visible' });
    }
    if (await username.isVisible()) {
      const scopeError = "The user doesn't have all the required scopes.";
      if (await page.getByText(scopeError, { exact: true }).isVisible())
        throw new Error(`MES rejected login: ${scopeError} Check the local stack's OAuth scope roles.`);
      await username.fill(process.env.CMF_E2E_UI_USER || 'admin');
      await page.locator(login.passwordSelector || "input[name='password']")
        .fill(process.env.CMF_E2E_UI_PASSWORD || 'admin');
      await submitForm(login.submitSelector || "button:has-text('Login')");
      console.log('UI: additional sign-in submitted');
      onProgress('Additional sign-in submitted');
      await sidebar.or(username).first().waitFor({ state: 'visible' });
      if (await username.isVisible()) throw new Error('Login returned to the login form after the additional sign-in');
    }
    for (const text of ['Administration', 'Business Data']) await sidebar.getByText(text, { exact: true }).first()
      .waitFor({ state: 'visible' });
    console.log('UI: homepage sidebar verified');
    onProgress('Homepage sidebar verified');
    await page.screenshot({ path: path.join(state, 'homepage.png'), fullPage: true });
  } catch (error) {
    if (page && !page.isClosed()) {
      const location = new URL(page.url());
      console.error(`UI: verification failed at ${location.origin}${location.pathname}`);
    }
    let location = null;
    if (page && !page.isClosed()) {
      location = new URL(page.url());
      const screenshot = await page.screenshot({ fullPage: true,
        mask: [page.locator('input[type="password"]')], maskColor: '#007ac9' }).catch(() => null);
      if (screenshot) await Promise.all([
        writeFile(path.join(state, 'ui-failure.png'), screenshot),
        writeFile(path.join(state, 'ui-progress.png'), screenshot),
      ]);
    }
    onProgress('Browser verification failed', { url: location ? `${location.origin}${location.pathname}` : '' });
    throw error;
  } finally { clearInterval(progressTimer); await browser.close(); setBrowser(null); }
}

export async function checkBrowser(setBrowser = () => {}) {
  const { chromium } = await import('playwright');
  let browser;
  try { browser = await chromium.launch({ headless: true, channel: 'chromium', handleSIGINT: false, handleSIGTERM: false }); }
  catch (error) {
    const library = error.message.match(/error while loading shared libraries: ([^:\s]+)/)?.[1];
    throw new Error(`${library ? `Chromium is missing the Linux library ${library}.` : error.message}\nInstall browser dependencies inside the runner's devcontainer:\nnpm --prefix scripts/e2e run browser:install`, { cause: error });
  }
  setBrowser(browser);
  try { await browser.close(); } finally { setBrowser(null); }
}
