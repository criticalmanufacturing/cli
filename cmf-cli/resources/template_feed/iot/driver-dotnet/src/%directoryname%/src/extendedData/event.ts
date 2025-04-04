import { Event as EquipmentEvent, validateConfigurations } from "@criticalmanufacturing/connect-iot-driver";

/** Extended Event data specific for this driver */
export interface EventExtendedData {
}

/** Default extended data for the events of this driver */
export const eventExtendedDataDefaults: EventExtendedData = {
};

/** Assign extended data in the events, based on the defaults and defined values */
export function validateEvents(definition: any, events: EquipmentEvent[]): void {
    for (const event of events) {
        event.extendedData = Object.assign({}, eventExtendedDataDefaults, event.extendedData || {});
        validateConfigurations(event.extendedData, definition.criticalManufacturing.automationProtocol.extendedData.event);
    }
}
