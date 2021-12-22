# @criticalmanufacturing/cli documentation

## How to contribute
1. Clone this git repository to your machine. Make sure you checkout the `documentation` branch:
    ```sh
    git clone --branch documentation https://github.com/criticalmanufacturing/cli.git
    ```
1.1. If you don't have push access, please create a fork and proceed using the new fork.

1. Run the command help pages generator (replace the path with your own):
    ```sh
    node gen-cmds.js /src/cmf-cli/cmf-cli/bin/Debug/cmf.dll
    ```

1. Now you can render the documentation with mdBook or MkDocs.
1.1. for mdBook
1.1.1. hosting the documentation in a container:
    ```sh
    npm run img:build && npm run publish:build && npm run publish:run
    ```
1.1.1. getting the generated website
    ```sh
    npm run img:build && npm run export
    ```
    Documentation will be at /book
1.1. for MkDocs
1.1.1. hosting the documentation in a container:
    ```sh
    npm run build:mkdocs && npm run serve:mkdocs
    ```
1.1.2. to get the generated website, run the previous command and then
    ```sh
    npm run dist:mkdocs
    ```
    Documentation will be at /dist

## Publishing to GitHub Pages
We raw publish to GitHub Pages. The official repository uses the `gh_pages` branch. Simply push the content of the output folder to this branch. Example:
```sh
cd dist
git init
git remote add origin https://github.com/criticalmanufacturing/cli.git
git add .
git commit -m"publish"
git push origin master:gh_pages -f
```