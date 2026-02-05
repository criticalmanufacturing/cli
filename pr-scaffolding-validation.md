# PR Scaffolding Validation Against Latest MES

## Context

The `@criticalmanufacturing/cli` repository provides the CLI used to scaffold and build packages for **Critical Manufacturing MES** projects.

Relevant documentation:

- CLI scaffolding documentation  
  https://cli-docs-preview.vercel.app/scaffolding/

- Critical Manufacturing MES documentation  
  https://help.criticalmanufacturing.com/

Changes to the CLI must not break the **developer workflow used when building MES extensions**.

To guarantee this, we need an automated validation workflow that runs the full scaffolding lifecycle using the CLI version built from the Pull Request.

---

# Objective

Implement a **GitHub Actions workflow** that validates the CLI against the **latest available Critical Manufacturing MES version**.

The workflow must:

1. Build the CLI from the PR source
2. Install the CLI via npm
3. Start or connect to a **latest MES environment**
4. Run a full CLI workflow:
   - `cmf init`
   - `cmf new`
   - `cmf build`
   - `cmf pack`

If any step fails, the workflow must fail.

---

# File to Create

Create the workflow file:

.github/workflows/pr-scaffolding-validation.yml

---

# Workflow Trigger

The workflow must run on pull requests targeting `main`.

```yaml
on:
  pull_request:
    branches:
      - main


⸻

Environment

Runner:

ubuntu-latest

Node version:

20.x


⸻

Implementation Steps

1. Checkout repository

Use the GitHub checkout action.

The workflow must use the PR source code.

⸻

2. Setup Node

Install Node 20.

Install dependencies and build the CLI.

Example:

npm ci
npm run build


⸻

3. Package CLI from PR

Create a tarball of the CLI.

npm pack

Install the generated package globally:

npm install -g ./<generated-package>.tgz

Verify installation:

cmf --version


⸻

4. Start Latest MES Environment

The workflow must use the latest available Critical Manufacturing MES version referenced in:

https://help.criticalmanufacturing.com/

Possible implementations:

Preferred

Start a containerized MES environment if an image is available.

Example:

docker run -d -p 5000:5000 criticalmanufacturing/mes:latest

Alternative

Download and install the latest MES version referenced in the documentation.

⸻

5. Create Test Workspace

Create a temporary project workspace.

mkdir workspace
cd workspace


⸻

6. Run CLI Scaffolding

Run:

cmf init

This command should scaffold a full MES extension project.

If the CLI supports non-interactive mode, prefer it.

⸻

7. Generate Packages

Create packages using the CLI generator.

At minimum generate:
	•	business
	•	data
	•	database
	•	html
	•	feature
	•	test

Example:

cmf new business
cmf new data
cmf new database
cmf new html
cmf new feature
cmf new test

The goal is to validate that all generators still work.

⸻

8. Build Packages

Run:

cmf build

This validates that generated packages compile successfully.

⸻

9. Pack Artifacts

Run:

cmf pack

This should generate deployable artifacts.

⸻

Logging Requirements

Each major stage should appear clearly in logs.

Examples:

Installing CLI from PR
Starting MES environment
Scaffolding project
Generating packages
Building packages
Packing artifacts


⸻

Expected Behaviour

The workflow should succeed if:
	•	CLI installs successfully
	•	MES environment starts
	•	cmf init scaffolds a project
	•	cmf new generates packages
	•	cmf build succeeds
	•	cmf pack produces artifacts

The workflow must fail if any step fails.

⸻

Optional Improvements

These enhancements are recommended but not required:
	•	Upload generated .cmfpackage artifacts
	•	Enable npm dependency caching
	•	Run validation on multiple Node versions
	•	Run cmf ls to inspect generated package tree
	•	Test against multiple MES versions

⸻

Acceptance Criteria
	•	Workflow runs automatically on PRs
	•	CLI is installed from PR build
	•	Latest MES version is used
	•	Project scaffolding works
	•	Package generation works
	•	Builds succeed
	•	Packaging succeeds
	•	Workflow fails on any CLI error
