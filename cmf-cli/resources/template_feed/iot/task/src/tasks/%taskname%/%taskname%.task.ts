import { Task, System, TaskBase } from "@criticalmanufacturing/connect-iot-controller-engine";

/** Default values for settings */
export const SETTINGS_DEFAULTS: <%= $CLI_PARAM_ClassName %>Settings = {
<%= $CLI_PARAM_SettingsDefaults %>
};

/**
 * @whatItDoes
 *
 * This task does something ... describe here
 *
 * @howToUse
 *
 * yada yada yada
 *
 * ### Inputs
 * * `any` : **activate** - Activate the task
 *
 * ### Outputs
 *
 * * `bool`  : ** success ** - Triggered when the the task is executed with success
 * * `Error` : ** error ** - Triggered when the task failed for some reason
 *
 * ### Settings
 * See {@see <%= $CLI_PARAM_ClassName %>Settings}
 */
@Task.Task()
export class <%= $CLI_PARAM_ClassName %>Task extends TaskBase implements <%= $CLI_PARAM_ClassName %>Settings {

    /** Accessor helper for untyped properties and output emitters. */
    // [key: string]: any;

    /** **Inputs** */
<%= $CLI_PARAM_InputsInterface %>

    /** **Outputs** */
<%= $CLI_PARAM_OutputsInterface %>

    /** Properties Settings */
<%= $CLI_PARAM_SettingsInterface %>

    /**
     * When one or more input values is changed this will be triggered,
     * @param changes Task changes
     */
    public override async onChanges(changes: Task.Changes): Promise<void> {
        if (changes["activate"]) {
            // It is advised to reset the activate to allow being reactivated without the value being different
            this.activate = undefined;

            // ... code here
            this.success.emit(true);

            // or
            this._logger.error(`Something very wrong just happened! Log it!`);
            this.error.emit(new Error ("Will stop processing, but Error output will be triggered with this value"));
        }
    }

    /** Right after settings are loaded, create the needed dynamic outputs. */
    public override async onBeforeInit(): Promise<void> {
    }

    /** Initialize this task, register any event handler, etc */
    public override async onInit(): Promise<void> {
        this.sanitizeSettings(SETTINGS_DEFAULTS);
    }

    /** Cleanup internal data, unregister any event handler, etc */
    public override async onDestroy(): Promise<void> {
    }
}

// Add settings here
/** <%= $CLI_PARAM_ClassName %> Settings object */
export interface <%= $CLI_PARAM_ClassName %>Settings extends System.TaskDefaultSettings {
<%= $CLI_PARAM_SettingsInterface %>
}
