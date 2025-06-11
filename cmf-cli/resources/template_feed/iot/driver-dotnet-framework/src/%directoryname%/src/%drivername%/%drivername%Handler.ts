import { injectable, Container, inject } from "inversify";
import * as EventEmitter from "events";
import { Logger, TYPES as COMMON_TYPES, Utils } from "@criticalmanufacturing/connect-iot-common";
import * as path from "path";
import * as fs from "fs-extra";
import { <%= $CLI_PARAM_Identifier %>CommunicationSettings } from "../communicationSettings";
import { Configuration, findPackageRootDirectory, TYPES as DRIVER_TYPES } from "@criticalmanufacturing/connect-iot-driver";
import { validateEventProperties, validateEvents } from "../extendedData";

@injectable()
export class <%= $CLI_PARAM_Identifier %>Handler extends EventEmitter {
    protected _configuration: Configuration;

    private _container: Container;
    private assemblyName: string = path.join(__dirname, "../../../lib/Cmf.Connect.IoT.Driver.<%= $CLI_PARAM_Identifier %>.dll");
    private typeName: string = "Cmf.Connect.IoT.Driver.<%= $CLI_PARAM_Identifier %>.Startup";
    private _pJson: any = {};
    private _pendingEventHandlers: string[] = [];

    private edge: any; // Keep it like this due to PackagePacker dependency processing
    private _driver_registerEventHandler: any;
    private _driver_connect: any;
    private _driver_disconnect: any;
    private _driver_getValues: any;
    private _driver_setValues: any;
    private _driver_executeCommand: any;
    private _driver_registerEvent: any;
    private _driver_unregisterEvent: any;

    private _logger: Logger;
    private _communicationSettings: <%= $CLI_PARAM_Identifier %>CommunicationSettings;

    @inject(DRIVER_TYPES.DriverId)
    protected _driverId: string;

    // Parameter is required to fix error The number of constructor arguments in the derived class <%= $CLI_PARAM_Identifier %>Handler must be >= than the number of constructor arguments of its base class.
    public constructor(@inject("dummy") private dummy: any) {
        super();
    }

    /**
     * Initialize the <%= $CLI_PARAM_Identifier %>Handler. Initialized at the same time as the driver implementation
     */
    public async initialize(container: Container): Promise<void> {
        this._container = container;
        this._logger = this._container.get<Logger>(COMMON_TYPES.Logger);
    }

    /**
     * Initialize Edge. Initialized at the same time as the driver implementation
     */
    public async initializeEdge(): Promise<void> {

        // Make sure .net core is NOT being used
        // https://github.com/agracio/electron-edge-js/issues/9
        if(process.env["EDGE_USE_CORECLR"]) {
            delete process.env["EDGE_USE_CORECLR"];
        }

        this.edge = require("edge-js");
        await this.registerDotNetAssemblyHandlers();
    }

    /**
     * Connect to the equipment using the .net driver assembly
     * @param communicationSettings Communication settings to use
     */
    public async connect(communicationSettings: <%= $CLI_PARAM_Identifier %>CommunicationSettings): Promise<void> {
        this._communicationSettings = communicationSettings;

        return (new Promise<void>((resolve, reject) => {
            this._driver_connect(communicationSettings, async (error: any, result: any) => {
                if (error) {
                    reject(error);
                } else {
                    if (result === false) {
                        reject(new Error("Unable to connect"));
                    } else {
                        resolve();
                    }
                }
            });
        }));
    }

    /**
     * Disconnect from the equipment using the .net driver assembly
     */
    public async disconnect(): Promise<void> {
        return (new Promise<void>((resolve, reject) => {
            this._driver_disconnect(undefined, async (error: any, result: any) => {
                if (error) {
                    reject(error);
                } else {
                    if (result === false) {
                        reject(new Error("Unable to disconnect"));
                    } else {
                        resolve();
                    }
                }
            });
        }));
    }

    /**
     * Register an event handler in driver (using promises)
     * @param eventName Name of event
     * @param callback Callback to handle the event
     */
    // eslint-disable-next-line
    private async registerEventHandler(eventName: string, callback: Function): Promise<void> {
        return (new Promise<void>((resolve, reject) => {
            this._driver_registerEventHandler({ eventName: eventName, callback: callback }, async (error: any, result: any) => {
                if (error) {
                    this._logger.error(`Unable to register event "${eventName}": ${error.message}`);
                    resolve(); // Never fail ?
                } else {
                    resolve();
                }
            });
        }));
    }

