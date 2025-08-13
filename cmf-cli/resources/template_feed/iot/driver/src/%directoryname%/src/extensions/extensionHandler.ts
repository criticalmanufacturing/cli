import { injectable, inject, Container } from "inversify";
import { TYPES as COMMON_TYPES, Communication, Logger } from "@criticalmanufacturing/connect-iot-common";
import { TYPES as DRIVER_TYPES, Command, CommandParameter, Property } from "@criticalmanufacturing/connect-iot-driver";
import { Event as EquipmentEvent } from "@criticalmanufacturing/connect-iot-driver";
import { <%= $CLI_PARAM_Identifier %>DeviceDriver } from "./../driverImplementation";
import { commandExtendedDataDefaults, eventExtendedDataDefaults, propertyExtendedDataDefaults, validateCommandParameters, validateCommands, validateEventProperties, validateEvents } from "../extendedData";

@injectable()
export class ExtensionHandler {
    private _container: Container;
    private _driver: <%= $CLI_PARAM_Identifier %>DeviceDriver;
    private _pJson = require("../../package.json");

    @inject(DRIVER_TYPES.Communication.Controller)
    private _controllerTransport: Communication.Transport;
    @inject(COMMON_TYPES.Logger)
    private _logger: Logger;

    /**
     * Initialize the Transaction Handler. It is assumed this method is called with the controller communication channel already created.
     */
    public async initialize(container: Container): Promise<void> {
        this._container = container;

        this._driver = this._container.get<<%= $CLI_PARAM_Identifier %>DeviceDriver>(DRIVER_TYPES.Device.Driver);

        this._controllerTransport.unsubscribe(this.handleExecuteCommand);
        this._controllerTransport.unsubscribe(this.handleRegisterCustomEvent);
        this._controllerTransport.unsubscribe(this.handleUnregisterCustomEvent);

        this._controllerTransport.subscribe("connect.iot.driver.template.executeCommand", this.handleExecuteCommand.bind(this));
        this._controllerTransport.subscribe("connect.iot.driver.template.registerEvent", this.handleRegisterCustomEvent.bind(this));
        this._controllerTransport.subscribe("connect.iot.driver.template.unregisterEvent", this.handleUnregisterCustomEvent.bind(this));
    }

    /**
     * Handle event occurrences and forward it to controller tasks
     * @param eventName Event registration name
     * @param occurrenceTimeStamp Event timestamp
     * @param values Event values
     */
    public handleEventOccurrence(eventName: string, occurrenceTimeStamp: Date, values: Map<string, any>): void {

        this._logger.info(`Forwarding event '${eventName}' occurrence to controller`);

        const customEventOccurrence: any = {};
        const valuesTreated = [];
        for (const [key, value] of values.entries()) {
            valuesTreated.push({
                propertyName: key,
                originalValue: value,
                value
            });
        }

        customEventOccurrence.eventName = eventName;
        customEventOccurrence.timestamp = occurrenceTimeStamp;
        customEventOccurrence.propertyValues = valuesTreated;

        // Publish the event occurrence to any task listening
        this._controllerTransport.notify({
            type: `connect.iot.driver.template.event.${eventName}`,
            content: customEventOccurrence
        });
    }

    /**
     * Send a command to the device
     * @param msg Message containing the command data
     */
    private handleExecuteCommand = async (msg: Communication.Message<{ command: Command; parameters: any; }> | undefined): Promise<Communication.Message<any>> => {
        if (msg != null) {

            const command: Command = msg.content.command;

            if (command == null) {
                this._logger.error(`No Command was provided!`);
                throw new Error(`No Command was provided!`);
            }

            // Prepare default values
            command.deviceId = command.deviceId != null ? command.deviceId : "";
            command.extendedData = command.extendedData != null ? command.extendedData : {};
            command.extendedData = Object.assign({}, commandExtendedDataDefaults, command.extendedData || {});
            command.parameters = command.parameters != null ? command.parameters : [];
            command.parameters.forEach((parameter: CommandParameter) => {
                parameter.extendedData = parameter.extendedData != null ? parameter.extendedData : {};
            });

            // Validate the event structure
            validateCommands(this._pJson, [command]);
            validateCommandParameters(this._pJson, [command]);

            this._logger.info(`Sending command '${command.deviceId}' to device. Command was sent by a controller task.`);
            const parameters: Map<CommandParameter, any> = new Map<CommandParameter, any>();
            if (msg.content.parameters) {
                for (const p in msg.content.parameters) {
                    if (msg.content.parameters.hasOwnProperty(p)) {
                        if (p !== "$id") {
                            const param = command.parameters.find((cp: CommandParameter) => cp.name === p);
                            if (param != null) {
                                parameters.set(param, msg.content.parameters[p]);
                            } else {
                                this._logger.error(`Parameter '${p}' is not known for the command!`);
                                throw new Error(`Parameter '${p}' is not known for the command!`);
                            }
                        }
                    }
                }
            }

            return {
                type: Communication.MESSAGES.REPLY,
                content: await this._driver.execute(command, parameters),
            };
        } else {
            throw new Error("Invalid command received!");
        }
    };

    /**
     * Register a handler for a specific message type. Only one handler is allowed, so the last one will be valid.
     * @param msg Message containing the handler data
     */
    private handleRegisterCustomEvent = (msg: Communication.Message<EquipmentEvent> | undefined): void => {
        if (msg) {
            const event: EquipmentEvent = msg.content;
            if (event == null) {
                this._logger.error("Unable to register null event data");
                throw new Error("Unable to register null event data");
            }

            event.properties = event.properties != null ? event.properties : [];
            event.isEnabled = event.isEnabled != null ? event.isEnabled : true;
            event.extendedData = Object.assign({}, eventExtendedDataDefaults, event.extendedData || {});
            event.properties.forEach((property: Property) => {
                property.systemId = property.systemId != null ? property.systemId : "-1";
                property.dataType = property.dataType != null ? property.dataType : "String";
                property.deviceType = property.deviceType != null ? property.deviceType : "String";
                property.deviceId = property.deviceId != null ? property.deviceId : "";
                property.extendedData = Object.assign({}, propertyExtendedDataDefaults, property.extendedData || {});
            });

            // Validate the event structure
            validateEvents(this._pJson, [event]);
            validateEventProperties(this._pJson, [event]);

            this._driver.registerCustomEvent(event);
        }
    };

    /**
     * Register a handler for a specific message type. Only one handler is allowed, so the last one will be valid.
     * @param msg Message containing the handler data
     */
    private handleUnregisterCustomEvent = (msg: Communication.Message<{ name: string; }> | undefined): void => {
        if (msg && msg.content) {
            this._driver.unregisterCustomEvent(msg.content.name);
        }
    };
}