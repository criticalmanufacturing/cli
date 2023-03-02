# Migration Paths

## To 4x
### CI-Release Deprecated
 - Go to your repo and delete Builds/CI-Release.yml
 - Check the documentation regarding the new [CD-VM](pipelines/cd/index.md#cd-vm-for-windows-machine-installations)

### GlobalVariables file was moved and renamed from EnvironmentConfigs/GlobalVariables.yml to Builds/.vars/global.yml
 - Go to your repo and delete EnvironmentConfigs/GlobalVariables.yml

### Environment(yml) variables file was moved and renamed from EnvironmentConfigs/ENV_NAME.yml to Builds/.vars/ENV_NAME.yml
 - Go to your repo and delete EnvironmentConfigs/ENV_NAME.yml

## To 3x
Default Steps for Business and Data Packages are now inject by the cli. If you are upgrading to cmf-cli 3x you **should remove** all the ***steps*** from the following packages:

 - Business
 - Data
 - IoTData
 - Tests MasterData