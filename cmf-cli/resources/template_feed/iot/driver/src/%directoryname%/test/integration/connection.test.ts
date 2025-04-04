import "reflect-metadata";
import * as inversify from "inversify";
import * as chai from "chai";
import * as chaiSpies from "chai-spies";
import { container as MainContainer } from "../../src/inversify.config";

import { DeviceDriver, CommunicationState, TYPES as COMMUNICATION_TYPES, PropertyValuePair } from "@criticalmanufacturing/connect-iot-driver";
import { TestUtilities } from "@criticalmanufacturing/connect-iot-driver/dist/test";
import { TYPES as COMMON_TYPES, Logger } from "@criticalmanufacturing/connect-iot-common";
import { LoggerMock } from "@criticalmanufacturing/connect-iot-common/dist/test";

chai.use(chaiSpies);

describe("<%= $CLI_PARAM_Identifier %> driver connection tests", () => {
    let container: inversify.Container;

    before(async () => {
    });

    beforeEach((done) => {
        MainContainer.snapshot();
        container = new inversify.Container();
        container.parent = MainContainer;

        container.bind<Logger>(COMMON_TYPES.Logger).to(LoggerMock).inSingletonScope();
        container.bind("Configurations").toConstantValue({
            commands: [],
            communication: {},
            events: [],
            properties: []
        });

        done();
    });

    it("Dummy test", (done) => {
        done();
    });

    afterEach(async () => {
        MainContainer.restore();
    });

    after(async () => {
    });
});