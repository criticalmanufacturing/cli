// Root scaffolds share a manifest, so keep package commands sequential while
// overlapping them with the independent local stack and browser work.
export async function runFlow({ types, scaffold, build, pack, install, generateLbos,
  serve, login, stopServe, cancel }) {
  const gate = () => {
    let resolve, reject;
    const promise = new Promise((yes, no) => { resolve = yes; reject = no; });
    promise.catch(() => {}); // A failure can reject a gate before it is awaited.
    return { promise, resolve, reject };
  };
  const scaffolded = gate(), htmlBuilt = gate(), installed = gate(), lbos = gate();
  let failure, cancellation;
  const guard = async action => {
    try { await action(); }
    catch (error) {
      if (!failure) {
        failure = error;
        for (const readiness of [scaffolded, htmlBuilt, installed, lbos]) readiness.reject(error);
        cancellation = Promise.resolve().then(cancel);
        cancellation.catch(() => {});
      }
      throw error;
    }
  };
  await Promise.allSettled([
    guard(async () => {
      for (const type of types) await scaffold(type);
      scaffolded.resolve();
      // Pack after each build; only the test package requires generated LBOs.
      for (const type of types) {
        if (type === 'test') await lbos.promise;
        await build(type);
        if (type === 'html') htmlBuilt.resolve();
        await pack(type);
      }
    }),
    guard(async () => {
      await install();
      installed.resolve();
      await scaffolded.promise;
      await generateLbos();
      lbos.resolve();
    }),
    guard(async () => {
      await Promise.all([htmlBuilt.promise, installed.promise]);
      await serve();
      await login();
      await stopServe();
    }),
  ]);
  if (cancellation) await cancellation;
  if (failure) throw failure;
}
