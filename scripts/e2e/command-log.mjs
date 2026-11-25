const sensitiveOption = /^(?:--(?:token|pat|password|secret|api-key|access-token|refresh-token|nugetRegistryPassword)|-p|-t)$/i;

function redactArgument(value, secrets) {
  let text = String(value);
  for (const secret of secrets.filter(Boolean)) text = text.split(secret).join('[REDACTED]');
  return text.replace(/(https?:\/\/[^\s/@:]+:)[^\s/@]+@/g, '$1[REDACTED]@');
}

export function redactArguments(argv, secrets = []) {
  let hideNext = false;
  return argv.map(value => {
    const text = String(value);
    if (hideNext) { hideNext = false; return '[REDACTED]'; }
    if (sensitiveOption.test(text)) { hideNext = true; return text; }
    const equals = text.indexOf('=');
    if (equals > 0 && sensitiveOption.test(text.slice(0, equals)))
      return `${text.slice(0, equals)}=[REDACTED]`;
    // Remote commands are transported as a JSON argv array.
    if (text.startsWith('[')) {
      try {
        const nested = JSON.parse(text);
        if (Array.isArray(nested)) return JSON.stringify(redactArguments(nested, secrets));
      } catch { /* Ordinary argument, not a JSON command. */ }
    }
    return redactArgument(text, secrets);
  });
}

export function formatCommand(command, argv, secrets = []) {
  const quote = value => /^[a-zA-Z0-9_./:@=,+-]+$/.test(value) ? value : `'${value.replaceAll("'", "'\\''")}'`;
  return [redactArgument(command, secrets), ...redactArguments(argv, secrets)].map(quote).join(' ');
}
