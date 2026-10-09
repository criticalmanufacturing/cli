# @criticalmanufacturing/cli documentation

The documentation uses [MkDocs](https://www.mkdocs.org/) with the Material theme.
Configuration is in `mkdocs.yml`, Markdown and assets are in `src/`, and the
static website is generated in `site/`. Run the commands below from `docs/`.

## Local development

Use Python 3.13 and a virtual environment:

```sh
python -m venv .venv
. .venv/bin/activate
python -m pip install -r requirements.txt
mkdocs serve
```

On Windows, activate the environment with `.venv\Scripts\Activate.ps1` in
PowerShell. Open <http://localhost:8000> to preview the site. Changes to sources
and configuration reload automatically.

Build the static website with the same validation used in CI:

```sh
mkdocs build --strict
```

Dependencies are pinned in `requirements.txt`. Navigation uses the existing
`.pages` files, and Mermaid diagrams and section indexes use Material's built-in support.
The website build no longer loads the disabled PDF-export plugin or its native
system dependencies.

## Docker

Docker with Buildx can build, preview, or export the site without installing
Python or Node.js on the host.

For a live preview with automatic reload:

```sh
docker build --target mkdocs -t cli/docs-base .
docker run --rm -p 8081:8000 -v "$(pwd):/docs" cli/docs-base
```

Open <http://localhost:8081>. For a static preview served by Nginx:

```sh
docker build --target web -t cli/docs .
docker run --rm -p 8081:80 cli/docs
```

Export the static website directly to `site/`:

```sh
docker buildx build --target export --output type=local,dest=site .
```

The optional npm shortcuts are `build:mkdocs:base` and `serve:mkdocs` for the live
preview, `build:mkdocs` and `run:mkdocs` for Nginx, and `dist:mkdocs` for export.

## Generate command reference pages

Build the CLI in the devcontainer, then run the generator from `docs/`:

```sh
dotnet build ../cmf-cli/cmf.csproj
npm run help:init
node gen-cmds.js ../cmf-cli/bin/Debug/cmf.dll
```

The generator requires the `help2md` repository and updates the `<!-- BEGIN USAGE -->`
blocks. Review its output before committing; do not edit these blocks by hand.

## GitHub Pages

The `GitHub Pages` workflow builds and validates documentation for pull requests
that change `docs/` or the workflow itself. The generated site is available as the
`documentation` workflow artifact, including `.nojekyll`.

Pushes to `main` build the same Docker export and publish it to the `gh-pages`
branch with `peaceiris/actions-gh-pages`. Pull requests only build and upload the
site; publication runs in a separate job with write permission.
