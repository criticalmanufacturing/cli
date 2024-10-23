# Migration To v3x

On @criticalmanaufacturing/cli v3, default steps on cmfpackage.json configuration files for Business and Data Packages are injected by the cli. If you are upgrading to cmf-cli 3x you **should remove** all the ***steps*** from the following packages:

* Business;
* Data;
* IoTData;
* Tests MasterData.
