# Changelog

All notable changes to this project will be documented in this file. See [standard-version](https://github.com/conventional-changelog/standard-version) for commit guidelines.

## [5.8.0-3](https://github.com/criticalmanufacturing/cli/compare/5.8.0-2...5.8.0-3) (2026-01-27)


### Features

* add Conditional property support to Dependency ([9c7bb36](https://github.com/criticalmanufacturing/cli/commits/9c7bb3634b603dfbc5eba208eedd8371a1a1a4b2))

## [5.8.0-2](https://github.com/criticalmanufacturing/cli/compare/5.8.0-1...5.8.0-2) (2026-01-16)


### Bug Fixes

* duplicate keys now recognise deep array paths ([00a64ef](https://github.com/criticalmanufacturing/cli/commits/00a64ef8de1ff0d359dbcfdf8ef347d6ce7fe7df))

## [5.8.0-1](https://github.com/criticalmanufacturing/cli/compare/5.7.1...5.8.0-1) (2026-01-15)


### Features

* add Tenant option to InitCommand ([86828af](https://github.com/criticalmanufacturing/cli/commits/86828afb705c50c09bb977010ae86b734f7df797))
* added Organization and Product variables to templates ([fb79aec](https://github.com/criticalmanufacturing/cli/commits/fb79aece7bbfada03daba41e63adb9952e0ff284))
* **new test:** add performance tests template ([#633](https://github.com/criticalmanufacturing/cli/issues/633)) ([1b63cf0](https://github.com/criticalmanufacturing/cli/commits/1b63cf09d5efeaabb1f0be038cfe077c4f0e46ce))
* update JSON Validator to check for duplicate keys ([a2c5106](https://github.com/criticalmanufacturing/cli/commits/a2c5106ed43d5bd899226ac30baff4de971c0508))


### Bug Fixes

* **init:** pin docker's version to prevent traefik's failures ([baf0b7b](https://github.com/criticalmanufacturing/cli/commits/baf0b7b8035c2cc452ba03e323226c00aee33ce1))
* only add database steps for types referenced in contentToPack ([1045c34](https://github.com/criticalmanufacturing/cli/commits/1045c34b9c4cc68de13b35325231403b13206ed2))
* removed duplicate keys from test jsons ([fff3cdb](https://github.com/criticalmanufacturing/cli/commits/fff3cdb9a83616829bcf266bb3dd4afa6dbe138f))

## [5.8.0-0](https://github.com/criticalmanufacturing/cli/compare/5.7.1-0...5.8.0-0) (2025-12-26)


### Features

* add support for mdfile prefix on json validatior ([ee29ec5](https://github.com/criticalmanufacturing/cli/commits/ee29ec573a76960cf9b02e6c18ba89e5f1ddc995))
* support for dev setup start manager ([d6c8a99](https://github.com/criticalmanufacturing/cli/commits/d6c8a99971acc06e7b05c41169526ec73c7d913a))


### Bug Fixes

* send oldSystemName to Stpe constructor ([8a84033](https://github.com/criticalmanufacturing/cli/commits/8a840334341857d79549c60efc36be26b03b30d3))

## [5.8.0-0](https://github.com/criticalmanufacturing/cli/compare/5.7.1-0...5.8.0-0) (2025-12-26)


### Features

* add support for mdfile prefix on json validatior ([ee29ec5](https://github.com/criticalmanufacturing/cli/commits/ee29ec573a76960cf9b02e6c18ba89e5f1ddc995))
* support for dev setup start manager ([d6c8a99](https://github.com/criticalmanufacturing/cli/commits/d6c8a99971acc06e7b05c41169526ec73c7d913a))


### Bug Fixes

* send oldSystemName to Stpe constructor ([8a84033](https://github.com/criticalmanufacturing/cli/commits/8a840334341857d79549c60efc36be26b03b30d3))

### [5.7.1](https://github.com/criticalmanufacturing/cli/compare/5.7.1-0...5.7.1) (2025-12-29)


### Bug Fixes

* send oldSystemName to Stpe constructor ([5c72d81](https://github.com/criticalmanufacturing/cli/commits/5c72d816991c6a2af098a6a78ff97af751755a83))

### [5.7.1-0](https://github.com/criticalmanufacturing/cli/compare/5.7.0-2...5.7.1-0) (2025-11-28)


### Features

* **help:** cleaning package json ([cc92416](https://github.com/criticalmanufacturing/cli/commits/cc92416be675e611f472dbd7163f55ff7eeaeffe))
* include old-system-name in step and log unknown step attributes ([b9ba976](https://github.com/criticalmanufacturing/cli/commits/b9ba9768c666e223fef64e0d70bf5ff4dcfa8878))
* **telemetry:** improve telemetry startup time ([700059f](https://github.com/criticalmanufacturing/cli/commits/700059f362be2ee9be11c3292d96ac7cd9d05aeb))


### Bug Fixes

* remove default deps removal from CmfPackageV1 ([6fd7c20](https://github.com/criticalmanufacturing/cli/commits/6fd7c20938c1749aeb36f196dfdebcc437b600ba))

## [5.7.0](https://github.com/criticalmanufacturing/cli/compare/5.7.0-2...5.7.0) (2025-11-14)

## [5.7.0-2](https://github.com/criticalmanufacturing/cli/compare/5.7.0-0...5.7.0-2) (2025-11-13)


### Bug Fixes

* enhance URI handling in RepositoriesConfig ([0fbebe3](https://github.com/criticalmanufacturing/cli/commits/0fbebe3f6a5611ee3e10432f3fc425983a466cba))
* improve deployment directory handling for absolute and relative URIs ([4eb7263](https://github.com/criticalmanufacturing/cli/commits/4eb72630ff9f5f697316101f2d8203cd771ef3ba))

## [5.7.0-1](https://github.com/criticalmanufacturing/cli/compare/5.7.0-0...5.7.0-1) (2025-11-12)


### Bug Fixes

* enhance URI handling in RepositoriesConfig ([0fbebe3](https://github.com/criticalmanufacturing/cli/commits/0fbebe3f6a5611ee3e10432f3fc425983a466cba))

## [5.7.0-0](https://github.com/criticalmanufacturing/cli/compare/5.6.0-11...5.7.0-0) (2025-11-07)


### Features

* make ISOLocation optional in project config ([79e2ff7](https://github.com/criticalmanufacturing/cli/commits/79e2ff78d13fb2dccb4243a10d1e1c108bf98025))
* **upgrade-base:** add upgrade base command to cli ([65d93e1](https://github.com/criticalmanufacturing/cli/commits/65d93e1b8d3a4b29792c650feb09fd4501099650))


### Bug Fixes

* improve DEEValidatorCommand error handling with detailed messages ([29e0809](https://github.com/criticalmanufacturing/cli/commits/29e080911ccb0a301b2f3cebce0409e8ec3b7dcd))
* **iot-tasks:** output task selection ([f05628d](https://github.com/criticalmanufacturing/cli/commits/f05628dab12c86d104c720092036c9ac25f6fd08))
* **json validator:** handle empty IsFile property ([#588](https://github.com/criticalmanufacturing/cli/issues/588)) ([2c88eed](https://github.com/criticalmanufacturing/cli/commits/2c88eedd6bc88019bc80d603b7e73d99b3224c65))
* prismjs version 1.30.0 on help scaffold template ([969ed00](https://github.com/criticalmanufacturing/cli/commits/969ed00f695d7be24d0910f14db5852bf6ce25bf))

## [5.5.0](https://github.com/criticalmanufacturing/cli/compare/5.5.0-6...5.5.0) (2025-09-03)

## [5.5.0-6](https://github.com/criticalmanufacturing/cli/compare/5.5.0-5...5.5.0-6) (2025-08-26)


### Bug Fixes

* **core:** re-calculate derived credentials after loading repository URLs ([c285253](https://github.com/criticalmanufacturing/cli/commits/c2852533ac156ced84c2db25b7af1fe79116f24b))

## [5.5.0-5](https://github.com/criticalmanufacturing/cli/compare/5.5.0-4...5.5.0-5) (2025-08-22)


### Bug Fixes

* accept CIFS shares with IP addresses for host definition ([9efafb3](https://github.com/criticalmanufacturing/cli/commits/9efafb36f05edb4830b55b3b3388dd39f85e1282))
* **core:** official critical authenticated repos domains ([13a7264](https://github.com/criticalmanufacturing/cli/commits/13a72644cd80142dd5218684d31610c23e217349))
* **template_feed:** add missing security portal configs ([3ebb247](https://github.com/criticalmanufacturing/cli/commits/3ebb247e9e94dccc92e89f1dffadafac44853fef))
* **template_feed:** ensure we always copy `Cmf.Custom.Tests` dependencies DLL's ([e772eb5](https://github.com/criticalmanufacturing/cli/commits/e772eb581674b6e176e142fcc5bb0255abaf07e8))

## [5.5.0-4](https://github.com/criticalmanufacturing/cli/compare/5.5.0-3...5.5.0-4) (2025-08-19)


### Features

* improve launch json ([#563](https://github.com/criticalmanufacturing/cli/issues/563)) ([33d6078](https://github.com/criticalmanufacturing/cli/commits/33d607802638a9fc8a478d0b22186bd7e7351b95))

## [5.5.0-3](https://github.com/criticalmanufacturing/cli/compare/5.5.0-2...5.5.0-3) (2025-08-19)


### Bug Fixes

* **pack:** fix icon path in root package app pack ([35bad62](https://github.com/criticalmanufacturing/cli/commits/35bad62751d987e5c6d76aea201f2c0fb20d1a6e))
* skip root package entry when converting tgz to zip ([2354d6c](https://github.com/criticalmanufacturing/cli/commits/2354d6c4039bfe6fa6979f05adf00c000309d02f))

## [5.5.0-2](https://github.com/criticalmanufacturing/cli/compare/5.5.0-1...5.5.0-2) (2025-08-18)


### Bug Fixes

* **iot:** new message ([8171afa](https://github.com/criticalmanufacturing/cli/commits/8171afae7e5199acca77e78147a6d1af2f929eb4))

## [5.5.0-1](https://github.com/criticalmanufacturing/cli/compare/5.4.0...5.5.0-1) (2025-08-13)


### Features

* **iot:** support driver extensions ([527eb28](https://github.com/criticalmanufacturing/cli/commits/527eb284e875db36e9a796db63d8b5fcd4642720))
* **npm-publish:** add manifestVersion to manifest ([9ffb38f](https://github.com/criticalmanufacturing/cli/commits/9ffb38fa644ce7cbec12e3d748651e8eb3dc6e39))


### Bug Fixes

* download NPM package file correct extension ([4b0b4e7](https://github.com/criticalmanufacturing/cli/commits/4b0b4e7274cd287b4aadfb8375697ffffbf8f5a1))
* load derived credentials for internal repo operations ([470f776](https://github.com/criticalmanufacturing/cli/commits/470f77626e7888ed8f36c4a9b19fc0ac64300ee0))
* pin scaffolded devcontainer docker buildx version to 0.25.0 ([00976ae](https://github.com/criticalmanufacturing/cli/commits/00976aedf55a2ead5618a7fec36fde0cec9a57a7))
* skip folder entries when converting zip to tgz ([da7f476](https://github.com/criticalmanufacturing/cli/commits/da7f476eaf7768b9c21b5b589d0669824b5d87e4))


### Under the hood

* **core:** use constants where appropriate ([40bdbe2](https://github.com/criticalmanufacturing/cli/commits/40bdbe27def1d4bbd76da1f381819fde45bd3d48))

## [5.5.0-0](https://github.com/criticalmanufacturing/cli/compare/5.4.0-2...5.5.0-0) (2025-08-05)


### Features

* **iot:** driver dotnet ([#497](https://github.com/criticalmanufacturing/cli/issues/497)) ([4ca2062](https://github.com/criticalmanufacturing/cli/commits/4ca206262d74e4e49949f96bb02a6cfb1e9e4d94))

## [5.4.0](https://github.com/criticalmanufacturing/cli/compare/5.4.0-2...5.4.0) (2025-08-05)

## [5.4.0-2](https://github.com/criticalmanufacturing/cli/compare/5.4.0-1...5.4.0-2) (2025-07-31)


### Bug Fixes

* add missing clickhouse-related step types ([1591c33](https://github.com/criticalmanufacturing/cli/commits/1591c33d160c8b031a0adfa22cdb421ada5239f6))
* **business:** fix application version csproj template to include licensed app name ([4074801](https://github.com/criticalmanufacturing/cli/commits/4074801a2d3408c83b1792a86a2748e350fe1574))
* **init:** update app deployment manifest ([5ef9f39](https://github.com/criticalmanufacturing/cli/commits/5ef9f39a84207619dfb8fb676ff86b6f8dd0a437))
* keep package.json when downloading SQL backups from NPM ([9cd5081](https://github.com/criticalmanufacturing/cli/commits/9cd50816460224ec7485277117d7e23b69b1b695))
* publishing clickhouse backup packages ([a759a7a](https://github.com/criticalmanufacturing/cli/commits/a759a7aad87ea19d55db0cc8d69c91b63be95d0a))
* support publish to NPM with duplicated dependency packages ([ed76712](https://github.com/criticalmanufacturing/cli/commits/ed76712d1fcf373b66eae0d938b0cc9fe914b6f2))


### Under the hood

* improve debugging when NPM package is not found ([3150f26](https://github.com/criticalmanufacturing/cli/commits/3150f2613601e34167d7c618dd591a5d1ab8ac8a))
* remove unused dotnet TarReader code ([2205434](https://github.com/criticalmanufacturing/cli/commits/220543450d3d77bf0a98ad5f1081e0fa22c16db9))

## [5.4.0-1](https://github.com/criticalmanufacturing/cli/compare/5.4.0-0...5.4.0-1) (2025-07-09)


### Features

* add shortcut flags to publish to the CI and Release repositories ([cc65a35](https://github.com/criticalmanufacturing/cli/commits/cc65a350844bac2a260a6f6cce8414007ecef0cf))
* authenticate cmf feeds from repository.json with CustomerPortal PAT on cmf login ([b6d72a0](https://github.com/criticalmanufacturing/cli/commits/b6d72a05fb2b12d29c1fc79a034862b2d0fb2ba5))
* **html:** modify index.html and ng serve arguments for apps ([dccb80f](https://github.com/criticalmanufacturing/cli/commits/dccb80ffe139ef6074144e85054118c4538604ea))
* use lbos *.tgz when linking lbos ([36e999e](https://github.com/criticalmanufacturing/cli/commits/36e999eea455f07b34e73e1569d58d2e529769db))


### Bug Fixes

* commited file system suggestion ([1c668ea](https://github.com/criticalmanufacturing/cli/commits/1c668eaff10a6475d594e52a49609689136430b2))
* make scaffolded related packages property cross-platform ([2c72be4](https://github.com/criticalmanufacturing/cli/commits/2c72be4bc922fe403903ca57d3f800d2790c1f13))
* modified HandlePkg function to be internal ([c35006e](https://github.com/criticalmanufacturing/cli/commits/c35006e80686071614bbf2e1a7eb5faee083c9f2))
* moved app package logic to new function ([6f274aa](https://github.com/criticalmanufacturing/cli/commits/6f274aa472206499fcd5ab40cc479e60313a2ee8))

## [5.4.0-0](https://github.com/criticalmanufacturing/cli/compare/5.3.1...5.4.0-0) (2025-06-30)


### Features

* pack tests generates a package.json to allow publishing to NPM ([1a097cd](https://github.com/criticalmanufacturing/cli/commits/1a097cd7108b0f5a254718ea91af83b901ac573b))


### Bug Fixes

* cmf build of test pkgs ignoring user-level nuget configs ([e457d39](https://github.com/criticalmanufacturing/cli/commits/e457d39bb66277af533aaefeb4fb9e00eb5cbe95))
* **html:** fix ng add arguments ([af5f2af](https://github.com/criticalmanufacturing/cli/commits/af5f2af1259fc3222baef4a26fc1f194ff9ea4c5))

### [5.3.1](https://github.com/criticalmanufacturing/cli/compare/5.3.1-0...5.3.1) (2025-06-27)

### [5.3.1-0](https://github.com/criticalmanufacturing/cli/compare/5.3.0...5.3.1-0) (2025-06-11)


### Bug Fixes

* **build:** run tests on relatedPackages ([717f141](https://github.com/criticalmanufacturing/cli/commits/717f141349d617bf45cce157aba310932db7a2c1))
* fixed cmf new test local runsettings scaffold for apps ([d1c5c97](https://github.com/criticalmanufacturing/cli/commits/d1c5c97920619b865e6b5a2241b5ec65476bbf46))
* **iot:** enum handling ([bcfdca2](https://github.com/criticalmanufacturing/cli/commits/bcfdca2bf6716aa734812fb3153c76abf99b6088))
* **iot:** test package reference ([4fa40f5](https://github.com/criticalmanufacturing/cli/commits/4fa40f5b1b4d138f916a04855db1bce8332d5c80))
* login sync cmd succeed even if docker is not installed ([5a91c29](https://github.com/criticalmanufacturing/cli/commits/5a91c2935bc31a2e4bc4ca0cc81274d6dcef61e4))
* updated base context to support security portal authentication ([ca9e9fe](https://github.com/criticalmanufacturing/cli/commits/ca9e9fef4bef45bd6cd86b43fc4a5b8b782b879b))

## [5.3.0](https://github.com/criticalmanufacturing/cli/compare/5.3.0-5...5.3.0) (2025-06-06)

## [5.3.0-5](https://github.com/criticalmanufacturing/cli/compare/5.3.0-4...5.3.0-5) (2025-06-03)


### Bug Fixes

* devcontainer able to run cmf login sync at launch ([8d96d00](https://github.com/criticalmanufacturing/cli/commits/8d96d00f28ebabfbdc6da9b29c704a5b2bfe35de))
* docs command generation correct path ([c2b6f3d](https://github.com/criticalmanufacturing/cli/commits/c2b6f3d90faef2dc0c8ed5db0d7536c3d339e113))
* **login:** fix password label prompt ([606195c](https://github.com/criticalmanufacturing/cli/commits/606195c7ce0ce7417928e7db92e49b371e69eb3f))
* nuget config credentials path on linux ([af43e0b](https://github.com/criticalmanufacturing/cli/commits/af43e0bd31cf44fda3b2382ce1f79bf6a0add495))
* replace docker socket mount with devcontainer feature ([d9a9e9a](https://github.com/criticalmanufacturing/cli/commits/d9a9e9a60b3871267035994f4ff6cc890a786c7f))
* show error message when no repository URL is provided ([7bf6aec](https://github.com/criticalmanufacturing/cli/commits/7bf6aec0d796bd40f7353e13dc976bd0289a8cb9))

## [5.3.0-4](https://github.com/criticalmanufacturing/cli/compare/5.3.0-3...5.3.0-4) (2025-05-27)


### Features

* automatically renew portal token when running `cmf login sync` ([802cb6d](https://github.com/criticalmanufacturing/cli/commits/802cb6dbbcdc0470c7008f4ffd3febb5b1e39b88))
* handle derived credentials at runtime only ([65888f2](https://github.com/criticalmanufacturing/cli/commits/65888f22383b4e014c387e57daef62b8adc93024))


### Bug Fixes

* disable devcontainers docker credential helper ([caa2ef1](https://github.com/criticalmanufacturing/cli/commits/caa2ef1ada58c88f96caeaaef2f7cffec105ecd4))
* write portal token to cmfportaltoken file when syncing ([5f40b08](https://github.com/criticalmanufacturing/cli/commits/5f40b089d467d99b687fde550dafa767bb7fff88))

## [5.3.0-3](https://github.com/criticalmanufacturing/cli/compare/5.3.0-2...5.3.0-3) (2025-05-16)


### Features

* add devcontainer definition to init template ([3a6a373](https://github.com/criticalmanufacturing/cli/commits/3a6a373aa54ac024456138cdb372a02adf49c4bb))
* add devcontainer workflows ([bf77952](https://github.com/criticalmanufacturing/cli/commits/bf77952524efe3e08294cf8456ee6f6d4c390b97))
* create devcontainer feature ([2ab242f](https://github.com/criticalmanufacturing/cli/commits/2ab242fea6347b46b6807636e2f3a384a0c438c6))
* implement put method on CIFSRepositoryClient ([6fb68bd](https://github.com/criticalmanufacturing/cli/commits/6fb68bdad77648972e6441bb6694ed7b1639dbcb))


### Bug Fixes

* repositoryType missing from cmf new data command ([ca776ce](https://github.com/criticalmanufacturing/cli/commits/ca776ce0aa14448cc2bb93338cd15d10d5e2ac65))
* support relative paths when calling help package's build commands ([c69e4d4](https://github.com/criticalmanufacturing/cli/commits/c69e4d449d50344d4f2c81b3069744d2cc47c52a))


### Under the hood

* segregate deleting the template engine cache file for easy reuse, namely when running in a debug ([9d0f7ea](https://github.com/criticalmanufacturing/cli/commits/9d0f7ea9352eec1619a177cfad40813e2abce357))

## [5.3.0-2](https://github.com/criticalmanufacturing/cli/compare/5.3.0-1...5.3.0-2) (2025-05-12)


### Features

* use cmf auth file for npm and cifs repository clients ([2672c71](https://github.com/criticalmanufacturing/cli/commits/2672c7159b2b0137f5a0d600162fbb3ed95aa8a2))


### Bug Fixes

* avoid npx interactive confirmation prompt when installing packages ([9212869](https://github.com/criticalmanufacturing/cli/commits/92128696679ab94e0097c633a507b0567b4d0462))

## [5.3.0-1](https://github.com/criticalmanufacturing/cli/compare/5.3.0-0...5.3.0-1) (2025-04-21)


### Features

* add extract-i18n command ([c75379f](https://github.com/criticalmanufacturing/cli/commits/c75379fb690cb33319788504a16fc1734f030c87))
* add html localize command ([d22f248](https://github.com/criticalmanufacturing/cli/commits/d22f248b999b965595dc31f36db8ea68659b7e78))
* cmf login and cmf sync commands ([c2cbc73](https://github.com/criticalmanufacturing/cli/commits/c2cbc7348073ffb67967b03a7e05b2af738eeb8c))

## [5.3.0-0](https://github.com/criticalmanufacturing/cli/compare/5.2.1...5.3.0-0) (2025-04-07)


### Features

* add feature flag service ([a23d8ad](https://github.com/criticalmanufacturing/cli/commits/a23d8ad53f14365ea1b1af8a336c0ebf11b36a39))
* add package publish command ([5d12c21](https://github.com/criticalmanufacturing/cli/commits/5d12c2156c5e72008e449ad03b3fa65020c4e9cd))
* **repositories:** add archive repository ([ce1e162](https://github.com/criticalmanufacturing/cli/commits/ce1e1627b724572f7dc572352eb200cfeb5aee5d))
* **repositories:** add CIFS repository client ([5de7125](https://github.com/criticalmanufacturing/cli/commits/5de7125a8b29c8cba6f55eda556cc9ec6a622f04))
* **repositories:** add local source client ([47210a3](https://github.com/criticalmanufacturing/cli/commits/47210a3173143e848cc2e68c508aee88b8104aef))
* **repositories:** add NPM repository client ([d66eebc](https://github.com/criticalmanufacturing/cli/commits/d66eebc9d216d5ee27b715fc649f743277499673))


### Under the hood

* add repository locator service ([ff3a4d6](https://github.com/criticalmanufacturing/cli/commits/ff3a4d6f38bab24be8a69e3b695e7ba2b7ec0896))
* separate CmfPackage data object from its methods ([8901819](https://github.com/criticalmanufacturing/cli/commits/890181909f8ccc6309cd7dbe92803ce822c01acb))
* support repo clients in ls command ([f684a05](https://github.com/criticalmanufacturing/cli/commits/f684a05371ffa6ebc9431bfde5cf89a7cff85177))
* support repo clients in restore command ([732428c](https://github.com/criticalmanufacturing/cli/commits/732428ca011af684df700b70d4227441296609cc))

### [5.2.1](https://github.com/criticalmanufacturing/cli/compare/5.2.0...5.2.1) (2025-04-04)


### Bug Fixes

* **iot:** business Scenarios ([36a20b9](https://github.com/criticalmanufacturing/cli/commits/36a20b924e7d8731f7e96f82afd6b605d0300132))

## [5.2.0](https://github.com/criticalmanufacturing/cli/compare/5.2.0-4...5.2.0) (2025-03-12)

## [5.2.0-4](https://github.com/criticalmanufacturing/cli/compare/5.2.0-3...5.2.0-4) (2025-02-27)


### Bug Fixes

* **iot:** add tsconfig.json ([6d8cec2](https://github.com/criticalmanufacturing/cli/commits/6d8cec2438f00f7f5e6c228a2778492886a201c5))

## [5.2.0-3](https://github.com/criticalmanufacturing/cli/compare/5.2.0-2...5.2.0-3) (2025-02-27)


### Features

* **iot:** add launch.json ([2328c4d](https://github.com/criticalmanufacturing/cli/commits/2328c4dc01703549fba20a7ad51c0ce344442173))
* **iot:** support for nvmrc ([86b5042](https://github.com/criticalmanufacturing/cli/commits/86b504232e1e42935058b76309aab82db449e06b))


### Bug Fixes

* **pack:** iot pack on older scaffoldings ([deed148](https://github.com/criticalmanufacturing/cli/commits/deed14874881ba7a31dfaeb7aef0f73af4c047e6))

## [5.2.0-2](https://github.com/criticalmanufacturing/cli/compare/5.2.0-1...5.2.0-2) (2025-02-24)


### Features

* add devcontainer ([ef9acd3](https://github.com/criticalmanufacturing/cli/commits/ef9acd37a049231441c23b5c7e93ed812ac3151e))
* **core:** make template command's filesystem mockable ([fe54792](https://github.com/criticalmanufacturing/cli/commits/fe54792fea840a1c88c9757e95da3bd4e62653de))


### Bug Fixes

* **new help:** scaffolding help package for v11.1 ([c5b6ccf](https://github.com/criticalmanufacturing/cli/commits/c5b6ccff32a1786b0c889b794c1e988f69a2b4c9))
* show warn instead of exception when no repositories are passed ([902528b](https://github.com/criticalmanufacturing/cli/commits/902528bbf076f9676a2e67fc0bdb00b8ed540b18))
* wait for async method to propagate exceptions ([82f0819](https://github.com/criticalmanufacturing/cli/commits/82f0819f85e5d87cdb3c1a37acc240730200d5db))

## [5.2.0-1](https://github.com/criticalmanufacturing/cli/compare/5.2.0-0...5.2.0-1) (2025-02-17)


### Bug Fixes

* **IoT:** only add automation business scenarios and task libraries steps if the IoT package is ATL ([69ea175](https://github.com/criticalmanufacturing/cli/commits/69ea1750e3aedef0397cbc0a364b52ad2294a5c5))
* **new iot:** angular package generation flag for MES >= 10.2.7 ([#467](https://github.com/criticalmanufacturing/cli/issues/467)) ([bc058e6](https://github.com/criticalmanufacturing/cli/commits/bc058e609b23b81067983e2f00dc8bd99e404696))
* update package.json to use latest bundler ([d11713c](https://github.com/criticalmanufacturing/cli/commits/d11713c4cfb75e9a201250ecd2139a46f2ea978b))

## [5.2.0-0](https://github.com/criticalmanufacturing/cli/compare/5.1.0...5.2.0-0) (2025-01-22)


### Features

* **json validator:** validate subworkflows closes [#452](https://github.com/criticalmanufacturing/cli/issues/452) ([61b96c5](https://github.com/criticalmanufacturing/cli/commits/61b96c577ecdf3e92331efef6944da1672bc466b))
* **new iot:** merge generator-iot closes [#450](https://github.com/criticalmanufacturing/cli/issues/450) ([54f1399](https://github.com/criticalmanufacturing/cli/commits/54f1399b35f4a01eaa97733bebebf8ebe32a927c))
* **nodePackageBundler:** iot package ([4d8fcf8](https://github.com/criticalmanufacturing/cli/commits/4d8fcf89d08b407a168664c3a38613250f899a01))
* **restore:** add cifs client to unc paths usage to work on any OS ([e883568](https://github.com/criticalmanufacturing/cli/commits/e883568085ffaabbb2c05567f466f4e966b09064))


### Under the hood

* **installation:** update postinstall to always copy `cmf-cli` to currentdir and prioritize a local `cmf` bin file ([5efe946](https://github.com/criticalmanufacturing/cli/commits/5efe94675ceb86811185441abbdec9226b574449))

## [5.2.0-0](https://github.com/criticalmanufacturing/cli/compare/5.1.0...5.2.0-0) (2025-01-22)


### Features

* **json validator:** validate subworkflows closes [#452](https://github.com/criticalmanufacturing/cli/issues/452) ([61b96c5](https://github.com/criticalmanufacturing/cli/commits/61b96c577ecdf3e92331efef6944da1672bc466b))
* **new iot:** merge generator-iot closes [#450](https://github.com/criticalmanufacturing/cli/issues/450) ([54f1399](https://github.com/criticalmanufacturing/cli/commits/54f1399b35f4a01eaa97733bebebf8ebe32a927c))
* **nodePackageBundler:** iot package ([4d8fcf8](https://github.com/criticalmanufacturing/cli/commits/4d8fcf89d08b407a168664c3a38613250f899a01))
* **restore:** add cifs client to unc paths usage to work on any OS ([e883568](https://github.com/criticalmanufacturing/cli/commits/e883568085ffaabbb2c05567f466f4e966b09064))


### Under the hood

* **installation:** update postinstall to always copy `cmf-cli` to currentdir and prioritize a local `cmf` bin file ([5efe946](https://github.com/criticalmanufacturing/cli/commits/5efe94675ceb86811185441abbdec9226b574449))

## [5.1.0](https://github.com/criticalmanufacturing/cli/compare/5.1.0-1...5.1.0) (2024-11-25)

## [5.1.0-1](https://github.com/criticalmanufacturing/cli/compare/5.1.0-0...5.1.0-1) (2024-11-04)

## [5.1.0-0](https://github.com/criticalmanufacturing/cli/compare/5.0.1...5.1.0-0) (2024-10-30)


### Features

* **init:** Add App environment configuration ([#425](https://github.com/criticalmanufacturing/cli/issues/425)) ([6204946](https://github.com/criticalmanufacturing/cli/commits/620494635d394a04c4842fd9ec3962874513fb13))


### Bug Fixes

* **build:** check angular.json using package folder ([3e4d783](https://github.com/criticalmanufacturing/cli/commits/3e4d783012cfe4912b5fce89a40b6aee66ec4ad3))
* **bump:** prevent wrong assembly files to being changed when running bump from root ([bfcaf6a](https://github.com/criticalmanufacturing/cli/commits/bfcaf6ab61b8df9cf9fa62f47e3d0c191f43ba7b)), closes [#433](https://github.com/criticalmanufacturing/cli/issues/433)

### [5.0.1](https://github.com/criticalmanufacturing/cli/compare/5.0.1-1...5.0.1) (2024-10-08)


### Bug Fixes

* **iot:** change template identity ([b930919](https://github.com/criticalmanufacturing/cli/commits/b930919035011f8351ac657c5d96216b21244089))

### [5.0.1-1](https://github.com/criticalmanufacturing/cli/compare/5.0.1-0...5.0.1-1) (2024-09-09)


### Bug Fixes

* **new html:** add default routing for v11 ([0feba77](https://github.com/criticalmanufacturing/cli/commits/0feba778c42b30ad57e3d96023a3f782bbd6b0ab))

### [5.0.1-0](https://github.com/criticalmanufacturing/cli/compare/5.0.0...5.0.1-0) (2024-09-05)


### Bug Fixes

* App manifest issues ([#418](https://github.com/criticalmanufacturing/cli/issues/418)) ([2d8ec86](https://github.com/criticalmanufacturing/cli/commits/2d8ec86ffc134294c4c9dc30d863d8b305768bac))
* **business:** add .net8 targetFramework for v11 ([9e298cc](https://github.com/criticalmanufacturing/cli/commits/9e298cce346d0094bbc9db50481ce975aad75af5))
* fixed manifest generation for packages with master data for different target platforms ([c95311b](https://github.com/criticalmanufacturing/cli/commits/c95311b39641addcd963d31a09aa4f56bc3fa18c))
* **html:** pin specific jquery-ui version ([759fb13](https://github.com/criticalmanufacturing/cli/commits/759fb135e79737abbb4b5d4ea46db18ba9bb6ce4))
* **new help:** add v11 help template ([6c00977](https://github.com/criticalmanufacturing/cli/commits/6c009773e60650b69fc6d8976d23f5cfed66f1e5))
* **test:** add .net8 targetFramework for v11 ([4fcf1af](https://github.com/criticalmanufacturing/cli/commits/4fcf1afa3efddd7c1d4c4724fe8172e2f20a4879))

## [5.0.0](https://github.com/criticalmanufacturing/cli/compare/5.0.0-2...5.0.0) (2024-08-12)

## [5.0.0-2](https://github.com/criticalmanufacturing/cli/compare/5.0.0-1...5.0.0-2) (2024-07-24)

## [5.0.0-1](https://github.com/criticalmanufacturing/cli/compare/5.0.0-0...5.0.0-1) (2024-07-03)


### Features

* **iot:** implement support for ATLs ([f4e8835](https://github.com/criticalmanufacturing/cli/commits/f4e88354651d5e2eabbac39fdb2ee477450191d4))
* **new:** fix masterdata context ([427043d](https://github.com/criticalmanufacturing/cli/commits/427043d4a8da7f8dd8339416d7134d7c0a32b410))
* **new:** target platform property set on creation ([b12e8ec](https://github.com/criticalmanufacturing/cli/commits/b12e8ece1ab7bb293d9ec92cb06ededa2be070df))
* **pack:** serialize target platform on master data package packing ([37466f4](https://github.com/criticalmanufacturing/cli/commits/37466f41590a010627bf1e2e2e36b6e1a0e65cdb))

## [5.0.0-0](https://github.com/criticalmanufacturing/cli/compare/4.4.0...5.0.0-0) (2024-07-01)


### Features

* **help:** run generate of documents database and markdown links ([4b61851](https://github.com/criticalmanufacturing/cli/commits/4b61851680d013701038938d99455afda437a9df))
* support dependency versions for MES v11 ([e7b95aa](https://github.com/criticalmanufacturing/cli/commits/e7b95aaadd06b98950f896899b293018cbe8142b))


### Bug Fixes

* **build grafana:** targetLayers should be lowercase ([098d4a2](https://github.com/criticalmanufacturing/cli/commits/098d4a26873368175931fcd3d5acaeb4109ed760))
* **build:** remove mandatory iot lib command ([0494014](https://github.com/criticalmanufacturing/cli/commits/04940148606b090533fd8c0063d118a5a9298375))
* **help:** join the `&nbsp;` string ([f2af424](https://github.com/criticalmanufacturing/cli/commits/f2af42440c9f050e360b85ce1bd76127e6be35b3))
* **help:** replace new lines and spaces inside tables generated from templates ([e1a814a](https://github.com/criticalmanufacturing/cli/commits/e1a814aa62b0fa0933cb23dde061875dff80ed66))
* **PackCommand:** use package root instead of '.' as default `workingDir` ([5d79576](https://github.com/criticalmanufacturing/cli/commits/5d795762a998915bcab259788c94020984d84294))
* **Pack:** generate app manifest only if cmfapp.json exists ([b98138e](https://github.com/criticalmanufacturing/cli/commits/b98138e46e95266bbb3a9f17c58231184a614ea2))

## [4.4.0](https://github.com/criticalmanufacturing/cli/compare/4.4.0-0...4.4.0) (2024-05-29)

## [4.4.0-0](https://github.com/criticalmanufacturing/cli/compare/4.3.0...4.4.0-0) (2024-05-21)


### Features

* **scaffolding:** move reporting package to root ([ac3c9f5](https://github.com/criticalmanufacturing/cli/commits/ac3c9f5be71d3737bfa08f00751e21f00f22d9e2))


### Bug Fixes

* pin yeoman version to 4.3.1 on pack ([fc90834](https://github.com/criticalmanufacturing/cli/commits/fc90834a9848556efc30244ec81de118f17ab7f9))
* **scaffolding:** remove hardcoded 0.0.0 from reporting package ([7081eab](https://github.com/criticalmanufacturing/cli/commits/7081eab1b8ca891fbb016a4d1d2a3ad16d684dc1))
* **scaffolding:** set CriticalManufacturing.DeploymentMetadata as not mandatory ([9c4a7b0](https://github.com/criticalmanufacturing/cli/commits/9c4a7b0f148f04b013861f73d9ba35b3a617731e))

## [4.3.0](https://github.com/criticalmanufacturing/cli/compare/4.3.0-0...4.3.0) (2024-05-16)

## [4.3.0-0](https://github.com/criticalmanufacturing/cli/compare/4.2.2...4.3.0-0) (2024-05-03)


### Features

* **init:** support cmfapp.json generation ([0a0d9ed](https://github.com/criticalmanufacturing/cli/commits/0a0d9ed8398aa5035ef703183ddf2f70a01b21b0))
* **init:** support for app deployment manifest ([38dac8b](https://github.com/criticalmanufacturing/cli/commits/38dac8bb7e56e2d88d87048e84944a385e374302))
* **new:** application version ([39c1bf2](https://github.com/criticalmanufacturing/cli/commits/39c1bf2a5fb096118862763cab04c6cd0d3d455a))
* **new:** fix pack readme removal ([9ba6e68](https://github.com/criticalmanufacturing/cli/commits/9ba6e68a07057671a8c6f590b8adb51e83b300e9))
* **new:** grafana custom package ([361f6dc](https://github.com/criticalmanufacturing/cli/commits/361f6dc688b80b2c7201e2b2da9787db8e654e4b))
* **new:** readme update ([787a56b](https://github.com/criticalmanufacturing/cli/commits/787a56b2130179182120d13fc5e23532da282d72))
* **new:** test ([1e03079](https://github.com/criticalmanufacturing/cli/commits/1e03079a8b6e8fc881bd8f3517825e6932f894bb))
* **new:** yml update ([3d2186c](https://github.com/criticalmanufacturing/cli/commits/3d2186cabf3ee61d576c364ebb0447863e103bb4))
* **pack:** support app packing ([c1d031c](https://github.com/criticalmanufacturing/cli/commits/c1d031c5e94f07085be03717c013b44bb6ddb6a1))

### [4.2.2](https://github.com/criticalmanufacturing/cli/compare/4.2.2-1...4.2.2) (2024-04-29)

### [4.2.2-1](https://github.com/criticalmanufacturing/cli/compare/4.2.2-0...4.2.2-1) (2024-04-22)


### Bug Fixes

* **runner:** consider /usr/local installations as global ([65148a2](https://github.com/criticalmanufacturing/cli/commits/65148a2805718d9cb2a6f817ccd1bb2db2053b31))

### [4.2.2-0](https://github.com/criticalmanufacturing/cli/compare/4.2.1...4.2.2-0) (2024-04-21)


### Bug Fixes

* **plugins:** allow instant streaming of plugin outputs and errors ([0fe1dab](https://github.com/criticalmanufacturing/cli/commits/0fe1dab4a9907d52fc6381fe505b748dbc403a97))

### [4.2.1](https://github.com/criticalmanufacturing/cli/compare/4.2.1-0...4.2.1) (2024-04-21)

### [4.2.1-0](https://github.com/criticalmanufacturing/cli/compare/4.2.0...4.2.1-0) (2024-04-16)


### Bug Fixes

* **DEE validation:** support alternate tags used by DEE Debugger ([a59a954](https://github.com/criticalmanufacturing/cli/commits/a59a95462bdb30da1e6a7a74eadeef69f26032f5))


### Under the hood

* **DEE validation:** display all errors instead of exiting on first ([81a19ab](https://github.com/criticalmanufacturing/cli/commits/81a19abf7fb7294e1580ebef4577ab1ede754218))

## [4.2.0](https://github.com/criticalmanufacturing/cli/compare/4.2.0-4...4.2.0) (2024-04-11)

## [4.2.0-4](https://github.com/criticalmanufacturing/cli/compare/4.2.0-3...4.2.0-4) (2024-03-21)

## [4.2.0-3](https://github.com/criticalmanufacturing/cli/compare/4.2.0-2...4.2.0-3) (2024-03-19)


### Bug Fixes

* **init:** make default domain optional, environment may not have AD config ([1b95baa](https://github.com/criticalmanufacturing/cli/commits/1b95baa1df352f28f2879035a6558ad11b0cb707))

## [4.2.0-2](https://github.com/criticalmanufacturing/cli/compare/4.2.0-1...4.2.0-2) (2024-03-11)


### Bug Fixes

* force order by name on getContentToPack method ([fd9ac9a](https://github.com/criticalmanufacturing/cli/commits/fd9ac9af9e6a2e1c206bd72cb5f66cda334039fa))
* **new help:** set release tag on CM packages ([c748b77](https://github.com/criticalmanufacturing/cli/commits/c748b77501e23d284f6b3c9e12be3076701c97a2))

## [4.2.0-1](https://github.com/criticalmanufacturing/cli/compare/4.2.0-0...4.2.0-1) (2024-03-04)


### Bug Fixes

* use hostUseSSL key in IoT tests runsettings template ([f7057d4](https://github.com/criticalmanufacturing/cli/commits/f7057d4613aba1fd469b9361ff79a06e6a1d138d))

## [4.2.0-0](https://github.com/criticalmanufacturing/cli/compare/4.1.2...4.2.0-0) (2024-02-28)


### Features

* **data:** support for dee validation ([82dd89d](https://github.com/criticalmanufacturing/cli/commits/82dd89d1fa5edfe89a43cdc3d488a46402398d6c))
* **data:** support for json workflow path slash validation ([8559edf](https://github.com/criticalmanufacturing/cli/commits/8559edfa7a1b7a6c450fcef6ba693e9454855b8e))

### [4.1.2-0](https://github.com/criticalmanufacturing/cli/compare/4.1.1...4.1.2-0) (2024-02-26)

### [4.1.2](https://github.com/criticalmanufacturing/cli/compare/4.1.1...4.1.2) (2024-02-28)


### Bug Fixes

* downgrade 'tmp' package to maintain node 12 compat ([6b1084c](https://github.com/criticalmanufacturing/cli/commits/6b1084c6d97ca5b0ba4e65011120836e41bece3f))

### [4.1.1](https://github.com/criticalmanufacturing/cli/compare/4.1.1-0...4.1.1) (2024-02-26)

### [4.1.1-0](https://github.com/criticalmanufacturing/cli/compare/4.1.0...4.1.1-0) (2024-02-02)


### Bug Fixes

* **new-iot:** yoeman version ([256e372](https://github.com/criticalmanufacturing/cli/commits/256e372d4cc90a68c715559f1f501739890b23d2))

## [4.1.0](https://github.com/criticalmanufacturing/cli/compare/4.1.0-2...4.1.0) (2024-01-18)

## [4.1.0-2](https://github.com/criticalmanufacturing/cli/compare/4.1.0-1...4.1.0-2) (2024-01-11)


### Features

* **SecurityPortalPackageTypeHandler:** support sp config.json for v10 ([77d6602](https://github.com/criticalmanufacturing/cli/commits/77d66028b5dd681591f5fa169590594390eec7cf))

## [4.1.0-1](https://github.com/criticalmanufacturing/cli/compare/4.1.0-0...4.1.0-1) (2024-01-05)


### Bug Fixes

* process exit code ([acbdc18](https://github.com/criticalmanufacturing/cli/commits/acbdc18d488b3f73d1b12b2583544650e4ffb2c2))

## [4.1.0-0](https://github.com/criticalmanufacturing/cli/compare/4.0.3...4.1.0-0) (2024-01-02)


### Features

* add support for iot drivers ([d8713d2](https://github.com/criticalmanufacturing/cli/commits/d8713d277ec023de0c068543806e8c8b6f674c0e))


### Bug Fixes

* **related packages:** load via static method to ensure we run the default constructor ([c499d0e](https://github.com/criticalmanufacturing/cli/commits/c499d0e2a0ca0650f01179ef70f5350398082043))

### [4.0.3](https://github.com/criticalmanufacturing/cli/compare/4.0.3-3...4.0.3) (2023-12-20)

### [4.0.3-3](https://github.com/criticalmanufacturing/cli/compare/4.0.3-2...4.0.3-3) (2023-12-19)

### [4.0.3-2](https://github.com/criticalmanufacturing/cli/compare/4.0.3-1...4.0.3-2) (2023-12-13)


### Bug Fixes

* cast JValue to String before split ([72c12c4](https://github.com/criticalmanufacturing/cli/commits/72c12c44360a42c4e4306bce53a7d076f9199db5))
* **pack:** transform host config only if MaintenanceId have a dot ([35565f0](https://github.com/criticalmanufacturing/cli/commits/35565f057d133c21eacd9aab708721de2ee6c655))

### [4.0.3-1](https://github.com/criticalmanufacturing/cli/compare/4.0.3-0...4.0.3-1) (2023-11-07)

### [4.0.3-0](https://github.com/criticalmanufacturing/cli/compare/4.0.2...4.0.3-0) (2023-10-30)


### Features

* **help/html:** Cross-env, port and prevent memory leak ([0c59e75](https://github.com/criticalmanufacturing/cli/commits/0c59e75549cd5cac63668eb74c575279cd3e1ac9))
* **new:** add support for IoT Drivers ([d8c1c53](https://github.com/criticalmanufacturing/cli/commits/d8c1c538aafdc1aa28fa249f2f53b760ae71ccf5))


### Bug Fixes

* **build html/help:** fixed token injection for HTML and Help packages ([d77b4fe](https://github.com/criticalmanufacturing/cli/commits/d77b4fe1439d29ef25eb7122b13f37ff2d7a4b70))
* **build html:** move install and build LBOs to link LBOs command ([d542944](https://github.com/criticalmanufacturing/cli/commits/d542944559891dfe7a7d3679e052df704e997fc6))
* **core:** use PackageId from servide provider on template hostIdentifier ([61d8b66](https://github.com/criticalmanufacturing/cli/commits/61d8b66c14d399850a7e3e2e29bde71dd4a47a71))
* **new data:** add business related package by default ([43a5dc7](https://github.com/criticalmanufacturing/cli/commits/43a5dc79be9790fb9e64819613458b586019ea21))
* relatedPackage action order ([3ab2fae](https://github.com/criticalmanufacturing/cli/commits/3ab2faefe0b26878654605d87bd89c21c0404f31))

### [4.0.2](https://github.com/criticalmanufacturing/cli/compare/4.0.2-0...4.0.2) (2023-10-16)

### [4.0.2-0](https://github.com/criticalmanufacturing/cli/compare/4.0.1...4.0.2-0) (2023-10-03)


### Bug Fixes

* **iot:** transform step only makes sense for MES <v10 ([e05d18d](https://github.com/criticalmanufacturing/cli/commits/e05d18d72fb1867379ef86d595e79731cd02f06f))


### Under the hood

* improve error handling running templates ([710e50b](https://github.com/criticalmanufacturing/cli/commits/710e50b8262d4bf2d7bbd6aeb5eff4b4c31f63da))

### [4.0.1](https://github.com/criticalmanufacturing/cli/compare/4.0.1-2...4.0.1) (2023-09-27)

### [4.0.1-2](https://github.com/criticalmanufacturing/cli/compare/4.0.1-1...4.0.1-2) (2023-09-19)


### Bug Fixes

* **build:** install and build lbos package ([35444f7](https://github.com/criticalmanufacturing/cli/commits/35444f70f16926ff35c6b59b21e82b50eb405b7b))

### [4.0.1-1](https://github.com/criticalmanufacturing/cli/compare/4.0.1-0...4.0.1-1) (2023-08-28)


### Bug Fixes

* **build:** check generic type build steps ([1988729](https://github.com/criticalmanufacturing/cli/commits/1988729b3fbf2caebe89eef50c8e0aa96d6e6db2))

### [4.0.1-0](https://github.com/criticalmanufacturing/cli/compare/4.0.0...4.0.1-0) (2023-08-25)


### Bug Fixes

* **init:** workaround for System.CommandLine bug that considers unknown option as argument values ([fae1507](https://github.com/criticalmanufacturing/cli/commits/fae1507c4437bd58b5ceae0b63b29e7f0fddd911))
* **new help:** menugroup typo on helpsrc packages ([9453ed7](https://github.com/criticalmanufacturing/cli/commits/9453ed7347f0d7741299f16ebbb49ddbe8053cb8))

## [4.0.0](https://github.com/criticalmanufacturing/cli/compare/4.0.0-10...4.0.0) (2023-07-25)

## [4.0.0-10](https://github.com/criticalmanufacturing/cli/compare/4.0.0-9...4.0.0-10) (2023-07-17)

## [4.0.0-9](https://github.com/criticalmanufacturing/cli/compare/4.0.0-8...4.0.0-9) (2023-07-13)


### Bug Fixes

* guard DevTasksVersion ([20fcfcb](https://github.com/criticalmanufacturing/cli/commits/20fcfcb8fbf1508dab46d1f97d2284d10862c097))
* reference Nuget.Versioning directly, as we use it for SemanticVersion types ([9bbd982](https://github.com/criticalmanufacturing/cli/commits/9bbd9828f78a25b311fba94403053aabf0a3f9b2))

## [4.0.0-8](https://github.com/criticalmanufacturing/cli/compare/4.0.0-7...4.0.0-8) (2023-07-03)


### Bug Fixes

* iot for versions lesser than v835 ([#319](https://github.com/criticalmanufacturing/cli/issues/319)) ([7264300](https://github.com/criticalmanufacturing/cli/commits/7264300fe707d6819ffd10483c7686e9e8e2cd78))

## [4.0.0-7](https://github.com/criticalmanufacturing/cli/compare/4.0.0-6...4.0.0-7) (2023-07-03)


### Bug Fixes

* change on V9 IoT Build Process ([01428ea](https://github.com/criticalmanufacturing/cli/commits/01428eacd051892d18548181bd88155af79d8f8d))

## [4.0.0-6](https://github.com/criticalmanufacturing/cli/compare/4.0.0-5...4.0.0-6) (2023-06-29)


### Features

* **new:** add Support for New IoT V10 ([a024784](https://github.com/criticalmanufacturing/cli/commits/a02478412d7ece47148117ebfed7b92c2f18f151))

## [4.0.0-5](https://github.com/criticalmanufacturing/cli/compare/4.0.0-4...4.0.0-5) (2023-06-22)


### Bug Fixes

* **build:** use package name instead of workspace key for token replacer ([8216a71](https://github.com/criticalmanufacturing/cli/commits/8216a715dc934a108611b337ee804b970d3f3c55))

## [4.0.0-4](https://github.com/criticalmanufacturing/cli/compare/4.0.0-3...4.0.0-4) (2023-05-30)


### ⚠ BREAKING CHANGES

* **LBOs:** remove deprecated LBO generation script

### Features

* **help packages:** add token placer as we do for html ([08102a9](https://github.com/criticalmanufacturing/cli/commits/08102a970933e3f3184e02fbeea73e439174faae))
* **LBOs:** automatically link LBOs when building UI packages ([1109ad2](https://github.com/criticalmanufacturing/cli/commits/1109ad2f49349156b86ea31b8a98f8713956af30))


### Under the hood

* **LBOs:** remove deprecated LBO generation script ([ed5ec3a](https://github.com/criticalmanufacturing/cli/commits/ed5ec3ae964259e8e6e9be01880844ffe141612f))

## [4.0.0-3](https://github.com/criticalmanufacturing/cli/compare/4.0.0-2...4.0.0-3) (2023-05-16)

## [4.0.0-2](https://github.com/criticalmanufacturing/cli/compare/4.0.0-1...4.0.0-2) (2023-05-08)


### Features

* add validate start/end methods command ([d9e24fa](https://github.com/criticalmanufacturing/cli/commits/d9e24fa9721f5e3a3f5436abd358335d9451673b))


### Under the hood

* **build:** remove .NET bundling from bundle (testing) builds ([60f0328](https://github.com/criticalmanufacturing/cli/commits/60f03284430332ffe130ce421955ec3c8796c66b))

## [4.0.0-1](https://github.com/criticalmanufacturing/cli/compare/4.0.0-0...4.0.0-1) (2023-04-19)


### ⚠ BREAKING CHANGES

* **plugins:** -- is no longer needed/supported: all params are sent to plugin
* **pipelines:** deprecate pipelines scaffolding

### Bug Fixes

* **plugins:** pass null to NPMClient if no registry was specified ([b8b11af](https://github.com/criticalmanufacturing/cli/commits/b8b11af59a93ee53d925d280c94d330e76cf1e9b))


### Under the hood

* **installation:** remove temporary zip file from github releases ([7293ca9](https://github.com/criticalmanufacturing/cli/commits/7293ca9a046f138ccf648639385ddb89ac9fa93b))
* **pipelines:** deprecate pipelines scaffolding ([114a53f](https://github.com/criticalmanufacturing/cli/commits/114a53f06d99356d2ebcefa310a2bf21dc67f218)), closes [#268](https://github.com/criticalmanufacturing/cli/issues/268) [#288](https://github.com/criticalmanufacturing/cli/issues/288)
* **plugins:** bypass System.CommandLine API when invoking plugins ([4ba5e6f](https://github.com/criticalmanufacturing/cli/commits/4ba5e6fb1c633f5ebe436c096aef3ef0c6c6c871))
* support pre-release ngxSchematics and yeoman generators ([2f3bb51](https://github.com/criticalmanufacturing/cli/commits/2f3bb51221a969698dcfcc9ee07ef128e3beef09))

## [4.0.0-0](https://github.com/criticalmanufacturing/cli/compare/3.4.3...4.0.0-0) (2023-04-10)


### ⚠ BREAKING CHANGES

* **pipelines:** remove Docker Variables
* **pipelines:** GlobalVariables file was moved and renamed from
EnvironmentConfigs/GlobalVariables.ymlto Builds/.vars/global.yml
* **pipelines:** Environment(yml) variables file was moved and renamed from
EnvironmentConfigs/ENV_NAME.yml to Builds/.vars/ENV_NAME.yml
* **pipelines:** split all tasks in small pipeline templates that can be reused
* revert Consistency Check
* invoke execution via Parser instead of RootCommand, with a custom invocation pipeline
* upgrade dotnet command line api dependencies to the latest beta

### Features

* build MES v10 help packages ([807564c](https://github.com/criticalmanufacturing/cli/commits/807564ca320f4b987b97017754b47cbeea8051e9))
* **build:** support @angular/cli managed HTML package ([f9f0f96](https://github.com/criticalmanufacturing/cli/commits/f9f0f964d36d615037518564115b9fae32f464ea))
* **bump:** support @angular/cli managed HTML packages ([8090bee](https://github.com/criticalmanufacturing/cli/commits/8090bee3bb236df5a0ccb0f8fde4707b96356e94))
* **new:** allow scaffolding HTML packages for MES v10 ([c59345f](https://github.com/criticalmanufacturing/cli/commits/c59345f2f8aa561e31d63604299bbb4e638a1373))
* **pipelines:** add Authentication Variables closes [#253](https://github.com/criticalmanufacturing/cli/issues/253) ([731b738](https://github.com/criticalmanufacturing/cli/commits/731b738392b41bb64ea13d0fc8f0cdb87bee36b6))
* scaffold a reference doc package for MES v10 ([7a9e2c8](https://github.com/criticalmanufacturing/cli/commits/7a9e2c8b5e22ffb66c2d95bb45ac6257869390ce))
* support scaffolding apps in `cmf init` ([cf41659](https://github.com/criticalmanufacturing/cli/commits/cf41659ad48df876fb1ad73744668d83b144d206))


### Bug Fixes

* missing MES version token replacement in new Feature command ([91406d5](https://github.com/criticalmanufacturing/cli/commits/91406d5d93fad1956b260193fc56c663c12ad91b))


* **pipelines:** improvements on CI-Release and CD-Containers ([b6b77dc](https://github.com/criticalmanufacturing/cli/commits/b6b77dcfac782f9a55a646963e83a01cd55cf3ec))
* upgrade dotnet command line api dependencies to the latest beta ([5281ff1](https://github.com/criticalmanufacturing/cli/commits/5281ff1d727a8d1452da0e7a34b9d812be50cddb))


### Under the hood

* invoke execution via Parser instead of RootCommand, with a custom invocation pipeline ([d9f35b1](https://github.com/criticalmanufacturing/cli/commits/d9f35b1bbbf43b92b0452b7c4ee26db97405a603))
* move TemplaceCommand to Core ([a037440](https://github.com/criticalmanufacturing/cli/commits/a037440e6f20fa1d828e74753c15ae46cb7dc03c))
* **pipelines:** move all environment related variables to Builds/.vars ([eb1f5d2](https://github.com/criticalmanufacturing/cli/commits/eb1f5d2c3149eeedd78f7d4b2875a33c91dd91c7))
* read project-config as a versioned object instead of a JsonDocument ([41cb620](https://github.com/criticalmanufacturing/cli/commits/41cb620386a9d77e3ae776d7fca58d1ce4423417))
* revert Consistency Check ([9d60396](https://github.com/criticalmanufacturing/cli/commits/9d60396cfb6de359297dfb47330f3ce8d8c6d440))

### [3.4.3](https://github.com/criticalmanufacturing/cli/compare/3.4.3-1...3.4.3) (2023-03-21)

### [3.4.3-1](https://github.com/criticalmanufacturing/cli/compare/3.4.3-0...3.4.3-1) (2023-03-14)


### Features

* updated FromManifest to deserialize cliPackageType ([c47285a](https://github.com/criticalmanufacturing/cli/commits/c47285a572319ff0687f300eaec88b612712ecf1))

### [3.4.3-0](https://github.com/criticalmanufacturing/cli/compare/3.4.2...3.4.3-0) (2023-03-08)


### Bug Fixes

* loading repos from repositories.json should create absolute Uris ([57a8d89](https://github.com/criticalmanufacturing/cli/commits/57a8d890678d2ca2d68de76812d610dba4f34c4e))

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
