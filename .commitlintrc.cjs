module.exports = {
  extends: ["@commitlint/config-conventional"],
  // Dependabot PRs skip commitlint; release PRs include those generated commits again.
  ignores: [message => /^chore\(deps\): Bump .+\n/.test(message)
    && /^Signed-off-by: dependabot\[bot\] <support@github\.com>$/m.test(message)],
  rules: {
    "header-max-length": [2, "always", 200],
    "body-max-line-length": [2, "always", 200]
  }
};
