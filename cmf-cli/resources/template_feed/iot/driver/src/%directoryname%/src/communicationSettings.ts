import { validateConfigurations } from "@criticalmanufacturing/connect-iot-driver";

/** Driver Communication Settings. Will be available in the Equipment Setup Task */
export interface <%= $CLI_PARAM_Identifier %>CommunicationSettings {
    // Add driver specific settings here

    // Common/driver WS settings
    heartbeatInterval: number;
    setupTimeout: number;
    intervalBeforeReconnect: number;
    connectingTimeout: number;
}

/** Default Communication Settings */
export const <%= $CLI_PARAM_IdentifierCamel %>DefaultCommunicationSettings: <%= $CLI_PARAM_Identifier %>CommunicationSettings = {
    // Add driver specific default settings here

    // Common/driver WS settings
    heartbeatInterval: 30000,
    setupTimeout: 10000,
    intervalBeforeReconnect: 5000,
    connectingTimeout: 30000,
};

/** Validate communication parameters enum values */
export function validateCommunicationParameters(definition: any, configs: any): void {
    validateConfigurations(configs, definition.criticalManufacturing.automationProtocol.parameters);
}