    /**
     * Helper function (using promises) to register custom event notification from device
     * @param eventId Event to register in .net driver
     */
    public async registerEvent(eventId: string): Promise<boolean> {
        return new Promise<boolean>((resolve, reject) => {
            if (this._driver_registerEvent == null) {
                this._logger.warning(`<%= $CLI_PARAM_Identifier %> handler is not yet initialized, so the event '${eventId}' will be registered afterwards`);
                this._pendingEventHandlers.push(eventId);
                resolve(true);
            }
            else {
                try {
                    this._driver_registerEvent({ eventId: eventId, callback: this.onEventOccurrence.bind(this) }, async (error: any, result: any) => {
                        if (error) {
                            reject(error);
                        } else {
                            if (result === false) {
                                reject(new Error(`Unable to register event "${eventId}": ${error.message}`));
                            } else {
                                resolve(true);
                            }
                        }
                    });
                } catch (error) {
                    reject(error);
                }
            }
        });
    }

    /**
     * Helper function (using promises) to unregister event notification from device
     * @param eventId Event to unregister in .net driver
     */
    public async unregisterEvent(eventId: string): Promise<boolean> {
        return new Promise<boolean>((resolve, reject) => {
            try {
                this._driver_unregisterEvent({ eventId: eventId }, async (error: any, result: any) => {
                    if (error) {
                        reject(error);
                    } else {
                        if (result === false) {
                            reject(new Error(`Unable to unregister event "${eventId}": ${error.message}`));
                        } else {
                            resolve(true);
                        }
                    }
                });
            } catch (error) {
                reject(error);
            }
        });
    }

    /**
     * Helper function (using promises) to set execute a command in device
     * @param commandId Name of the command to execute
     * @param parameters List of parameters and values to pass to command
     */
    public async executeCommand(commandId: string, parameters: Map<string, any>): Promise<any> {
        //#if hasCommands
        // Execute the command in the equipment

        // Convert the parameters into a more easy way to be processed in C# side
        const parametersAsObject: any[] = [];
        let values: string = "";
        for(const [key, value] of parameters) {
            parametersAsObject.push({ name: key, value: value });

            values += (`\n   '${key}'='${Utils.objectToString(value)}'`);
        }
        this._logger.debug(`>> Command '${commandId}' ${values}`);

        return new Promise<any>((resolve, reject) => {
            try {
                this._driver_executeCommand({ commandId: commandId, parameters: parametersAsObject }, async (error: any, result: any) => {
                    if (error) {
                        reject(error);
                    } else {
                        this._logger.debug(`# Successfully executed command '${commandId}'`);

                        resolve(result);
                    }
                });
            } catch (error) {
                reject(error);
            }
        });
        //#else
        // Implement as SetValues!
        throw new Error("Execute should not be called directly");
        //#endif
    }

    /**
     * Function that validates the existence of the global.json and manages its value according to the netcore SDK
     */
    private manageFileDotNetVersion(netCoreSdkVersion?: string): void {

        const root: string = findPackageRootDirectory(__dirname);
        const fileName: string = path.join(root, "global.json");

        if (netCoreSdkVersion != null && netCoreSdkVersion !== "") {
            // Use version 0.0.0 which is invalid as a token to identify test run
            // this is necessary to make sure the file present in the source code tree
            // is not delered after the test run. This file is necessary to be able to succcessfuly compile
            // the edge binaries (incompatible with .net core >= 7)
            if (netCoreSdkVersion !== "0.0.0") {
                this._logger.debug(`Creating/editing global.json to use netcore SDK version ${netCoreSdkVersion}`);
                fs.writeJSONSync(fileName, {
                    "sdk": {
                        "version": netCoreSdkVersion,
                    }
                }, {
                    spaces: 2,
                    mode: 0o777,
                });
            }
        } else {
            if (fs.existsSync(fileName)) {
                this._logger.debug(`netcore SDK version not set. Removing global.json`);
                fs.removeSync(fileName);
            }
        }
    }

    protected get driverInstanceId() {
        return this._driverId.replace("/", "_");
    }

    /**
     * Register the event handlers and method calls on the .net core assembly secs gem driver
     */
    public async registerDotNetAssemblyHandlers(): Promise<void> {
        const assemblyPath = path.resolve(this.assemblyName);
        this._logger.info(`Using assembly from '${assemblyPath}'`);
        const createHandler = this.edge.func({
            methodName: "CreateHandler",
            assemblyFile: assemblyPath,
            typeName: this.typeName
        });
        const driverHandler = createHandler(null, true);
        this._logger.info(`Registering DotNetFramework assembly functions with '${assemblyPath}'`);

        // Methods
        this._driver_registerEventHandler = driverHandler.RegisterEventHandler;
        this._driver_connect = driverHandler.Connect;
        this._driver_disconnect = driverHandler.Disconnect;
        this._driver_getValues = driverHandler.GetValues;
        this._driver_setValues = driverHandler.SetValues;
        this._driver_executeCommand = driverHandler.ExecuteCommand;
        this._driver_registerEvent = driverHandler.RegisterEvent;
        this._driver_unregisterEvent = driverHandler.UnregisterEvent;

        // Register events
        await this.registerEventHandler("Log", this.onLog.bind(this));
        await this.registerEventHandler("Disconnect", this.onDisconnect.bind(this));
        await this.registerEventHandler("Connect", this.onConnect.bind(this));

        // Register the controller events
        for (const eventType of this._pendingEventHandlers) {
            this._logger.info(`Registering controller handler for events of type '${eventType}'`);
            await this.registerEvent(eventType);
        }
        this._pendingEventHandlers = [];
    }

