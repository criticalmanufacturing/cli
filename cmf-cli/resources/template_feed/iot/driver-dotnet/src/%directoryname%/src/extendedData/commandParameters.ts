import { Command, validateConfigurations, getConfigurationNode } from "@criticalmanufacturing/connect-iot-driver";

/** Command Parameter Extended Data allows ASCII conversion selection and definition of Command Parameter Type */
export interface CommandParameterExtendedData {
}

/** Default extended data for the Command Parameter of this driver */
export const commandParameterExtendedDataDefaults: CommandParameterExtendedData = {
};

/**  Assign default extended data in the command parameter, based on the defaults and defined values */
export function validateCommandParameters(definition: any, commands: Command[]): void {
    const parameterTypeNode = getConfigurationNode(definition.criticalManufacturing.automationProtocol.extendedData.commandParameter, "parameterType");

    for (const command of commands) {
        if ( command.parameters.length > 0 ) {
            for (const parameter of command.parameters) {
                parameter.extendedData = Object.assign({}, commandParameterExtendedDataDefaults, parameter.extendedData || {}) ;
                validateConfigurations(parameter.extendedData, definition.criticalManufacturing.automationProtocol.extendedData.commandParameter);
            }
        }
    }
}
