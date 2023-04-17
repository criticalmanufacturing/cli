# Migration Paths

## To 4x
### Pipelines Scaffolding Deprecated
From now on, the Pipelines Scaffolding is done by our internal plugin `cmf-pipeline`, for more information check the documentation.

## To 3x
Default Steps for Business and Data Packages are now inject by the cli. If you are upgrading to cmf-cli 3x you **should remove** all the ***steps*** from the following packages:

 - Business
 - Data
 - IoTData
 - Tests MasterData