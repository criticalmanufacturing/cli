import "reflect-metadata";
import { Task } from "@criticalmanufacturing/connect-iot-controller-engine";
import EngineTestSuite from "@criticalmanufacturing/connect-iot-controller-engine/dist/test";
//#if isProtocol
import { DriverProxyMock } from "@criticalmanufacturing/connect-iot-controller-engine/dist/test/mocks/driver-proxy.mock";
//#endif
// import { DataStoreMock } from "@criticalmanufacturing/connect-iot-controller-engine/dist/test/mocks/data-store.mock";
import * as chai from "chai";

import {
    <%= $CLI_PARAM_ClassName %>Task,
    <%= $CLI_PARAM_ClassName %>Settings
} from "../../../../src/tasks/<%= $CLI_PARAM_TaskName %>/<%= $CLI_PARAM_TaskName %>.task";

describe("<%= $CLI_PARAM_ClassName %> Task tests", () => {

    //#if isProtocol
    let driverMock: DriverProxyMock;
    beforeEach(() => {
        driverMock = new DriverProxyMock();
    });
    //#endif

    // Optional: See container handling under <%= $CLI_PARAM_TaskName %>TestFactory
    // let dataStoreMock: DataStoreMock;
    beforeEach(() => {
        // dataStoreMock = new DataStoreMock();
    });

	// eslint-disable-next-line @typescript-eslint/ban-types
    const <%= $CLI_PARAM_TaskName %>TestFactory = (settings: <%= $CLI_PARAM_ClassName %>Settings | undefined, trigger: Function, validate: Function): void => {

        const taskDefinition = {
            class: <%= $CLI_PARAM_ClassName %>Task,
            id: "0",
            settings: (settings || {
<%= $CLI_PARAM_TestSettingsDefaults %>
            } as <%= $CLI_PARAM_ClassName %>Settings)
        };

        EngineTestSuite.createTasks([
            taskDefinition,
            {
                id: "1",
                class: Task.Task({
                    name: "mockTask"
                })(class MockTask implements Task.TaskInstance {
                    [key: string]: any;
                    _outputs: Map<string, Task.Output<any>> = new Map<string, Task.Output<any>>();

                    async onBeforeInit(): Promise<void> {
                        this["activate"] = new Task.Output<any>();
                        this._outputs.set("activate", this["activate"]);
                        // Create other custom outputs (for the Mock task) here
                    }

                    // Trigger the test
                    async onInit(): Promise<void> {
                        trigger(this._outputs);
                    }

                    // Validate the results
                    async onChanges(changes: Task.Changes): Promise<void> {
                        validate(changes);
                    }
                })
            }
        ], [
            { sourceId: "1", outputName: `activate`, targetId: "0", inputName: "activate", },
            { sourceId: "0", outputName: `success`, targetId: "1", inputName: "success", },
            { sourceId: "0", outputName: `error`, targetId: "1", inputName: "error", },
            // Add more links needed here...
        ],
        //#if isProtocol
        driverMock,
        //#else
        undefined,
        //#endif
        (containerId) => {
            // Change what you need in the container
            // Example:
            // containerId.unbind(TYPES.System.PersistedDataStore);
            // containerId.bind(TYPES.System.PersistedDataStore).toConstantValue(dataStoreMock);
        });
    };

    /**
     * Instructions about the tests
     * It is assumed that there are two tasks:
     *    0 - <%= $CLI_PARAM_ClassName %> Task
     *    1 - Mockup task
     *
     * All Outputs of Mock task are connected to the inputs of the <%= $CLI_PARAM_ClassName %> task
     * All Outputs of <%= $CLI_PARAM_ClassName %> Task are connected to the Mock task inputs
     *
     * You, as the tester developer, will trigger the outputs necessary for the <%= $CLI_PARAM_ClassName %> to be activated
     * and check the changes to see if the <%= $CLI_PARAM_ClassName %> task sent you the correct values
     *
     * Note: This is just an example about how to unit test the task. Not mandatory to use this method!
     */

    it("should get success when activated", (done) => {
        <%= $CLI_PARAM_TaskName %>TestFactory(undefined,
            (outputs: Map<string, Task.Output<any>>) => {
                // Trigger an output
                outputs.get("activate").emit(true);
            }, (changes: Task.Changes) => {
                // Validate the input
                chai.expect(changes["success"].currentValue).to.equal(true);
                // Report the test as a success
                done();
            });
    });
});
