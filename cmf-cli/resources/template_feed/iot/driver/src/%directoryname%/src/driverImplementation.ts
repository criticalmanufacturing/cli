import { injectable, inject, Container } from "inversify";
import { CommunicationState, PropertyValuePair } from "@criticalmanufacturing/connect-iot-driver";
import { Property, EventOccurrence, PropertyValue, Command, Event as EquipmentEvent, DeviceDriverBase, CommandParameter } from "@criticalmanufacturing/connect-iot-driver";
import { <%= $CLI_PARAM_Identifier %>CommunicationSettings, <%= $CLI_PARAM_IdentifierCamel %>DefaultCommunicationSettings, validateCommunicationParameters } from "./communicationSettings";
import { validateProperties, validateEvents, validateEventProperties, validateCommands, validateCommandParameters } from "./extendedData/index";
import { Utils } from "@criticalmanufacturing/connect-iot-common";
import { TYPES } from "./types";

@injectable()
export class <%= $CLI_PARAM_Identifier %>DeviceDriver extends DeviceDriverBase {
    private _communicationSettings: <%= $CLI_PARAM_Identifier %>CommunicationSettings;
    private _container: Container;

    @inject(TYPES.Injector)
    private _parentContainer: Container;

    public constructor() {
        super();

        // Should redirect Execute Command to Set Property Values instead?
        //#if hasCommands
        this.__useSetValuesInsteadOfExecute = false;
        //#else
        this.__useSetValuesInsteadOfExecute = true;
        //#endif
    }

    /**
     * Initialize the driver. Used to prepare all components from containers, register events, etc.
     * Note: Called by the driverBase
     */
    public async initializeDriver(): Promise<void> {
        if (!this._container) {
            this._container = new Container();
            this._container.parent = this._parentContainer;
        }

        // Initialize the specific driver
        // ...
    }

    /**
     * Notification regarding the communication parameters being available.
	 * Validate the integrity of the values
     * Note: Called by the driverBase
     * @param communication Communication settings object
     */
    public async setCommunicationConfiguration(communication: any): Promise<void> {
        this._communicationSettings = Object.assign({}, <%= $CLI_PARAM_IdentifierCamel %>DefaultCommunicationSettings, communication);

        // eslint-disable-next-line
        const pJson = require("../package.json");
        validateCommunicationParameters(pJson, this._communicationSettings);

        // Prepare the extended data
        validateProperties(pJson, this.configuration.properties);
        validateEvents(pJson, this.configuration.events);
        validateEventProperties(pJson, this.configuration.events);
        validateCommands(pJson, this.configuration.commands);
        validateCommandParameters(pJson, this.configuration.commands);
    }

    /**
     * Connect to the equipment.
     * Note: Called by the driverBase
     */
    public async connectToDevice(): Promise<void> {
        this.setCommunicationState(CommunicationState.Connecting);

        try {
            // Connect to the equipment
            // ...

            // Provide an empty object as a fallback to avoid errors
            const { $id, ...cleanCommunicationSettings } = this._communicationSettings as any || {};
            this.logger.info(`Using the following configurations: ${JSON.stringify(cleanCommunicationSettings, undefined, " ")}`);

            // Notify the communication was a success and it is now ready for the setup process
            this.setCommunicationState(CommunicationState.Setup);
        } catch (error) {
            this.logger.error(`Failed to connect to device: ${error.message}`);
            this.setCommunicationState(CommunicationState.ConnectingFailed);
        }
    }

    /**
     * Disconnect the communication with the equipment
     * Note: Called by the driverBase
     */
    public async disconnectFromDevice(): Promise<void> {
        this.setCommunicationState(CommunicationState.Disconnecting);

        try {
            // Disconnect to the equipment
            // ...
        } catch (error) {
            this.logger.error(`Failed to disconnect from device: ${error.message}`);
        }

        this.setCommunicationState(CommunicationState.Disconnected);
    }

    /**
     * Notification that the setup process was a success
     * Note: Called by the driverBase
     */
    public async setupCompleted(): Promise<void> {
        // Since the setup was a success, set the state to Communicating
        await this.setCommunicationState(CommunicationState.Communicating);
    }

