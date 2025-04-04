import "reflect-metadata";
import { Container } from "inversify";

import { DeviceDriver, TYPES as COMMUNICATION_TYPES, container as driverContainer } from "@criticalmanufacturing/connect-iot-driver";
import { TYPES } from "./types";
import { <%= $CLI_PARAM_Identifier %>DeviceDriver } from "./driverImplementation";
import { <%= $CLI_PARAM_Identifier %>Handler } from "./<%= $CLI_PARAM_IdentifierCamel %>/<%= $CLI_PARAM_IdentifierCamel %>Handler";

const container = new Container();
container.parent = driverContainer;
container.parent?.bind<Container>(TYPES.Injector).toConstantValue(container);

container.bind("dummy").toConstantValue("dummy"); // Needed to bypass constructor issue with node 12.16 and eventemitter
container.bind<<%= $CLI_PARAM_Identifier %>Handler>(TYPES.<%= $CLI_PARAM_Identifier %>Handler).to(<%= $CLI_PARAM_Identifier %>Handler).inSingletonScope();
// Must place in parent otherwise the driver(common) will not find this
container.parent?.bind<DeviceDriver>(COMMUNICATION_TYPES.Device.Driver).to(<%= $CLI_PARAM_Identifier %>DeviceDriver).inSingletonScope();

export { container };
