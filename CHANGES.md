# Changelog

All notable changes to this project will be documented in this file. See [standard-version](https://github.com/conventional-changelog/standard-version) for commit guidelines.

### [3.4.2](https://github.com/criticalmanufacturing/cli/compare/3.4.2-0...3.4.2) (2023-03-06)

* **core**: enhancements for plugin development

### [3.4.2-0](https://github.com/criticalmanufacturing/cli/compare/3.4.1...3.4.2-0) (2023-02-27)

### [3.4.1](https://github.com/criticalmanufacturing/cli/compare/3.4.0-5...3.4.1) (2023-02-16)


### Bug Fixes

* **log:** double out width if not running on terminal ([1a402dc](https://github.com/criticalmanufacturing/cli/commits/1a402dc32cc0e96cfb55e09b5d10972b975e51f4))

## [3.4.0](https://github.com/criticalmanufacturing/cli/compare/3.4.0-5...3.4.0) (2023-02-15)

## [3.4.0-5](https://github.com/criticalmanufacturing/cli/compare/3.4.0-4...3.4.0-5) (2023-02-14)


### Features

* **pr-changes:** add validation to search for test cases on pullrequest workitems ([b974fab](https://github.com/criticalmanufacturing/cli/commits/b974fabc79d171524e7a0259af64071ef7eccb58))

## [3.4.0-4](https://github.com/criticalmanufacturing/cli/compare/3.4.0-3...3.4.0-4) (2023-02-08)


### Bug Fixes

* pin node typings to the node version ([748489b](https://github.com/criticalmanufacturing/cli/commits/748489bfe6de45016bef730484cb156b8ba6a0cb))

## [3.4.0-3](https://github.com/criticalmanufacturing/cli/compare/3.4.0-2...3.4.0-3) (2023-02-07)

## [3.4.0-2](https://github.com/criticalmanufacturing/cli/compare/3.4.0-1...3.4.0-2) (2023-01-30)


### Features

* add plugin scaffolding command ([03d6023](https://github.com/criticalmanufacturing/cli/commits/03d60237335315f745dc7832a2f338d522ef11a3))
* allow commands to specify an Id in addition to the name ([977d772](https://github.com/criticalmanufacturing/cli/commits/977d77284b633ff0db36854db9c6f51c05403e0b))
* **build:** collect test output for coverage analysis ([e10fed2](https://github.com/criticalmanufacturing/cli/commits/e10fed2f6cb73fd0b9797555c0840f1c9c6efbed))
* **build:** publish test and coverage reports for displaying in pipeline UI ([163ba11](https://github.com/criticalmanufacturing/cli/commits/163ba11cdc62a3cc2ffb451a6eb7d23789376348))
* **restore dependencies:** move CopyLibs to cmf-cli restore dependencies ([9ee2573](https://github.com/criticalmanufacturing/cli/commits/9ee25733b38b54f3b7aa65862522d9fb9a53f3df))


### Bug Fixes

* allow -v version shorthand without executing the root command ([9a7fb5e](https://github.com/criticalmanufacturing/cli/commits/9a7fb5e9a8963a78c43c02f9330e597eb0e8b640))

## [3.4.0-1](https://github.com/criticalmanufacturing/cli/compare/v3.3.1...3.4.0-1) (2023-01-10)

## [3.4.0-0](https://github.com/criticalmanufacturing/cli/compare/v3.3.0...3.4.0-0) (2023-01-04)

### [3.3.1](https://github.com/criticalmanufacturing/cli/compare/v3.3.0...v3.3.1) (2023-01-10)


### Bug Fixes

* missing replace on CI-Release.yml on PR [#232](https://github.com/criticalmanufacturing/cli/issues/232) ([e0c5a4f](https://github.com/criticalmanufacturing/cli/commits/e0c5a4ff23fabd73d1327b51d10c1bb46d5bfb44))

## [3.3.0](https://github.com/criticalmanufacturing/cli/compare/3.3.0-1...3.3.0) (2022-12-14)

## [3.3.0-1](https://github.com/criticalmanufacturing/cli/compare/3.3.0-0...3.3.0-1) (2022-12-06)

## [3.3.0-0](https://github.com/criticalmanufacturing/cli/compare/3.2.0...3.3.0-0) (2022-12-05)


### Features

* **pipelines:** refactor pr and ci pipelines ([5b2c61c](https://github.com/criticalmanufacturing/cli/commits/5b2c61c15911cb191f258d7257241ab1ac117858))


### Bug Fixes

* **build help:** make legacy package check more permissive to style ([3d97c7f](https://github.com/criticalmanufacturing/cli/commits/3d97c7f7899efbeca25f3127992fcbd1b6ebfcdb))

## [3.2.0](https://github.com/criticalmanufacturing/cli/compare/3.2.0-5...3.2.0) (2022-11-29)

## [3.2.0-5](https://github.com/criticalmanufacturing/cli/compare/3.2.0-4...3.2.0-5) (2022-11-28)


### Bug Fixes

* downgrade global-dirs to 3.0.0 ([0a59642](https://github.com/criticalmanufacturing/cli/commits/0a59642787c9d6863b8d73f34287cae4142d321b))

## [3.2.0-4](https://github.com/criticalmanufacturing/cli/compare/3.2.0-3...3.2.0-4) (2022-11-24)


### Features

* add node tooling version to UI packages ([c276ce7](https://github.com/criticalmanufacturing/cli/commits/c276ce77d7db431ee34d08adc51c459833f0ad5e))

## [3.2.0-3](https://github.com/criticalmanufacturing/cli/compare/3.2.0-2...3.2.0-3) (2022-11-23)

## [3.2.0-2](https://github.com/criticalmanufacturing/cli/compare/3.2.0-1...3.2.0-2) (2022-11-22)


### Bug Fixes

* **builds:** fixed issues caused by azuredevops update ([c847312](https://github.com/criticalmanufacturing/cli/commits/c8473127556daf2e034857250c7e57d0e5c34b13))


### Under the hood

* make telemetry service parameterized to be used in plugins ([4a8f6b4](https://github.com/criticalmanufacturing/cli/commits/4a8f6b431eafd4e9a27c14f6c4955f676bbffdad))

### [3.1.3](https://github.com/criticalmanufacturing/cli/compare/3.1.2...3.1.3) (2022-10-25)


### Bug Fixes

* set dependencies folder based on the context package ([1986da7](https://github.com/criticalmanufacturing/cli/commits/1986da7e977b1817ab28699dffbc6d806f57d8ab))
* support BuildablePackages on data packages ([8110271](https://github.com/criticalmanufacturing/cli/commits/8110271c3c8979d1d496752cfd86a0873208a407)), closes [#66](https://github.com/criticalmanufacturing/cli/issues/66)

## [3.2.0-1](https://github.com/criticalmanufacturing/cli/compare/3.1.3...3.2.0-1) (2022-11-03)


### Features

* add command for plugin discovery ([d00f8b7](https://github.com/criticalmanufacturing/cli/commits/d00f8b796b18426b390e7715d5dbbbc6065e1680))
* **init:** allow namespacing the pipelines for multi-site projects ([a360854](https://github.com/criticalmanufacturing/cli/commits/a360854fbdd67a15c909d16b9e6f2fabe8f7d1d6))


### Bug Fixes

* append cmf.core.app to HTML packages, isn't used in ISO but is needed in bundles for containers ([cd79f03](https://github.com/criticalmanufacturing/cli/commits/cd79f0331799137201db6797135eaea6f7dd0801))
* **bump:** support single and double quotes on metadata ts file ([cfe7f22](https://github.com/criticalmanufacturing/cli/commits/cfe7f225046bb52ff60e5a0e1852e5fcc2c8294e))
* change CD-Containers: had an hardcoded agent pool ([60e0cab](https://github.com/criticalmanufacturing/cli/commits/60e0cab56ecbc80ce51527aafe527c75b48f3175))
* fail gracefully when there's no internet to check new versions (fixes [#218](https://github.com/criticalmanufacturing/cli/issues/218)) ([67f642d](https://github.com/criticalmanufacturing/cli/commits/67f642d171af5b491945599a280c46cb9845d71c))
* **new feature:** add MES dependencies to root feature scaffolding, they are mandatory ([70bbdb1](https://github.com/criticalmanufacturing/cli/commits/70bbdb1c12bd6683dda2c28206747f763c1f9685))
* **pipeline namespacing:** namespace triggers in release pipelines ([77dc943](https://github.com/criticalmanufacturing/cli/commits/77dc9433f9077c5b59618c4d6b56ea916e44dd98))

### [3.1.2](https://github.com/criticalmanufacturing/cli/compare/3.1.2-0...3.1.2) (2022-10-17)

### [3.1.2-0](https://github.com/criticalmanufacturing/cli/compare/3.1.1...3.1.2-0) (2022-09-23)


### Bug Fixes

* build dotnet projects on data package build ([66443bf](https://github.com/criticalmanufacturing/cli/commits/66443bffee1560869a43375be16a731869127503))
* iot package changing html config.json with wrong version ([74fe8e8](https://github.com/criticalmanufacturing/cli/commits/74fe8e89da5d9b2ffe10b25dc0a58b5c766ebd08)), closes [#202](https://github.com/criticalmanufacturing/cli/issues/202)
* serialization changing workingDirectory>buildSteps ([3fc8b18](https://github.com/criticalmanufacturing/cli/commits/3fc8b181908dff52dcdb3bb188f204929688f4ee)), closes [#190](https://github.com/criticalmanufacturing/cli/issues/190)

### [3.1.1](https://github.com/criticalmanufacturing/cli/compare/3.1.1-0...3.1.1) (2022-09-23)

### [3.1.1-0](https://github.com/criticalmanufacturing/cli/compare/3.1.0...3.1.1-0) (2022-09-15)


### Bug Fixes

* **pack:** gate new IoT steps to DF versions that actually support them ([c419586](https://github.com/criticalmanufacturing/cli/commits/c41958672777a86c7a27f51b33753012410b5a5d))


## [3.1.0](https://github.com/criticalmanufacturing/cli/compare/3.1.0-2...3.1.0) (2022-08-24)

## [3.1.0-2](https://github.com/criticalmanufacturing/cli/compare/3.1.0-1...3.1.0-2) (2022-07-22)


### Features

* Add ConnectIoT deployment steps to IoT Package ([6a27664](https://github.com/criticalmanufacturing/cli/commits/6a27664ff1c913e527e8c0f177ae54ac19e1489c))

## [3.1.0-1](https://github.com/criticalmanufacturing/cli/compare/3.1.0-0...3.1.0-1) (2022-07-11)


### Bug Fixes

* **init:** derive repository name from valid repository url ([6de229e](https://github.com/criticalmanufacturing/cli/commits/6de229e694140d99728aa795d235d804500e8af6)), closes [#160](https://github.com/criticalmanufacturing/cli/issues/160)

## [3.1.0-0](https://github.com/criticalmanufacturing/cli/compare/3.0.0...3.1.0-0) (2022-06-30)


### Features

* start capturing test coverage when running unit tests ([30d6bb7](https://github.com/criticalmanufacturing/cli/commits/30d6bb71561862f97140aa55474eb598223ec77c))

## [3.0.0](https://github.com/criticalmanufacturing/cli/compare/3.0.0-9...3.0.0) (2022-06-20)

## [3.0.0-9](https://github.com/criticalmanufacturing/cli/compare/3.0.0-8...3.0.0-9) (2022-06-13)


### Features

* add basic telemetry ([f7d0c87](https://github.com/criticalmanufacturing/cli/commits/f7d0c87735b912e8555046c5bf5e1c882074b09c))
* add extended telemetry for commands ([3e60463](https://github.com/criticalmanufacturing/cli/commits/3e60463921f99e5655dab8f9b6d49a20c96b02e0))


### Bug Fixes

* assemble object reference not set [#148](https://github.com/criticalmanufacturing/cli/issues/148) ([745be6f](https://github.com/criticalmanufacturing/cli/commits/745be6f0e8ca8c74838fbf2d1656c96acd245cf1))
* encode absolute dotnet version for Azure DevOps agents ([1ed5b9b](https://github.com/criticalmanufacturing/cli/commits/1ed5b9b6ba78ba71250e9c91b20da192069c2ffc))


### Under the hood

* make telemetry opt-in ([3422745](https://github.com/criticalmanufacturing/cli/commits/342274522a528b358d59c25174dc1e4e9e048bec))

## [3.0.0-8](https://github.com/criticalmanufacturing/cli/compare/3.0.0-7...3.0.0-8) (2022-06-01)


### Bug Fixes

* support legacy package formats for generateBasedOnTemplates command ([097d0b2](https://github.com/criticalmanufacturing/cli/commits/097d0b2e306fbf77fde151b273d0f3327396f7b4))

## [3.0.0-7](https://github.com/criticalmanufacturing/cli/compare/3.0.0-6...3.0.0-7) (2022-05-26)


### Features

* add new build step to run unit tests on build ([66c5c01](https://github.com/criticalmanufacturing/cli/commits/66c5c01fdbae68720c49baa5a216d476e69e2d75))
* add new build step to run unit tests on build ([fdca38c](https://github.com/criticalmanufacturing/cli/commits/fdca38cafa0e9dde424b2e982f69c02791967039))
* **pipelines:** run unit tests on pull request pipelines ([89fc302](https://github.com/criticalmanufacturing/cli/commits/89fc302da166481773551ef67e46ea21bce4b892))


### Bug Fixes

* **new database:** include database projects for database scaffolding ([e265469](https://github.com/criticalmanufacturing/cli/commits/e265469db57f9dc1ca1636ae98e84c27a75bb609))

## [3.0.0-6](https://github.com/criticalmanufacturing/cli/compare/3.0.0-5...3.0.0-6) (2022-05-23)


### Bug Fixes

* **pack:** ensure zip paths always use forward slash ([7c2766a](https://github.com/criticalmanufacturing/cli/commits/7c2766a72dea94742eea0a696fa0b59044bb5ee4))
* **pipelines:** change trigger format issue ([7caa55f](https://github.com/criticalmanufacturing/cli/commits/7caa55f322484b652f05b87a10e579799ff97cd5))

## [3.0.0-5](https://github.com/criticalmanufacturing/cli/compare/3.0.0-4...3.0.0-5) (2022-05-19)


### Bug Fixes

* **init:** add support for MES runtime environments ([2406192](https://github.com/criticalmanufacturing/cli/commits/24061926e9e2aa8f3767d1b7d98feb6c585de884))

## [3.0.0-4](https://github.com/criticalmanufacturing/cli/compare/3.0.0-3...3.0.0-4) (2022-05-17)


### Features

* **new:** add scaffolding compatible with .NET 6 and IoC for MES 9 and up ([4b47666](https://github.com/criticalmanufacturing/cli/commits/4b47666dbe34a8065eb647e62c8bd74f148ceca4))
* publish Core NuGet ([d7cf7eb](https://github.com/criticalmanufacturing/cli/commits/d7cf7eb021585972d65a21dfd6fd89d2d6e29ad8))


### Bug Fixes

* repos arg was being cleared by ExecutionContext.Instance initialization ([2e67a09](https://github.com/criticalmanufacturing/cli/commits/2e67a0925da5a80bf0aac908b924c5a6fa30ff27))

## [3.0.0-3](https://github.com/criticalmanufacturing/cli/compare/3.0.0-2...3.0.0-3) (2022-05-12)


### Features

* **help:** support multiple custom generated doc packages per doc portal instance ([a32878f](https://github.com/criticalmanufacturing/cli/commits/a32878fc73f42c059acff0436ca0dca722abba18))


### Bug Fixes

* **help:** avoid help menu item collisions ([156a227](https://github.com/criticalmanufacturing/cli/commits/156a2271e9fa1de66d2a341d8dfe8b92c715c0c3))

## [3.0.0-2](https://github.com/criticalmanufacturing/cli/compare/3.0.0-1...3.0.0-2) (2022-05-09)


### Features

* add new build step for IoT ([b62ae47](https://github.com/criticalmanufacturing/cli/commits/b62ae47a7b0327b95364bab0a6e4448f1d08b44a))
* adding test mode to single step command ([9da6599](https://github.com/criticalmanufacturing/cli/commits/9da65994194098a84bc9691441c0ff7c6a7bcd6d))
* create build and test with an added option ([5959b9a](https://github.com/criticalmanufacturing/cli/commits/5959b9a41b91e41c7b1766cbd87444cef426c6ce))


### Bug Fixes

* add child process import ([86bef6a](https://github.com/criticalmanufacturing/cli/commits/86bef6a33e6706be560aab42b91b367dc5b01249))

## [3.0.0-1](https://github.com/criticalmanufacturing/cli/compare/3.0.0-0...3.0.0-1) (2022-05-09)


### Features

* **build:** support manifest build steps for generic packages ([10df538](https://github.com/criticalmanufacturing/cli/commits/10df538edb4771935d0a0f57bd86caba1e6a47d4))
* force product dependencies as part of Root dependencies package ([4d8db6c](https://github.com/criticalmanufacturing/cli/commits/4d8db6cdb045511932691271ea069b20a3542966))


### Bug Fixes

* **ls:** correct printed tree found package labels ([cca485e](https://github.com/criticalmanufacturing/cli/commits/cca485ede5e2be4198f7931998737158222446a7))

## [3.0.0-0](https://github.com/criticalmanufacturing/cli/compare/2.3.1...3.0.0-0) (2022-04-29)


### ⚠ BREAKING CHANGES

* migrate to .NET 6

### Features

* **pipelines:** support package installation by environment ([da7aca4](https://github.com/criticalmanufacturing/cli/commits/da7aca48c7f36bb22f2ea86038396e9b607236a6))


### Bug Fixes

* database template packages are not uniqueinstall [#169](https://github.com/criticalmanufacturing/cli/issues/169) ([cfbaca2](https://github.com/criticalmanufacturing/cli/commits/cfbaca219cc48f6ebc2e03682f90cea953b0a5b2))


### Under the hood

* add Core Nuget spec ([464119b](https://github.com/criticalmanufacturing/cli/commits/464119b93e8b7b18f08c1e0000e7bdef6581c3ef))
* harmonize CLI namespaces ([7099052](https://github.com/criticalmanufacturing/cli/commits/70990527ae8424ee3cd86df7c88446a15464aebb))
* harmonize core namespaces ([23ba5c0](https://github.com/criticalmanufacturing/cli/commits/23ba5c0b15a9f4aa29c35bb28d59595c614df3a6))
* move core objects to a separate assembly ([232d432](https://github.com/criticalmanufacturing/cli/commits/232d43295fe508d40080e38562eb47c2b4b6885e))


* migrate to .NET 6 ([881b90b](https://github.com/criticalmanufacturing/cli/commits/881b90b8cb869197e7110de6aceac130dbf33949))

### [2.3.1](https://github.com/criticalmanufacturing/cli/compare/2.3.1-0...2.3.1) (2022-04-26)

### [2.3.1-0](https://github.com/criticalmanufacturing/cli/compare/2.3.0...2.3.1-0) (2022-04-13)


### Bug Fixes

* **pack:** package type handler was overriding dfPackageType for Generic packages ([e0b50e7](https://github.com/criticalmanufacturing/cli/commits/e0b50e797d764f392ca25256404483c4c08d818d))

## [2.3.0](https://github.com/criticalmanufacturing/cli/compare/2.3.0-0...2.3.0) (2022-04-12)

## [2.3.0-0](https://github.com/criticalmanufacturing/cli/compare/2.2.0...2.3.0-0) (2022-04-06)


### Features

* allow DFPackageType to be deserialized ([0e43b4c](https://github.com/criticalmanufacturing/cli/commits/0e43b4cebf6899c1fd2032cbeb91f03a27bf6591))
* allow the IsMandatory property to be deserialized ([cb54f25](https://github.com/criticalmanufacturing/cli/commits/cb54f25f8fb75ff5423d9df2a6819c6861559c5c))


### Bug Fixes

* hostUseSSL in tests BaseContext file ([1b32a61](https://github.com/criticalmanufacturing/cli/commits/1b32a6115e8d2f614be856de5e8f68d12b60c618))

## [2.2.0](https://github.com/criticalmanufacturing/cli/compare/2.2.0-5...2.2.0) (2022-03-31)

## [2.2.0-5](https://github.com/criticalmanufacturing/cli/compare/2.2.0-4...2.2.0-5) (2022-03-28)


### Features

* securityPortal New Command ([6d5903c](https://github.com/criticalmanufacturing/cli/commits/6d5903cd5b41b745149e2eb2d7dce1b112f1d2b6))
* securityPortal Pack Command ([cf9fa29](https://github.com/criticalmanufacturing/cli/commits/cf9fa29cadc2bda9de8a738ff47c707c0900eb0b))


### Bug Fixes

* **pack:** support packing presentation packages without any UI package ([3de07ab](https://github.com/criticalmanufacturing/cli/commits/3de07ab714ecaee102f8d75f5fe3131b128a5193))
* **pack:** support relative LBO paths for feature packages ([5e20dd5](https://github.com/criticalmanufacturing/cli/commits/5e20dd5b3f36ce7e430e8dd9b28c67137812f2ce))


### Under the hood

* change GetPackageJsonFile to Generic GetFile ([4d10c29](https://github.com/criticalmanufacturing/cli/commits/4d10c29b4338d1c19a6bb8c7b195855b93d380dd))

## [2.2.0-4](https://github.com/criticalmanufacturing/cli/compare/2.2.0-3...2.2.0-4) (2022-03-23)


### Bug Fixes

* **pack:** additional contentToPack check ([93f6d02](https://github.com/criticalmanufacturing/cli/commits/93f6d02645625166b22a3d71a816f007ffc0694c))

## [2.2.0-3](https://github.com/criticalmanufacturing/cli/compare/2.2.0-2...2.2.0-3) (2022-03-23)

(internal release pipeline changes)

## [2.2.0-2](https://github.com/criticalmanufacturing/cli/compare/2.2.0-1...2.2.0-2) (2022-03-21)


### Features

* add version checks at start up ([c47dfe8](https://github.com/criticalmanufacturing/cli/commits/c47dfe8025f8d4ef38cb57fd4d54751d0cd5d68a))


### Bug Fixes

* allow reading DF packages from read-only repositories ([5573ab3](https://github.com/criticalmanufacturing/cli/commits/5573ab3e435ecd05be2a490e770503158e2d9dd1))
* correct typo on Copy Cmf.FullBackup packages ([62b7b66](https://github.com/criticalmanufacturing/cli/commits/62b7b663f8bce7a9d2e598bf846ad8e6e99e00a5))
* **new HTML:** set webapp as compilable, necessary for bundle generation ([3780234](https://github.com/criticalmanufacturing/cli/commits/37802346c16d8ef2081a8be3ac5516026d88d786))
* **pack:** avoid silent fail ([51a8e26](https://github.com/criticalmanufacturing/cli/commits/51a8e26d68b24d7a9a05d2adcb85dfbfdeb941e9))


### Under the hood

* method ZipDirectory to stream ([bbeb84a](https://github.com/criticalmanufacturing/cli/commits/bbeb84a83a85e3be715b6005cd3cae1cca86fd16))

## [2.2.0-1](https://github.com/criticalmanufacturing/cli/compare/2.2.0-0...2.2.0-1) (2022-03-09)


### Bug Fixes

* allow read of Handler Version ([8e7b404](https://github.com/criticalmanufacturing/cli/commits/8e7b4044006b8691e6e1c385002af8e6a8904fe3))
* **init:** enforce mandatory options ([04e446f](https://github.com/criticalmanufacturing/cli/commits/04e446f5b0cf9f1c67dd06ac3fe44734700c9a18))

## [2.2.0-0](https://github.com/criticalmanufacturing/cli/compare/2.1.1...2.2.0-0) (2022-02-23)


### Features

* add cliPackage as tag in the manifest xml ([01d35c7](https://github.com/criticalmanufacturing/cli/commits/01d35c752c8eb87071302fbe4578d799f7c3bb91))
* **logging:** specify desired log level on invocation ([f8edef9](https://github.com/criticalmanufacturing/cli/commits/f8edef97a1dc63c38161aeb33646c0e24c190e0e))


### Bug Fixes

* continue gracefully when we don't have access to a path in $PATH ([1a852d8](https://github.com/criticalmanufacturing/cli/commits/1a852d89671ea4f24851a03ed1f9ab20f6aafad8))
* isPathInside fails if the APPDATA variable is not defined ([e71531a](https://github.com/criticalmanufacturing/cli/commits/e71531a4a40080effe91049cd369429c1ffd55ce))
* remove vm dependency from generateLBOs.ps1 ([a2c4cee](https://github.com/criticalmanufacturing/cli/commits/a2c4ceee71acfed28aefa06bbcfa9d5f25a18795))
* **restore:** cli was asking for unnecessary write permissions to DF packages ([ddf52f0](https://github.com/criticalmanufacturing/cli/commits/ddf52f0bb3afeb7027268f65d86ae76adb0482d0))

## [2.1.1](https://github.com/criticalmanufacturing/cli/compare/2.1.1-0...2.1.1) (2022-02-17)

### Bug Fixes

* only validate if is local pkg ([3586f53](https://github.com/criticalmanufacturing/cli/commits/3586f53bd5fd180dec330faab2318275d055b419))
* **pipelines:** regex for PackageId ([7a8a146](https://github.com/criticalmanufacturing/cli/commits/7a8a14644b51d876ed00134d4f082486c3e059b3))

## [2.1.1-0](https://github.com/criticalmanufacturing/cli/compare/2.1.0...2.1.1-0) (2022-02-10)

### Bug Fixes

* rename resource Html to HTML ([7b8f5d4](https://github.com/criticalmanufacturing/cli/commits/7b8f5d4c2eeff8bff55994018e007ff641e6046c))
* template IoTData ([36456b6](https://github.com/criticalmanufacturing/cli/commits/36456b601cbfd9440e79bb8fdcbd1a655938befb))
* remove config from html template ([11cb2c5](https://github.com/criticalmanufacturing/cli/commits/11cb2c563b11e2f5a6d00bd43875c7c8a48e64d1))
* getContentToPack to not create target folder ([b78fbae](https://github.com/criticalmanufacturing/cli/commits/b78fbae7f0efb2963dac0dc766d3cd9aebbeb4a9))

## [2.1.0](https://github.com/criticalmanufacturing/cli/compare/2.0.0...2.1.0) (2022-02-07)


### Features

* consistencyCheck Validator ([b0b3e34](https://github.com/criticalmanufacturing/cli/commits/b0b3e34c4d08ebcab7f6d17c0ac9c3661263ae73))


### Bug Fixes

* add StringEnumConverter and Order to Step class ([d377acd](https://github.com/criticalmanufacturing/cli/commits/d377acda999a9485fe54257323da8d2ed15bdad5))
* cd-containers create folder dailybackup before move file ([e93fa3f](https://github.com/criticalmanufacturing/cli/commits/e93fa3f841a2e3aedf97220cee31e6289c046f1a))
* copy runsettings folder independent ([33e15dd](https://github.com/criticalmanufacturing/cli/commits/33e15dd0af920007454d5f7b17f278f068c4a8e8))
* fix test ([61617e8](https://github.com/criticalmanufacturing/cli/commits/61617e826ca3e595f69ffdd35c04a0aee26ab7d7))
* ignore HandlerVersion on serialize ([419dde4](https://github.com/criticalmanufacturing/cli/commits/419dde41e2e59db5e3c4f989f1df08102933490a))
* levels as nullable ([8633aea](https://github.com/criticalmanufacturing/cli/commits/8633aeac82f6a9ac77dcd25926cf4c75257b1471))
* **pipeline:** ci-release add maintenance mode ([c10660e](https://github.com/criticalmanufacturing/cli/commits/c10660e339a4391b8477bbd61ff92b8cdb9c81da))
* register MasterData package in root ([99ce085](https://github.com/criticalmanufacturing/cli/commits/99ce085a0231efe41935ef034610464ea52a4977))
* remove changes ([017f947](https://github.com/criticalmanufacturing/cli/commits/017f9472c5c7d206a4d9dffb1eaf91da315bd01f))
* remove doc ([d4ac3f4](https://github.com/criticalmanufacturing/cli/commits/d4ac3f453caddfc7bc77023078175b11a0f9e7fa))
* remove documentation ([a162af5](https://github.com/criticalmanufacturing/cli/commits/a162af5290217ce8d54def1c5e74a202ed95c95d))
* remove typo ([58f7e70](https://github.com/criticalmanufacturing/cli/commits/58f7e70b6a7058e9ba0bf055e6cc03434f5500d9))
* **templates:** iot, html and tests ([29ebd47](https://github.com/criticalmanufacturing/cli/commits/29ebd47638896aeb050e8b2ded67abb8ec20f660))
* use fileSystem singleton throughout the code ([54f72e6](https://github.com/criticalmanufacturing/cli/commits/54f72e60ec54a8861cb0c612165677b578e1b8eb))

## [2.0.0](https://github.com/criticalmanufacturing/cli/compare/2.0.0-14...2.0.0) (2022-01-10)


### ⚠ BREAKING CHANGES

* **packageType:** rename Test type to Tests

### Bug Fixes

* **pipeline:** add missing trigger ([9236c4e](https://github.com/criticalmanufacturing/cli/commits/9236c4e3f73ffea6342300febfb54134eebd12c9))
* pullrequest comments ([a0ac70b](https://github.com/criticalmanufacturing/cli/commits/a0ac70b8f686776f4f5eb5b53c3e3c80fc3f7021))


### Under the hood

* **new:** naming schema: use format Org.Product.Client.Feature.Rest ([73639ff](https://github.com/criticalmanufacturing/cli/commits/73639ffc633c9d067c392535b398a962b9a72b7e))
* **packageType:** rename Test type to Tests ([a0856b9](https://github.com/criticalmanufacturing/cli/commits/a0856b98d12e740374f674031bb85442b9ac1fec))

## [2.0.0-14](https://github.com/criticalmanufacturing/cli/compare/2.0.0-13...2.0.0-14) (2021-12-27)


### Bug Fixes

* **assemble:** throw Exception instead of CliException ([adc7aaa](https://github.com/criticalmanufacturing/cli/commits/adc7aaa40a67465db8fcbef00af508b58b069c30))

## [2.0.0-13](https://github.com/criticalmanufacturing/cli/compare/2.0.0-12...2.0.0-13) (2021-12-23)


### Bug Fixes

* **assemble:** avoid object reference when test package not found ([933f1ed](https://github.com/criticalmanufacturing/cli/commits/933f1ed6d1b4a13033aafd99f705debbe6612ad2))
* **template:** add missing test masterdata cmfpackage.json ([bd0df9d](https://github.com/criticalmanufacturing/cli/commits/bd0df9d79931948e8826c0e5afe354468bd742dc))

## [2.0.0-12](https://github.com/criticalmanufacturing/cli/compare/2.0.0-11...2.0.0-12) (2021-12-22)


### Bug Fixes

* add xmlInjection to IoT Packages ([b5fb26a](https://github.com/criticalmanufacturing/cli/commits/b5fb26abe138dfcffc4cbe0b81cd9e022d9d6131))

## [2.0.0-11](https://github.com/criticalmanufacturing/cli/compare/2.0.0-10...2.0.0-11) (2021-12-21)


### Bug Fixes

* **test:** improved filelocation string for multios ([4460319](https://github.com/criticalmanufacturing/cli/commits/4460319a7627da8aa75fcc41acb7cd1b211ff673))


### Under the hood

* remove deploymentmetadata ([f1ce481](https://github.com/criticalmanufacturing/cli/commits/f1ce4812afdc76bace62cac5dbf723ed2e114e19))

## [2.0.0-10](https://github.com/criticalmanufacturing/cli/compare/2.0.0-9...2.0.0-10) (2021-12-17)


### Bug Fixes

* **init:** typo ([05c3755](https://github.com/criticalmanufacturing/cli/commits/05c3755856525a1e9ee57ea47bbe442373120ed1))
* **log:** make pack log destination path ([aa279fe](https://github.com/criticalmanufacturing/cli/commits/aa279fe57c04ae101b1e7ac44c90b6af67b62209))
* **template:** ci-release typos ([2d836b6](https://github.com/criticalmanufacturing/cli/commits/2d836b6de3a49c380602814a75c250977e20eefb))

## [2.0.0-9](https://github.com/criticalmanufacturing/cli/compare/2.0.0-8...2.0.0-9) (2021-12-09)


### Bug Fixes

* remove JSON extension in environment config name ([a4048c7](https://github.com/criticalmanufacturing/cli/commits/a4048c7cb0c8a25ab4ab6f8f678dbba7206a6caf))


### Under the hood

* **commands:** allow hidden commands, replacing conditional compiling ([66e1d61](https://github.com/criticalmanufacturing/cli/commits/66e1d61a88d817d34b6a839df754f2251e62b55c))
* **tests:** use Assert.ThrowsException instead of try/catch ([01d807d](https://github.com/criticalmanufacturing/cli/commits/01d807d32cb4263694ff131cab19f1330b6433b1))

## [2.0.0-8](https://github.com/criticalmanufacturing/cli/compare/2.0.0-7...2.0.0-8) (2021-12-03)


### Features

* **data packages:** load master data via Deployment Framework ([6c1bb00](https://github.com/criticalmanufacturing/cli/commits/6c1bb008de2f2003fc978e47faddaa977afa9ebf))

## [2.0.0-7](https://github.com/criticalmanufacturing/cli/compare/2.0.0-6...2.0.0-7) (2021-11-26)


### Bug Fixes

* **assemble:** consider testPackages ([01dde23](https://github.com/criticalmanufacturing/cli/commits/01dde239fedbe6d707de63fd95e337e1f80505c6))

## [2.0.0-6](https://github.com/criticalmanufacturing/cli/compare/2.0.0-5...2.0.0-6) (2021-11-24)


### Features

* **ls:** allow using repositories.json ([7f74b2e](https://github.com/criticalmanufacturing/cli/commits/7f74b2e94187c0455ce3f3208ff227c591617ae6))


### Bug Fixes

* **init:** escape backslashes in repositories.json ([55498c4](https://github.com/criticalmanufacturing/cli/commits/55498c45278895af430f894dfd6f32d928920ec5))
* merge with development-restore-dependencies-command ([510a6cd](https://github.com/criticalmanufacturing/cli/commits/510a6cdce73a219e6c63d51d8820c3cd3c7a1b12))
* **tests:** make mock file systems valid in both windows and *nix ([8c5f65c](https://github.com/criticalmanufacturing/cli/commits/8c5f65c6da5711cbe1077a966ef623e85fcd1908))
* **tests:** make mock file systems valid in both windows and *nix ([4784542](https://github.com/criticalmanufacturing/cli/commits/4784542730eb99d367b4e3722aa178409d8292fb))

## [2.0.0-5](https://github.com/criticalmanufacturing/cli/compare/2.0.0-4...2.0.0-5) (2021-11-23)

### ⚠ BREAKING CHANGES

* **publish:** replace publish command with assemble command [#53](https://github.com/criticalmanufacturing/cli/issues/53) ([d95220c](https://github.com/criticalmanufacturing/cli/commit/d95220c03507066a38171bea2953d2da1bbb9999))
* **pack**: remove dependencies packing - cmf pack will not longer pack any unpacked dependencies [#54](https://github.com/criticalmanufacturing/cli/issues/54) ([3bca78b](https://github.com/criticalmanufacturing/cli/commit/3bca78b6b552a5fcc391acdd6bd2603d9dc62f8f))
* **bump**: remove `all` option - bump will not longer bump the current package's dependencies [#49](https://github.com/criticalmanufacturing/cli/issues/49) ([120040e](https://github.com/criticalmanufacturing/cli/commit/120040efa77093b69deeae1e966888c619edd7ca))


### Features

* **new help:** auto fill default domain and tenant in config.json ([6636705](https://github.com/criticalmanufacturing/cli/commits/6636705100381f48aca378f6e359baf773c6f930))
* **new html:** autofill tenant and default domain in config.json ([8317204](https://github.com/criticalmanufacturing/cli/commits/83172046afb51d7acdc726d22d37c323129bb4c0))
* **restore:** add restore dependencies command ([6ab3529](https://github.com/criticalmanufacturing/cli/commits/6ab3529b2d82e37c4c6fae915b60a1f9a7c39556))
* **restore:** read repositories from file ([1c01d10](https://github.com/criticalmanufacturing/cli/commits/1c01d10646e6ad00d5b3fad6fc0e4eeb191dad33))


### Bug Fixes

* **new business:** typo ([bc11f8a](https://github.com/criticalmanufacturing/cli/commits/bc11f8a3232fccca33362172e7f4b3b00ac1dedb))
* **new iot:** package.json (IoT.Packages) was not valid according to npm ([6498f1b](https://github.com/criticalmanufacturing/cli/commits/6498f1bafe104780aa80343d83d3b97044ca85b5))
* **new:** replace OS specific environment variable ([f5384fa](https://github.com/criticalmanufacturing/cli/commits/f5384fa1ba7f6c2ccdf8de014ad469ce5ba7cbe4))
* **pipelines:** make repository paths consistent ([d8ed6dc](https://github.com/criticalmanufacturing/cli/commits/d8ed6dcc41ac54ebafbd9749f993b299240af5ac))
* **pipelines:** only run PR-Package if enabled and in the same folder as PR-Changes ([9cd8fbc](https://github.com/criticalmanufacturing/cli/commits/9cd8fbcab7a8c5f3ad6d23c108414a76257bcc37))
* **tests:** init tests were broken ([c00f914](https://github.com/criticalmanufacturing/cli/commits/c00f91431d39a0489ec131f72b72bfd0af5f0297))


### Under the hood

* **new:** disable test command in released versions ([4160d04](https://github.com/criticalmanufacturing/cli/commits/4160d049b78365863b8e64ce16e4e29115c06eb1))
* **new:** migrate to new database structure ([adb294a](https://github.com/criticalmanufacturing/cli/commits/adb294a546aa9b46ad9bead09e4d6c87b80c83fd))

## [2.0.0-4](https://github.com/criticalmanufacturing/cli/compare/2.0.0-3...2.0.0-4) (2021-10-26)


### Features

* **init:** [#40](https://github.com/criticalmanufacturing/cli/issues/40) support fr DevOps Center Environment parameters ([8c97248](https://github.com/criticalmanufacturing/cli/commits/8c972483ec8c0e94f9fdd5262370d4c088f847b5))
* **ls:** support multiple repos for ls command ([734c195](https://github.com/criticalmanufacturing/cli/commits/734c195b528210a35837744e228db404251a759f))
* **pack:** support targetLayer in manifests ([4da1b8e](https://github.com/criticalmanufacturing/cli/commits/4da1b8e36363c58b118399247aaea8e6b3adc081))
* support selecting cmf-cli version in pipelines ([101899f](https://github.com/criticalmanufacturing/cli/commits/101899f86a24dae3a96a3a96edb330562f187836))


### Bug Fixes

* **build:** packagePath argument default value didn't always work ([1df212b](https://github.com/criticalmanufacturing/cli/commits/1df212b8cb18460f79a67be368032619fe834469))
* **bump:** change dependency version set to after Execute [#36](https://github.com/criticalmanufacturing/cli/issues/36) ([500b0c6](https://github.com/criticalmanufacturing/cli/commits/500b0c6509c4f1ffcb9e1229683f5151030e8168))
* **installation:** global installation detection did not work with npm installed in AppData ([517e7da](https://github.com/criticalmanufacturing/cli/commits/517e7da73b5924999794664dd192fa9031398080))

## [1.1.2](https://github.com/criticalmanufacturing/cli/compare/1.1.2...1.1.1) (2021-10-15)

### Bug Fixes
* **bump:** [#36](https://github.com/criticalmanufacturing/cli/issues/36) bump command showing wrong message [#47](https://github.com/criticalmanufacturing/cli/issues/47)


## [1.1.1](https://github.com/criticalmanufacturing/cli/compare/1.1.0...1.1.0) (2021-10-15)

### Bug Fixes
* **installation:** [#44](https://github.com/criticalmanufacturing/cli/issues/44) global installation detection did not work with npm installed in AppData (closed by [#46](https://github.com/criticalmanufacturing/cli/issues/46))

## [2.0.0-3](https://github.com/criticalmanufacturing/cli/compare/1.1.0...2.0.0-3) (2021-09-30)


### ⚠ BREAKING CHANGES

* **structure:** move iot bump commands under the bump command

### Features

* add builds ([6410323](https://github.com/criticalmanufacturing/cli/commits/6410323474f588bdfc37a63c44c52249d838bf3f))
* add data package generator ([d30cedc](https://github.com/criticalmanufacturing/cli/commits/d30cedc8880b93abf937d834cebf5c159e2df745))
* add help generator ([1e5305f](https://github.com/criticalmanufacturing/cli/commits/1e5305f2a025eb117ee2bf565184e283845e029e))
* add HTML generator ([298e14b](https://github.com/criticalmanufacturing/cli/commits/298e14be2e00441f5219a29182ff75391f7e0c83))
* add init version package ([be74fb7](https://github.com/criticalmanufacturing/cli/commits/be74fb752bf8eb6eb56625c605be53e2e5a4914c))
* add IoT package generator command ([84494f8](https://github.com/criticalmanufacturing/cli/commits/84494f82c13757b1aa59c982e899824564d476ed))
* add ISO location and GlobalVariables template ([de082de](https://github.com/criticalmanufacturing/cli/commits/de082dea41f6b3ecd965f80164417c04772a9d62))
* Add Json Validator Builder ([12c6a20](https://github.com/criticalmanufacturing/cli/commits/12c6a203431146be0be13af4315680592f9f54c5))
* add test template ([de316f3](https://github.com/criticalmanufacturing/cli/commits/de316f37fcad0f61335a71093ad382d3ae5ef367))
* business layer generator ([21af4bd](https://github.com/criticalmanufacturing/cli/commits/21af4bd0aedb894866d2ec580188c3b72c095d78))
* file renaming ([8119d63](https://github.com/criticalmanufacturing/cli/commits/8119d63206416e4024df03b8588f87b9d349aca8))
* host dotnet-templating in cli ([e32299c](https://github.com/criticalmanufacturing/cli/commits/e32299c7df09efadbf43c6dff883db20fbc7c55c))
* implement cloud config conditionals ([a19c10e](https://github.com/criticalmanufacturing/cli/commits/a19c10effc0f282663d2f53779c5fbc258744411))
* **new command:** add reset switch for templating engine (useful for upgrades) ([4b6257e](https://github.com/criticalmanufacturing/cli/commits/4b6257e9626539710a4fed3a94041242944309d7))
* new feature command ([6a21c65](https://github.com/criticalmanufacturing/cli/commits/6a21c65fdf78b545def2608b6fa07b93e32f42e7))
* **templates:** add database template ([c301513](https://github.com/criticalmanufacturing/cli/commits/c301513f5583e7e0a5cdab3131c64babd3801cec))
* templating - init - WIP ([5731d7d](https://github.com/criticalmanufacturing/cli/commits/5731d7d4c3fc215328e456f2651dc62a51a1e91e))


### Bug Fixes

* **build:** add nuget.config ([4669fc3](https://github.com/criticalmanufacturing/cli/commits/4669fc37c0126a34125a90d6de44aec53183db05))
* **init template:** add missing content. fix nuget url path ([cc772a0](https://github.com/criticalmanufacturing/cli/commits/cc772a05ad13333ba6845609b20bf030f3436113))
* **init:** lbo script had unreplaced tokens ([3aae846](https://github.com/criticalmanufacturing/cli/commits/3aae846850b49fe1bc62ff9a9014c603adf49b43))
* make HTML and Help templates more resistant to config.json variations (introduced by generator-heml 8.1.1) ([79c961a](https://github.com/criticalmanufacturing/cli/commits/79c961a43d84d4c6ea781a4b93ad58fbdfe5e550))
* **pipelines:** fix wrong path ([995a71c](https://github.com/criticalmanufacturing/cli/commits/995a71c97b9bdf701c7f2096fce72d05d952ca8e))
* prevent object reference error when run outside a project ([abab000](https://github.com/criticalmanufacturing/cli/commits/abab000f41a0b3ebe50200215a23a6f3f9f4e034))
* stray quotes ([584d191](https://github.com/criticalmanufacturing/cli/commits/584d19102242837a0a64854aed2e7a9a00a3bd5a))
* **structure:** move iot bump commands under the bump command ([6a5c126](https://github.com/criticalmanufacturing/cli/commits/6a5c1260441864500950e620da0d1ff3bf770dbf))
* **templates:** update HTML bundles path ([0864f92](https://github.com/criticalmanufacturing/cli/commits/0864f92d80a06b4cdebddfdc57d1fd8156575ec2))
* use only CI-Changes in the CI-Builds folder ([51c520c](https://github.com/criticalmanufacturing/cli/commits/51c520cb359b244ab5f32e05cc38686817532108))
* wrong token template in help assets template ([92067d4](https://github.com/criticalmanufacturing/cli/commits/92067d423f87af9b56b0305e8a3dd875f1d8aec4))


### Under the hood

* disable bundling for local Help web app ([e2ddc63](https://github.com/criticalmanufacturing/cli/commits/e2ddc63fb0f39ce9437f339a05798a65811dea01))
* rebase "new business" command on LayerTemplateCommand instead of TemplateCommand ([44a5c43](https://github.com/criticalmanufacturing/cli/commits/44a5c4370aa067cfc172591d90cb40f39f01e121))

## [1.1.0](https://github.com/criticalmanufacturing/cli/compare/1.1.0-2...1.1.0) (2021-07-14)

### Features
* `publish` command - publish from repository to release folder the packages currently references in the dependency tree
* support test packages in cli (this deprecates the old CI-Test pipeline, but its use is optional)

### Bug Fixes

* **tests:** list dependencies fixtures did not work in github test runner ([be4ba9c](https://github.com/criticalmanufacturing/cli/commits/be4ba9c9f30ed1dcf01bb0dc1e7ff24c699c52c2))
* IoT packages: ignore drivers for HTML packages
* Data packages: include and use Master Data Loader config
* Prevent edge cases with plugin loading

### Under the hood

* commit checks ([2b620ff](https://github.com/criticalmanufacturing/cli/commits/2b620ff92167b177ceecc886ef022b1e3a4c0950))

## [1.1.0-2](https://github.com/criticalmanufacturing/cli/compare/1.1.0-1...1.1.0-2) (2021-07-06)

### Bug Fixes

* fixed #16 - IO.Abstractions broke command argument/option parsing in certain situations
* fixed typo in README (license name in README didn't match the actual license in LICENSE.md)

## [1.1.0-1](https://github.com/criticalmanufacturing/cli/compare/1.1.0-0...1.1.0-1) (2021-07-05)

### Under the hood

* refactor IO operations to use System.IO.Abstractions for testing purposes

## [1.1.0-0](https://github.com/criticalmanufacturing/cli/compare/1.0.4...1.1.0-0) (2021-06-23)

### Features
* `ls` command

### Under the hood
* Bug fixes

## [1.0.4](https://github.com/criticalmanufacturing/cli/compare/1.0.4-8...1.0.4) (2021-06-02)

### Bug Fixes
* Include bundles in HTML deployment
* Add debug possibility for installation and execution

## [1.0.4-8](https://github.com/criticalmanufacturing/cli/compare/1.0.4-7...1.0.4-8) (2021-06-01)

### Bug Fixes
* add default deploy step for HTML metadata/i18n bundles

## [1.0.4-7](https://github.com/criticalmanufacturing/cli/compare/1.0.4-6...1.0.4-7) (2021-05-31)

### Under the hood
* Add debugging to installation and execution

## 1.0.4-6 (2021-05-27)
* first public pre-release
