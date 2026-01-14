# CM MES Performance Tests

CM MES Performance Tests implemented using [Grafana k6](https://github.com/grafana/k6/) and leveraging the TypeScript Light Business Objects (LBOs).

## 1. Setup

Ensure [Grafana k6](https://github.com/grafana/k6/) is installed on the local environment.

```bash
$ k6 --version
k6 v1.4.2 (commit/5b725e8a6a, go1.25.4, linux/amd64)
```

Generate the `cmf-k6` LBO package.

```bash
$ cmf dev local lbos
[. . .]
LBOs generated!

$ tree -L 1 Libs/LBOs/K6/cmf_k6
Libs/LBOs/K6/cmf_k6
├── cmf-k6-0.0.1.tgz
├── cmf-k6.d.ts
├── cmf-k6.js
├── extensions
├── lib
├── package.json
└── thirdPartyNotice.json
```

## 2. Configuration

Before running tests, you must configure the connection to your MES instance.

1. **Generate a PAT:** Create a [Personal Access Token](https://help.criticalmanufacturing.com/userguide/administration/security/users/#creating-access-tokens-for-a-user) for your user.
2. **Update Config:** Edit `cmfLbo_config.json` with your environment details and the PAT.

### `cmfLbo_config.json` Structure

```json
{
    "system" : {
      "tenantName" : "MES",
      "address" : "http://localhost",
      "timeout" : 120000,
      "authentication" : {
        "type" : "SecurityPortal",
        "settings" : {
          "clientId" : "MES",
          "accessToken" : "YOUR_PERSONAL_ACCESS_TOKEN"
        }
      }
    }
}
```

**Configuration fields:**

  * `tenantName`: The tenant name of your CM MES instance.
  * `address`: The full address of the instance.
  * `timeout`: Default timeout in milliseconds for requests.
  * `authentication.type`: Authentication method (e.g., "SecurityPortal").
  * `authentication.settings.clientId`: Client ID for the Security Portal.
  * `authentication.settings.accessToken`: Your Personal Access Token.

## 3. Execution

You can run tests using the `k6` CLI directly or via `npm` scripts.

### Basic Execution

```bash
npm start Tests/get_material.ts
```

### With OpenTelemetry

To export metrics to an OpenTelemetry collector, update the [k6.env](./k6.env) file with the appropriate exporter configurations and execute the test using the npm helper task:

```bash
npm run otel Tests/get_material.ts
```

## 4. Creating New Tests

### Init Stage

Create a new TypeScript file in the `Tests` directory. Import the `cmf-k6` package and load the configuration.

```ts 
import { LboService, Cmf, PerformanceTest } from "../node_modules/cmf-k6/cmf-k6.js";

// The "open" function is only available in the init context
const lboConfigs = open("../cmfLbo_config.json");
```

### Setup Stage

The setup function builds the Access Token using the configuration file. This runs once per test.

```ts
export async function setup() {
  return await PerformanceTest.Setup(lboConfigs);
}
```

### Default Stage

This is the default function executed repeatedly for each Virtual User. Initialize the LBO service here to perform calls to the CM MES host.

```ts
export default PerformanceTest.Run(async (settings) => {
  const lboService = new LboService(settings);

  const input = new Cmf.Navigo.BusinessOrchestration.ResourceManagement.InputObjects.GetResourceByNameInput({
    Name: "Baker-01" 
  });
   
  const output = await lboService.call(input);
});
```

> **Note:** The `call` method is always asynchronous. Default timeout is 300000 ms.

### Performance Thresholds

Performance tests must utilize [Thresholds](https://grafana.com/docs/k6/latest/using-k6/thresholds) to enforce pass/fail criteria.

**Example:**
```ts
export const options = {
  thresholds: {
    http_req_failed: ['rate<0.01'], // http errors should be less than 1%
    http_req_duration: ['p(95)<200'], // 95% of requests should be below 200ms
  },
  tags: PerformanceTest.TestTags("get_resource") // set the test name
};
```

## 5. Logging & Debugging

By default, `cmf-k6` suppresses console output, and k6 only shows a final summary. The `cmf-k6` library uses `console.debug()` for internal logging.

To view logs during execution, use the `--verbose` flag:

```bash
k6 run --verbose Tests/get_site.ts
```

Or to save logs to a file:

```bash
k6 run --log-output=file=./k6.log --verbose Tests/get_site.ts
```

For more information on logging, consult the [k6 logging documentation](https://grafana.com/docs/k6/latest/using-k6/k6-options/reference/#log-output).
