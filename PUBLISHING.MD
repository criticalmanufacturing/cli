# Publish to NPM from GitHub
Publishing from GitHub is done using releases. As a guideline creating:

- a release publishes a new version of the cli to the **@latest** tag
    - these releases should only be created from branch _main_

- a pre-release publishes a new version of the cli to the **@next** tag
    - these releases may be created from branch _development_


Go to [Releases](https://github.com/criticalmanufacturing/cli/releases) and click "Draft a new release"


# Publish to NPM locally
At root, run:

- If needed
  
    > npm i
    
- clean dist folder
  
    > npm run build:clean 
    
- bump version. Run one of these:

    > npm run bump:pre # bump pre-release version
    > npm run bump:patch # bug fixing release
    > npm run bump:feature # feature release
    > npm run bump:breaking # release with breaking changes

- cross-compile cmf-cli
  
    > npm run build:prod 

- publish a pre-release to predefined repo. 
  
    > npm run publish

- if we are publishing a *live* release, run

    > npm run publish:live

- <u>COMMIT the following files</u>

  > cmf-cli\npm\package.json
  >
  > cmf-cli\npm\package-lock.json



An example flow for a bugfix would be:

- fix a bug in cli
- `npm run build:clean`
- `npm run bump:pre`
- `npm run build:prod`
- `npm run publish`
- install @criticalmanufacturing/cli@next and test your changes
- `npm run bump:patch`
- `npm run publish:live` to push the update to `latest`
- commit the updated package files
