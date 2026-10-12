// Root scaffolds share a manifest, so keep package commands sequential while
// overlapping them with the independent local stack and browser work.
import { createSteps } from './steps.mjs';

export async function executeWorkflow(runtime) {
  const { session, step, reporter } = runtime;
  const actions = createSteps(runtime);
  try {
    await step('preflight', actions.preflight);
    await reporter.createWorkspace();
    await step('login', actions.login);
    await step('init', actions.init);
    await step('devcontainer', commands => commands.startContainer());
    await step('container-login-sync', commands => commands.cli(['login', 'sync'], session.containerWorkspace, 'container-login-sync'));

    await runFlow({
      types: ['business', 'html', 'help', 'data', 'test', 'iot'],
      cancel: runtime.cancel,
      scaffold: async type => {
        await step(`scaffold-${type}`, commands => actions.scaffold(commands, type));
        await actions.resolvePackage(type); // Validate retained scaffolds on resume, too.
      },
      install: async () => {
        await step('stack-init', actions.stackInit);
        await step('stack-up', actions.stackUp);
        await step('installation', actions.installation);
      },
      generateLbos: () => step('generate-lbos', actions.generateLbos),
      build: type => step(`build-${type}`, commands => actions.build(commands, type)),
      pack: type => step(`pack-${type}`, commands => actions.pack(commands, type)),
      serve: () => step('serve', actions.serve),
      login: () => step('ui-login', actions.uiLogin),
      stopServe: () => step('stop-serve', actions.stopServe),
    });

    await step('stack-down', actions.stackDown);
    session.report.status = 'passed';
  } catch (error) {
    session.report.status = 'failed'; session.report.error = error.message;
    console.error(error.message);
    await runtime.cancel();
  } finally { await runtime.finish(); }
  return session.report;
}

export async function runFlow({ types, scaffold, build, pack, install, generateLbos,
  serve, login, stopServe, cancel }) {
  if (!types.includes('business')) throw new Error('The local stack requires the business package');
  const gate = () => {
    let resolve, reject;
    const promise = new Promise((yes, no) => { resolve = yes; reject = no; });
    promise.catch(() => {}); // A failure can reject a gate before it is awaited.
    return { promise, resolve, reject };
  };
  const businessBuilt = gate(), scaffolded = gate(), htmlBuilt = gate(), lbos = gate();
  let failure, cancellation;
  const guard = async action => {
    try { await action(); }
    catch (error) {
      if (!failure) {
        failure = error;
        for (const readiness of [businessBuilt, scaffolded, htmlBuilt, lbos]) readiness.reject(error);
        cancellation = Promise.resolve().then(cancel);
        cancellation.catch(() => {});
      }
      throw error;
    }
  };
  await Promise.allSettled([
    guard(async () => {
      for (const type of types) {
        await scaffold(type);
        if (type === 'business') {
          await build(type);
          businessBuilt.resolve();
          await pack(type);
        }
      }
      scaffolded.resolve();
      // Pack after each build; only the test package requires generated LBOs.
      for (const type of types) {
        if (type === 'business') continue; // Already built before stack startup.
        if (type === 'test') await lbos.promise;
        await build(type);
        if (type === 'html') htmlBuilt.resolve();
        await pack(type);
      }
    }),
    guard(async () => {
      await businessBuilt.promise;
      await install();
      await scaffolded.promise;
      await generateLbos();
      lbos.resolve();
    }),
    guard(async () => {
      await htmlBuilt.promise;
      await serve();
      await login();
      await stopServe();
    }),
  ]);
  if (cancellation) await cancellation;
  if (failure) throw failure;
}
