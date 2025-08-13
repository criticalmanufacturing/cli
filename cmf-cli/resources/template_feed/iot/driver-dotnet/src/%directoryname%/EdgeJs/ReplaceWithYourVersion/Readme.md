# Readme

For more information refer to the [Developer Portal](https://developer.criticalmanufacturing.com/explore/guides/customizations/automation/how-tos/customization_generating_node_binaries/).

# Generating NodeJs Binaries

Connect IoT is based on `NodeJs`, node is a JavaScript engine, but is also capable to execute `C++` code, using [libuv](https://nodejs.org/api/addons.html). In order to compile NodeJs for `C++`, node uses [node-gyp](https://github.com/nodejs/node-gyp).

## Example

It is not uncommon when wanting to use the NPM ecosystem to stumble upon packages that require the use of `C++`. This is very common when trying to use packages that are focused on performance.

For example, let's take a look at a project like [node-hid](https://github.com/node-hid/node-hid). If we check their page we can see that the package not only has `javascript` but that it also has `C++`, so it will require that node-gyp compiles the c++ and it will generate a .node binary, that can be imported as a module.

## Connect IoT Customization

Connect IoT customization is based on compiling ahead of time. The packages when they are shipped they must be ready to run. This for c++ originates an issue, it requires that the `.node` c++ files must be shipped with our customization package and ready to run.

These .node files are also platform dependent and also locked to the version of NodeJs they were compiled with. This is important when shipping the customization as it may occur that this was all built in `windows` and must be run on `arm` and it will not work, for example. Connect IoT as a product always guarantees all platforms are supported, but of course in customization you can choose to only support a subset of this.

## Generating .node Binaries

Some packages already come with exported node binaries. For example, [nsfw](https://www.npmjs.com/package/nsfw) if it's installed by running:

```bash
npm i nsfw
```

Will come with a `nsfw.node` in `node_modules/nsfw/build/Release/nsfw.node`. This binary is then the one we require to be shipped with the customization.

If we take a look at [node-hid](https://github.com/node-hid/node-hid), this does not happen. In order to generate the binaries run:

```bash
node-gyp rebuild
```

This will generate an `HID.node` in `node_modules/node-hid/build/Release/HID.node`. We can then use this binary to ship with the customization.

## Adding to the IoT Customization project

Connect IoT uses [node-package-bundler](https://www.npmjs.com/package/@criticalmanufacturing/node-package-bundler) to generate a self contained IoT Package. Add the binary to a folder in your project for example:

```log
📦Cmf.Custom.IoT
┣ 📂Cmf.Custom.IoT.Data
┃ ┗ (...)
┣ 📂Cmf.Custom.IoT.Packages
┃ ┣ 📂src
┃ ┃ ┣ 📂connect-iot-custom-driver
┃ ┃ ┃ ┣ 📂HID
┃ ┃ ┃ ┃ ┗📂1.0.0
┃ ┃ ┃ ┃  ┗ 📜HID.node
┃ ┃ ┃ ┗ 📜packConfig.json
┃ ┃ ┗ 📜...
┃ ┗  📜cmfpackage.json
┗ 📜cmfpackage.json
```

The packConfig.json is where the user configures the behavior of the bundler. In the addons section we can now add a reference to our dependency.

```json
{
    "type": "Component",
    "addons": [
        { "name": "HID", "version": "1.0.0", "fileMask": "*" }
    ],
    (...)
```

When running `cmf pack` or `npm run packagePacker` it will now generate bundle of the package that contains this module.
