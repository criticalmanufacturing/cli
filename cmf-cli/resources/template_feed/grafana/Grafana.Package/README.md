# Grafana custom Package Datasources

If you want to know more about data sources in general [Official Documentation here!](https://grafana.com/docs/grafana/latest/datasources/)

This information is for local development only!

In order to create custom data sources or dashboards you should put them in the respective folders.

If you are building an app and want to use the gRPC Data Manager data source you can import the following configuration into your local grafana instance (the variables between <> should be replaced):

``` json
{
  "id": 1,
  "uid": "<deployed-datasource-uid>",
  "orgId": 1,
  "name": "CMF gRPC Datasource",
  "type": "criticalmanufacturing-grpc-datasource",
  "typeName": "CMF gRPC Datasource",
  "typeLogoUrl": "public/plugins/criticalmanufacturing-grpc-datasource/img/logo.svg",
  "access": "proxy",
  "url": "",
  "user": "",
  "database": "",
  "basicAuth": false,
  "isDefault": false,
  "jsonData": {
    "endpoint": "<data-manager-url>:<port>"
  },
  "readOnly": false
}
```