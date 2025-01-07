import * as yargs from "yargs";
import * as path from "path";
import { container } from "./inversify.config";
import { handleCompiledAddons, ConfigurationSectionDriver } from "@criticalmanufacturing/connect-iot-driver";
import { Runner } from "@criticalmanufacturing/connect-iot-driver";
import { TYPES as COMMON_TYPES, Logger, Utils, Configuration } from "@criticalmanufacturing/connect-iot-common";

yargs(process.argv.slice(2));
yargs.strict(false);

yargs.usage("Usage: $0 [options]").wrap(0);

// Id
yargs.option("id", { type: "string", description: "Unique identifier of the driver instance Id", required: true });
yargs.option("managerId", { type: "string", default: "", description: "Name of the Manager running the Automation Instance", required: false });
yargs.option("componentId", { type: "string", default: "", description: "Component Id of the Automation Instance", required: false });
yargs.option("entityName", { type: "string", default: "", description: "System entity name associated with the driver", required: false });

// Configuration file
yargs.option("config", { type: "string", default: "config.json", description: "Configuration file to use" });

// Monitor
yargs.option("monitorPort", { alias: "mp", type: "number", description: "Websocket port of monitor", required: true })
     .option("monitorHost", { alias: "mh", type: "string", default: "localhost", description: "websocket address (without port) of monitor" })
     .option("monitorToken", { alias: "mt", type: "string", default: "monitorSecurityToken", description: "Security token to communicate with monitor" });

// Controller link (server)
yargs.option("serverPort", { alias: "sp", type: "number", default: 0, description: "Websocket port (server) to communicate with controller" })
     .option("serverHost", { alias: "sh", type: "string", default: "localhost", description: "Websocket address (server) without port to communicate with controller" });

yargs.help("h").alias("h", "help");

// Start driver runner if all mandatory parameters are provided
let logger: Logger;
if (yargs.argv) {
    const argv: any = yargs.argv;

    logger = container.get<Logger>(COMMON_TYPES.Logger);
    const configuration = container.get<Configuration.Configuration>(COMMON_TYPES.Configuration);

    let configurationFile: string = argv.config as string;
    if (!path.isAbsolute(configurationFile)) {
        configurationFile = path.resolve(process.cwd(), configurationFile);
    }

    // Prepare logger
    logger.setIdentificationTokens({
        id: argv.id as string,
        applicationName: "Driver<%= $CLI_PARAM_Identifier %>",
        pid: process.pid,
        componentId: argv.componentId || "Driver<%= $CLI_PARAM_Identifier %>",
        entityName: argv.entityName,
        managerId: argv.managerId,
    });

    // Parse and validate configuration
    configuration.setup(configurationFile);
    configuration.registerConfiguration(new ConfigurationSectionDriver(logger));
    configuration.registerConfiguration(new Configuration.ConfigurationSectionSslConfig({ sectionPath: "driver/processCommunication", description: "Process communication with Controller (from driver)", order: 31, validateAsServer: true, dumpSection: "processCommunication"}, logger));
    configuration.registerConfiguration(new Configuration.ConfigurationSectionSslConfig({ sectionPath: "monitor/processCommunication", description: "Process communication with Monitor", order: 32, validateAsServer: false, dumpSection: "monitorProcessCommunication"}, logger));
    configuration.parseAndValidate();

    // Inject extra information of the driver (the sections are assured by the previous calls)
    configuration.data.driver.monitorProcessCommunication = configuration.data.monitor.processCommunication;

    logger.setLogTransports(configuration.data.logging);

    logger.info(`Starting <%= $CLI_PARAM_Identifier %> driver with pid "${process.pid}".`);
    logger.debug(`Command line: ${Utils.objectToString(yargs.argv)}`);
    logger.info(`  ConfigurationFile='${configurationFile}'`);

    // Run Driver Runner (Connection with Monitor, and interface with controller)
    Runner.run({
        id: argv.id as string,
        monitor: {
            reconnectInterval: 1000,
            host: argv.monitorHost as string,
            port: argv.monitorPort as number,
            securityToken: argv.monitorToken as string,
            sslConfig: configuration.data.driver.monitorProcessCommunication,
        },
        device: {
            controller: {
                buffering: 30000,
                serverHost: argv.serverHost as string,
                serverPort: argv.serverPort as number
            },
            sslConfiguration: configuration.data.driver.processCommunication,
        }
    }).then(() => {
        logger.info(`<%= $CLI_PARAM_Identifier %> Driver process started with success`);
    }).catch((error: Error) => {
        logger.error(`<%= $CLI_PARAM_Identifier %> Driver process failed to start!`);
        logger.error(error.message);
        logger.error(error.stack || "<UnknownCallStack>");

        const internalLogger: any = (logger as any)._winston;
        internalLogger.on("finish", () => {
            process.exit(1);
        });
        setTimeout(() => {
            internalLogger.end();
        }, 2000);
    });
}

process.on("uncaughtException", (error: Error) => {
    if (logger) {
        logger.error(`Unexpected error occurred: ${error.message}\r\n${error.stack || ""}`);
        logger.error(`Trying to recover...`);
    }
});

process.on("unhandledRejection", (reason: any, promise: Promise<any>) => {
    if (logger) {
        logger.error(`unhandledRejection: ${reason.message || "Unknown promise rejection occurred"}\r\n${reason.stack || ""}`);
    }
});