    /**
     * To improve performance, use the json file read by the main driver
     * @param pJson Json content
     */
    public setPackageJson(pJson: any): void {
        this._pJson = pJson;
    }

    public async setConfiguration(configuration: Configuration, communication: <%= $CLI_PARAM_Identifier %>CommunicationSettings): Promise<void> {
        this._configuration = configuration;
        this._communicationSettings = communication;
        this.manageFileDotNetVersion(communication.netCoreSdkVersion);
    }

    /**
     * Set the Driver Definitions events.
     * @param driverDefinitions Entire driver definitions
     */
    public setDriverDefinitionsEvents(driverDefinitions: Configuration) {
        // Setup events
        if (driverDefinitions && driverDefinitions.events && driverDefinitions.events.length > 0) {
            for (const equipmentEvent of driverDefinitions.events) {
                if (equipmentEvent.isEnabled) {

                    equipmentEvent.properties = equipmentEvent.properties != null ? equipmentEvent.properties : [];
                    equipmentEvent.isEnabled = equipmentEvent.isEnabled != null ? equipmentEvent.isEnabled : true;
                    equipmentEvent.extendedData = equipmentEvent.extendedData != null ? equipmentEvent.extendedData : {};
                    equipmentEvent.properties.forEach((property: any) => {
                        property.systemId = property.systemId != null ? property.systemId : "-1";
                        property.dataType = property.dataType != null ? property.dataType : "String";
                        property.deviceType = property.deviceType != null ? property.deviceType : "String";
                        property.deviceId = property.deviceId != null ? property.deviceId : "";
                        property.extendedData = property.extendedData != null ? property.extendedData : {};
                    });

                    // Validate the event structure
                    validateEvents(this._pJson, [equipmentEvent]);
                    validateEventProperties(this._pJson, [equipmentEvent]);

                    try {
                        this._logger.info(`Registering event '${equipmentEvent.name}' (${equipmentEvent.systemId}) on <%= $CLI_PARAM_Identifier %>`);
                        this.registerEvent(equipmentEvent.deviceId);
                    } catch (error) {
                        this._logger.error(`Unable to register event '${equipmentEvent.name}': ${error}`);
                    }
                }
            }
        }
    }

    /**
     * Event Handler when device is connected
     * @param data NA
     * @param callback NA
     */
    private async onConnect(data: any, callback: any): Promise<void> {
        this.emit("connected");

        if (callback) {
            callback();
        }
    }

    /**
     * Event Handler when device is disconnected
     * @param data NA
     * @param callback NA
     */
    private async onDisconnect(data: any, callback: any): Promise<void> {
        this.emit("disconnected");

        if (callback) {
            callback();
        }
    }

    /**
     * Handler for the event occurrence on c# side of the driver
     * @param occurrence event data
     */
    private async onEventOccurrence(occurrence: any, callback: any): Promise<void> {

        if (typeof occurrence.values === "string") {
            occurrence.values = JSON.parse(occurrence.values);
        }

        // Log occurrence
        let values: string = "";
        if (occurrence.values) {
            values += (`\n   '${Utils.objectToString(occurrence.values)}'`);
        }
        this._logger.debug(`<< Event [${occurrence.messageId}] Type: '${occurrence.eventId}', RequestId: '${occurrence.requestId}' ${values}`);

        // Emit event to the Driver's event listener
        this.emit("on<%= $CLI_PARAM_Identifier %>EventOccurrence", occurrence);

        if (callback) {
            callback();
        }
    }

    /**
     * Event Handler when there's a message to log
     * @param data Data to log
     * @param callback NA
     */
    private async onLog(data: { verbosity: string; message: string; }, callback: any): Promise<void> {
        switch ((data.verbosity || "").toLowerCase()) {
            case "debug": this._logger.debug(data.message); break;
            case "info": this._logger.info(data.message); break;
            case "warning": this._logger.warning(data.message); break;
            case "error": this._logger.error(data.message); break;
            case "notice": this._logger.notice(data.message); break;
            case "crit": this._logger.crit(data.message); break;
            case "alert": this._logger.alert(data.message); break;
            case "emerg": this._logger.emerg(data.message); break;
            default: this._logger.error("Unknown log entry", data);
        }

        if (callback) {
            callback();
        }
    }
}
