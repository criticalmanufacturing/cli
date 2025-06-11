import { Event as EquipmentEvent, validateConfigurations } from "@criticalmanufacturing/connect-iot-driver";

/** Extended EventProperty data specific for this driver */
export interface EventPropertyExtendedData {
}

/** Default extended data for the Event/Properties of this driver */
export const eventPropertyExtendedDataDefaults: EventPropertyExtendedData = {
};

/** Assign default extended data in the property. The controller already adds the EventProperty extended data on the property itself */
export function validateEventProperties(definition: any, events: EquipmentEvent[]): void {
    for (const event of events) {
        for (const property of event.properties) {
            property.extendedData = Object.assign({}, eventPropertyExtendedDataDefaults, property.extendedData || {});
            validateConfigurations(property.extendedData, definition.criticalManufacturing.automationProtocol.extendedData.eventProperty);
        }
    }
}
