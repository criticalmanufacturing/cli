import { Property, validateConfigurations } from "@criticalmanufacturing/connect-iot-driver";

/** Extended Property data specific for this driver */
export interface PropertyExtendedData {
}

/** Default extended data for the properties of this driver */
export const propertyExtendedDataDefaults: PropertyExtendedData = {
};

/** Assign extended data in the properties, based on the defaults and defined values */
export function validateProperties(definition: any, properties: Property[]): void {
    for (const property of properties) {
        property.extendedData = Object.assign({}, propertyExtendedDataDefaults, property.extendedData || {});
        validateConfigurations(property.extendedData, definition.criticalManufacturing.automationProtocol.extendedData.property);
    }
}
