import { Command, validateConfigurations } from "@criticalmanufacturing/connect-iot-driver";

/** Extended Command data specific for this driver */
export interface CommandExtendedData {
}

/** Default extended data for the commands of this driver */
export const commandExtendedDataDefaults: CommandExtendedData = {
};

/** Assign extended data in the commands, based on the defaults and defined values */
export function validateCommands(definition: any, commands: Command[]): void {
    for (const command of commands) {
        command.extendedData = Object.assign({}, commandExtendedDataDefaults, command.extendedData || {});
        validateConfigurations(command.extendedData, definition.criticalManufacturing.automationProtocol.extendedData.command);
    }
}