    /**
     * Request the equipment for values of the properties
     * Note: Called by the driverBase
     * @param properties List of properties to get values
     */
    public async getValues(properties: Property[]): Promise<PropertyValue[]> {
        const results: PropertyValue[] = [];

        // Request the equipment for values
        // ...
        // Foreach result:
        /*
			let propertyValue: PropertyValue = {
				propertyName: property.name,
				originalValue: value,
				value: this.convertValueFromDevice(value, property.deviceType, property.dataType),
			};
			results.push(propertyValue);
		*/

        return(results);
    }

    /**
     * Set the value of properties in the equipment.
     * Note: Called by the driverBase
     * @param propertiesAndValues List of properties and new values
     */
    public async setValues(propertiesAndValues: PropertyValuePair[]): Promise<boolean> {
        // Request the equipment to define new values
        // ...

        return(true);
    }

    /**
     * Send a command to the equipment. Depending on some settings, different messages can be sent.
     * Note: Called by the driverBase
     * @param command Command to send
     * @param parameters List of parameters to use
     */
    public async execute(command: Command, parameters: Map<CommandParameter, any>): Promise < any > {
    //#if hasCommands
        // Execute the command in the equipment
        // ...

        return(true); // Or the command result
    //#else
        // Implement as SetValues!
        throw new Error("Execute should not be called directly");
    //#endif
    }

    /**
     * Handle the communication state changes
     * Note: Called by the driverBase
     * @param previousState Previous state
     * @param newState New state
     */
    public async notifyCommunicationStateChanged(previousState: CommunicationState, newState: CommunicationState): Promise<void> {
        // Add any specific handling here
    }

    /**
     * Handle the driver event notification. Trigger it to the controller if the trigger property was changed
	 * Note: This is just as an example... This code is not being called anywhere
     * @param eventId Id of the event (systemId)
     * @param values List of values of the event registered
     */
    private async onEventOccurrence(eventId: string, values: Map<string, any>): Promise<void> {
        const event = this.configuration.events.find(e => e.systemId === eventId);
        if (event && event.isEnabled) {
            const results: PropertyValue[] = [];

            // Fill results and check if the trigger properties have been the cause of the event occurrence
            if (values) {
                for (const eventProperty of event.properties) {
                    if (values.has(eventProperty.deviceId)) {
                        const value: any = values.get(eventProperty.deviceId);

                        const propertyValue: PropertyValue = {
                            propertyName: eventProperty.name,
                            originalValue: value,
                            value: this.convertValueFromDevice(value, eventProperty.deviceType, eventProperty.dataType),
                        };

                        results.push(propertyValue);
                    } else {
                        throw new Error(`Value for property '${eventProperty.name}' was not received in the event data`);
                    }
                }
            }

            // Raise event to controller
            const occurrence: EventOccurrence = {
                timestamp: new Date(),
                eventDeviceId: event.deviceId,
                eventName: event.name,
                eventSystemId: event.systemId,
                propertyValues: results
            };

            this.emit("eventOccurrence", occurrence);
        }
    }

    /**
     * Convert value received from device to system
     * Note: No conversion is being done at the moment!
     * @param raw value
     * @param fromType original value type (device)
     * @param toType destination value type (system)
     */
    private convertValueFromDevice(raw: any, fromType: string, toType: string): any {
        if (raw == null) {
            return (undefined);
        }

        // Convert the value (this is an example)
        raw = Utils.convertValueToType(raw, toType);

        // return same thing (could not convert it?)
        return(raw);
    }

    /**
     * Convert value received from system to device.
     * Note: No conversion is being done at the moment!
     * @param raw raw value
     * @param fromType original value type (system)
     * @param toType destination value type (device)
     */
    private convertValueToDevice(raw: any, fromType: string, toType: string): any {
        if (raw == null) {
            return (undefined);
        }

        // Convert the value
        // ...

        // return same thing (could not convert it?)
        return(raw);
    }
}
