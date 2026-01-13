/*
 * Sample Critical Manufacturing MES Performance Test developed using Grafana k6.  
 * 
 * k6 tests transition through four distinct stages:
 * 1. Init: Loads the script, imports modules, and defines test options. Code here runs once per Virtual User (VU).
 * 2. Setup: Runs once before the test starts. Used to initialize data. This is required to initialize the LBO Service.
 * 3. Test Code: The default function exports the main test logic. Runs repeatedly for each VU.
 * 4. Teardown: Runs once after the test completes. Used for cleanup. Required to safely terminate the LBO Service.
 * 
 * For more details on k6 Lifecycle: https://grafana.com/docs/k6/latest/using-k6/test-lifecycle/
 */

import { check } from 'k6';

// Import directly from the compiled JS file since k6 does not support Node.js module resolution.
import { LboService, Cmf, PerformanceTest } from "../node_modules/cmf-k6/cmf-k6.js";
import GenericServiceManagement = Cmf.Foundation.BusinessOrchestration.GenericServiceManagement;

// Load the connection settings for the LBO service.
// The "open" k6 function is only available in the init context.
const lboConfigs = open("../cmfLbo_config.json");

// Define k6 test options.
// For more details: https://grafana.com/docs/k6/latest/using-k6/k6-options/how-to/
export let options = {
    iterations: 100, // Total number of test iterations.
    vus: 5, // Number of Virtual Users to run concurrently.
    thresholds: {
        // Define pass/fail criteria for metrics.
        // For more details: https://grafana.com/docs/k6/latest/using-k6/thresholds/
        'http_req_duration{scenario:default}': ['p(95)<100'],

        // Exit with error if any validation using 'check' fails.
        checks: ['rate==1'],
    },
    // Provide the test name to the TestTags utility method.
    // It allows for metrics identification on observability.
    tags: PerformanceTest.TestTags("get_site")
}

// The setup function initializes the service configuration.
// It runs once at the beginning of the test lifecycle.
// The returned value (LboServiceConfig) is passed to the default function (VU code).
// The setup is REQUIRED to initialize the LBO Service connection.
//
// In this example, we're also using the setup step to create a `Site` for testing.
export async function setup() {
    // Initialize LBO Service connection
    const instanceSettings = await PerformanceTest.Setup(lboConfigs);

    // Create test `Site`
    const lboService = new LboService(instanceSettings);
    const output: GenericServiceManagement.OutputObjects.CreateObjectOutput = 
        await lboService.call(
            new GenericServiceManagement.InputObjects.CreateObjectInput({
                Object: new Cmf.Foundation.BusinessObjects.Site({
                    Name: `PerfTest_${crypto.randomUUID()}`
                })
            })
        );

    // Return the LBO connection settings and the Object Id of the created `Site`.
    // These will be available in the default function and teardown.
    return {
        settings: instanceSettings,
        siteId: output.Object.Id
    };
}

// The default function represents the main execution loop for Virtual Users.
// This test gets an existing `Site` from CM MES and validates that 95% of
// the requests take less than 100ms.
export default PerformanceTest.Run(async ({settings, siteId}) => {
    
    const lboService = new LboService(settings);

    // Prepare service input
    const getObjectById = new GenericServiceManagement.InputObjects.GetObjectByIdInput({
        Id: siteId,
        Type: 'Site',
        LevelsToLoad: 1
    });

    // Call LBO Service
    const output: GenericServiceManagement.OutputObjects.GetObjectByIdOutput = await lboService.call(getObjectById);
    const site: Cmf.Foundation.BusinessObjects.Site = output.Instance;
    
    // Validate that the returned result is the requested Site.
    // Note: k6 checks do not fail the test suite by default unless a threshold is defined.
    // See: https://grafana.com/docs/k6/latest/using-k6/checks/
    // Assertions can be used as an alternative but currently remain outside of core k6.
    // See: https://grafana.com/docs/k6/latest/using-k6/assertions/
    check(site, {
        'Successfully retrieved Site': (s) => s.Id === getObjectById.Id,
    });
});

// The teardown function runs once after all VUs have finished.
// Teardown is REQUIRED to safely terminate the LBO Service.
// 
// In this example, we're also Terminating the test `Site` created during setup.
export async function teardown({settings, siteId}) {
    const lboService = new LboService(settings);

    // Terminate Site
    const facilityOutput: GenericServiceManagement.OutputObjects.GetObjectByIdOutput = 
        await lboService.call(
            new GenericServiceManagement.InputObjects.GetObjectByIdInput({
                Id: siteId,
                Type: 'Site',
                LevelsToLoad: 1
            })
        );
    await lboService.call(
        new GenericServiceManagement.InputObjects.TerminateObjectInput({
            Object: facilityOutput.Instance
        })
    );

    // Performance Test cleanup
    PerformanceTest.Teardown(settings);
}
