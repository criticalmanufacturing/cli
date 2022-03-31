# Changelog

All notable changes to this project will be documented in this file. See [standard-version](https://github.com/conventional-changelog/standard-version) for commit guidelines.

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
