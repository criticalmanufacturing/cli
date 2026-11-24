---
name: cli-prepare-release
description: Prepare Critical Manufacturing CLI stable releases and development prereleases, open bump PRs, and publish GitHub Releases with workflow monitoring after explicit confirmation. Never infer merge or publication approval.
---

# Prepare and publish a CLI release

## Workspace and authorization

- Use an isolated worktree of `criticalmanufacturing/cli`, not the supplied checkout, for release changes. Inspect and preserve existing work. By default, fetch current main/development and tags, record their SHAs, and create release branches from the latest fetched source-branch commit. Tell the user that you will fetch and use a separate worktree, then proceed without an extra permission question. Reuse an appropriate existing worktree or matching branch/PR instead of duplicating it; refresh stale preparation against current source history. Honor an explicitly requested base/ref or workspace constraint.
- Read AGENTS.md, PUBLISHING.MD, package.json, .versionrc.json, the PR template, and applicable repository instructions. Run SDK/tool-dependent commands in the CLI devcontainer.
- Skill edits do not authorize release execution. Push/open PRs only within the user's authorized scope; local-only/no-push requests stop at reviewable local changes and proposed PR text, without invented URLs.
- Never merge a PR without an explicit command to merge that PR. Preparing releases, approving plans, saying "next", or agreeing to wait does not authorize merging. Never enable auto-merge or a merge queue, alter publishing workflows/settings, or directly update main/development to simulate a PR merge.
- Release-procedure PRs into **main and development must use a merge commit**, preserving history: **no squash and no Rebase and merge**. This supersedes the earlier fast-forward/rebase preference. The exception is a prerelease bump PR into development, which may be squash-merged; its source must still contain exactly one bump commit. State the intended method in each PR and verify repository capabilities when explicitly asked to merge; if unavailable, report the limitation rather than substituting another method.

## Choose the flow and versions

| Request | Source/version baseline | Destination and publication |
| --- | --- | --- |
| Standalone prerelease | Latest development | Bump PR into development; npm `next` |
| Stable release | Branch from latest development; calculate stable target from latest main | Release PR into main; npm `latest`, followed by synchronization and next prerelease PRs |

Create a bump PR by default. Skip it only when the user explicitly requests an already bumped version: verify matching versions in npm/package.json, cmf-cli/cmf.csproj, and core/core.csproj on the target branch. An existing prerelease suffix alone does not mean the requested bump is complete. Resume an existing bump PR; publication waits for its merge. Report existing published versions and their actual publishing outcome instead of duplicating publication.

**Standalone prerelease:** Calculate from fetched development, not main. Stable `6.0.1` means no prerelease bump since convergence with main: increment the patch to `6.0.2-0`. Numeric prereleases retain the numeric version and increment the suffix: `6.0.1-0` -> `6.0.1-1`, `6.0.1-4` -> `6.0.1-5`.

Use an explicitly supplied version as the requested target. If it differs from the default or conflicts with the rules (invalid/nonnumeric suffix, stable development target, downgrade, already published version), explain before making changes and ask whether the user intends a one-time override and/or wants the skill changed. Never silently substitute a version, rewrite this skill, overwrite a released tag, or republish a package. Execution still requires a valid, unpublished target appropriate to the channel.

**Stable:** Calculate from fetched main, never development; default main `6.0.0` -> stable `6.0.1`, without a prerelease suffix. Honor an explicit bump/version newer than main; clarify ambiguous overrides. Development may already have any prerelease of that target, but this is not a prerequisite and does not change the intended stable target. Default the subsequent development version to the next patch prerelease: stable `6.0.1` -> development `6.0.2-0`. Show both targets before changes without asking to confirm an unambiguous default. Check cross-file versions and existing tags on relevant branches.

