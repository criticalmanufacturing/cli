# Downloading and installing Node.js and cmf-cli

To be able to install cfm-cli, you must install Node.js and the npm command line interface using either a Node version manager or a Node installer. **We strongly recommend using a Node version manager like [nvm](https://github.com/nvm-sh/nvm) to install Node.js and npm.** We do not recommend using a Node installer, since the Node installation process installs npm in a directory with local permissions and can cause permissions errors when you run npm packages globally.

```
npm install --global @criticalmanufacturing/cli
```

## Checking your version of cmf-cli and Node.js

To see if you already have Node.js and npm installed and check the installed version, run the following commands:

```
node -v
cmf -v
```