Historical examples (inspect current conventions; do not copy obsolete versions or validation claims): stable [#625](https://github.com/criticalmanufacturing/cli/pull/625), branch `main-release-5.7.1` from development into main; synchronization [#646](https://github.com/criticalmanufacturing/cli/pull/646), main into development with body `chore: release 5.8.0`; prerelease [#867](https://github.com/criticalmanufacturing/cli/pull/867), `bump/6.0.1-0` into development with title `chore: bump to 6.0.1-0`. [#765](https://github.com/criticalmanufacturing/cli/pull/765) is a main-to-development example about scaffolding, not release synchronization. Use Conventional Commit titles rather than historical `Release X.Y.Z` titles.

## Prepare PRs

Inspect live refs and matching PRs before writes. Never overwrite concurrent work. Use an exact force-with-lease only for an authorized rewrite of a preparation branch; never force-push main or development.

For every bump:

- Use the existing root `npm run bump:*` scripts; inspect their semantics against the starting version. Removing a prerelease suffix does not always require increasing the numeric version. Default standalone numeric transitions use `bump:pre`; other available prerelease scripts include `bump:pre:feature` and `bump:pre:breaking`. Do not assume `bump:pre:patch` exists. If existing scripts cannot consistently produce a requested target, resolve the tooling gap with the user instead of hand-editing versions/changelog.
- Include **all generated bump changes in exactly one Conventional Commit**: npm/package.json, npm/package-lock.json, both .NET versions, CHANGES.md, and any other intended generated metadata. Inspect the full diff, preserve stable changelog entries, and verify matching versions/lock metadata. When refreshing a PR, consolidate the final bump diff on current source history; leave no extra synchronization/cleanup commits in a bump PR.
- Validate commit/PR titles using .commitlintrc.json and the installed commitlint CLI in the devcontainer. Merge/squash messages can inherit those titles and fail later checks. Report the actual validation and intended merge method.

**Standalone prerelease:** Create/resume `bump/X.Y.Z-N` from latest development, generate/validate the bump, and push/open the authorized PR to development titled `chore: bump to X.Y.Z-N`. Report it and ask whether to wait for merge as described below. Do not start the stable sequence.

**Stable sequence:** Create PRs one at a time, informing the user and stopping after each creation. An initial request opens only PR 1. In an established sequence, "next"/"next PR" means exactly the next PR, never merging or automatically advancing further. Determine progress from conversation and live GitHub state, not checkout identity. Verify each preceding PR is merged before proceeding; otherwise report the dependency and stop.

1. **Stable into main:** Branch `main-release-X.Y.Z` from latest development and generate the stable version calculated from main. Open into main titled `chore: release X.Y.Z`, requiring a merge commit. Review all development changes included in the release, not only the bump.
2. **Main back into development:** After PR 1 merges and the user says "next", refresh refs and open main -> development, requiring a merge commit. Never rebase or force-push main. If there is no meaningful diff, report synchronization as satisfied or explain the missing changes; do not manufacture empty commits/URLs. Ask how to proceed if skipping changes the sequence.
3. **Next development prerelease:** After PR 2 merges and the user says "next", create `bump/X.Y.Z-N` from refreshed synchronized development and generate the next prerelease, retaining stable release notes. Open into development titled `chore: bump to X.Y.Z-N`; the prerelease-bump exception permits squash. Report completion of the three-PR sequence.

For stable handoffs, list all three PRs in dependency order with actual URLs or "not created", source -> destination, version changes, merge method, and readiness/dependencies. Retain entries blocked by missing diffs or local-only restrictions. Explain "next" and report actual merge/publication state; do not claim unperformed actions.

## Validation scope

Run only the corresponding root bump command, package-age checks, and title lint in the devcontainer unless the user separately requests more. Verify npm's minimum-release-age setting (currently 3 days), use an existing npm age-check command if available, and run `npm run check:nuget:min-age`. Configuration verification is not a full locked-dependency age audit. Inspect generated files and run `git diff --check` (use `core.whitespace=cr-at-eol` for existing CRLF files). Report exact results and skips.

Do not run builds, dotnet/npm tests, integration suites, or act by default. Use `HUSKY=0` for commits when pre-commit hooks would otherwise run excluded tests; do not change shared hooks or CI configuration.

## Wait for merge

After creating/reopening a bump/release PR, ask: "Should I wait for this PR to be merged, then prepare the GitHub release/prerelease for your final confirmation?"

- **Yes:** Poll that exact PR every **30 seconds**, provide progress updates, and stop on user request. Require merged state/merge timestamp; closed alone is insufficient. If closed unmerged or queries fail, report it rather than assuming success. Never merge without separate explicit authorization.
- **No:** Stop polling and say: "Tell me when you want to prepare the GitHub release/prerelease." Do not poll in the background.
- **Merged:** Prepare the preview below. Agreement to wait authorizes only polling/preparation, never tagging or release creation.

## Preview, confirm, publish, and monitor

**Always show the concrete preview and obtain final explicit confirmation before creating any GitHub release/prerelease, including drafts.** General publishing requests, PR approval, and earlier intent do not replace this confirmation. Any changed SHA, notes, or other preview field requires a new preview and confirmation. Skill edits authorize no publication.

1. Verify the bump/release PR merged; refresh the target branch, record its head SHA, and verify matching npm/.NET versions. Development requires a prerelease and npm `next`; main requires stable and npm `latest`. Inspect `.github/workflows/npm-publish.yml` at that SHA for `release: published`. Its display name is currently `cmf-cli`; identify it by workflow path. The workflow chooses npm channel from `release.prerelease`, not branch alone.
2. Inspect recent releases of the same kind and ancestry/comparison ranges; choose an appropriate previous published tag rather than trusting list order. Examples: [6.0.1-0](https://github.com/criticalmanufacturing/cli/releases/tag/6.0.1-0) from development and [6.0.1](https://github.com/criticalmanufacturing/cli/releases/tag/6.0.1) from main. Title and tag are the exact version, without `v`.
3. Generate notes with `POST /repos/criticalmanufacturing/cli/releases/generate-notes`: `tag_name` = version, `target_commitish` = recorded head SHA, `previous_tag_name` = selected base. This is the API equivalent of **Generate Release Notes**, not publication. Review and retain the generated `## What's Changed`, PR/author entries, optional `## New Contributors`, and `**Full Changelog**` comparison link. Do not replace it with CHANGES.md or handwritten notes. See [GitHub Releases REST API](https://docs.github.com/en/rest/releases/releases#generate-release-notes-content-for-a-release).
4. Show exactly how it will appear: repository, title/tag, branch and full SHA, **Pre-release**/**Latest** badge, npm `next`/`latest`, and the complete generated Markdown description. Explain that publication triggers npm-publish. Ask explicitly whether to create this exact release/prerelease and **wait for confirmation**. Do not create a tag/draft for the preview.
5. After confirmation, recheck branch head, tag, and release state. If head advanced, revalidate/regenerate and return to preview/confirmation. Existing tags must resolve to the intended head; stop on mismatch without moving them. Reuse a matching existing release and inspect its run rather than duplicating it. Create via `POST /repos/criticalmanufacturing/cli/releases` with exact version as `tag_name`/`name`, verified SHA as `target_commitish`, reviewed notes as `body`, and `draft: false`. Development: `prerelease: true`, `make_latest: "false"`; main: `prerelease: false`, `make_latest: "true"`. A missing tag is created at that SHA; never default to the repository's default branch. Preserve JSON types and Markdown newlines using structured input, e.g. `gh api --input` with a file.
6. Verify returned title/tag/flags and resolved tag SHA. Find the newly triggered npm-publish.yml run by release event, exact tag/SHA, and publication time. Do not substitute an older/different run. If absent, inspect trigger/authentication restrictions; do not dispatch another workflow or claim success. Actions' GITHUB_TOKEN generally cannot trigger another workflow by creating a release; use an authorized user/app credential capable of generating the event, without exposing credentials.
7. Wait through completion, checking in short intervals with progress updates. Success requires the **entire workflow** to succeed, including NuGet. Report release/run URLs, version, branch, npm channel, and conclusion. On failure inspect failed job/step logs, explain the cause and any partial publication; npm can succeed while NuGet fails. Report cancellation, timeout, or access limits accurately. A GitHub Release alone is not proof of package publication. Never automatically rerun publishing, delete tags/releases/assets, or republish: duplicate packages/assets require a scoped recovery decision from the user.
