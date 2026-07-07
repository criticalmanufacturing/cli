//#if (useLegacyTaskTemplate)
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


//#elseif (isTaskBase)
import { Task, TaskBase } from "@criticalmanufacturing/connect-iot-controller-engine";
import type { System } from "@criticalmanufacturing/connect-iot-controller-engine";

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
     * When the task is activated, this method is called with the changes and the activation value.
     * @param changes Task changes
     * @param activatedValue Activation value
     */
    protected override async onActivate(changes: Task.Changes, activatedValue: any): Promise<void> {
        // ... code here
        this.success.emit(true);

        // or
        this._logger.error(`Something very wrong just happened! Log it!`);
        this.error.emit(new Error("Will stop processing, but Error output will be triggered with this value"));
    }
}

// Add settings here
/** <%= $CLI_PARAM_ClassName %> Settings object */
export interface <%= $CLI_PARAM_ClassName %>Settings extends System.TaskDefaultSettings {
<%= $CLI_PARAM_SettingsInterface %>
}


//#elseif (isAutoActivatedTaskBase)
import { Task, AutoActivatedTaskBase } from "@criticalmanufacturing/connect-iot-controller-engine";
import type { System } from "@criticalmanufacturing/connect-iot-controller-engine";

/** Default values for settings */
export const SETTINGS_DEFAULTS: <%= $CLI_PARAM_ClassName %>Settings = {
    autoActivate: true,
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
export class <%= $CLI_PARAM_ClassName %>Task extends AutoActivatedTaskBase implements <%= $CLI_PARAM_ClassName %>Settings {

    /** Accessor helper for untyped properties and output emitters. */
    // [key: string]: any;

    /** **Inputs** */
<%= $CLI_PARAM_InputsInterface %>

    /** **Outputs** */
<%= $CLI_PARAM_OutputsInterface %>

    /** Properties Settings */
    public autoActivate: boolean = SETTINGS_DEFAULTS.autoActivate;
<%= $CLI_PARAM_SettingsInterface %>

    public constructor() {
        super({
            activate: () => this.activateListenerCallback(),
            deactivate: () => this.deactivateListenerCallback(),
            getAutoActivateValue: () => this.autoActivate,
        });
    }

    private async activateListenerCallback(): Promise<void> {
        if (this._isActivated === true) {
            this._logger.info(`The listener is already active. Ignoring action`);
        } else {
            // ... code here
            this.success.emit(true);
        }
    }

    private async deactivateListenerCallback(): Promise<void> {
        if (this._isActivated === true) {
            // ... code here
        }
    }

    /** Applies default settings before the task is initialized. */
    public override async onBeforeInit(): Promise<void> {
        await super.onBeforeInit();
        this.sanitizeSettings(SETTINGS_DEFAULTS);
    }

}

// Add settings here
/** <%= $CLI_PARAM_ClassName %> Settings object */
export interface <%= $CLI_PARAM_ClassName %>Settings extends System.TaskDefaultSettings {
    autoActivate: boolean;
<%= $CLI_PARAM_SettingsInterface %>
}


//#elseif (isDriverTriggeredTaskBase)
import { Task, DriverTriggeredTaskBase } from "@criticalmanufacturing/connect-iot-controller-engine";
import type { System } from "@criticalmanufacturing/connect-iot-controller-engine";

/** Default values for settings */
export const SETTINGS_DEFAULTS: <%= $CLI_PARAM_ClassName %>Settings = {
    autoActivate: true,
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
export class <%= $CLI_PARAM_ClassName %>Task extends DriverTriggeredTaskBase implements <%= $CLI_PARAM_ClassName %>Settings {

    /** Accessor helper for untyped properties and output emitters. */
    // [key: string]: any;

    /** **Inputs** */
<%= $CLI_PARAM_InputsInterface %>

    /** **Outputs** */
<%= $CLI_PARAM_OutputsInterface %>

    /** Properties Settings */
    public autoActivate: boolean = SETTINGS_DEFAULTS.autoActivate;
<%= $CLI_PARAM_SettingsInterface %>

    public constructor() {
        super({
            activate: async () => this.subscribeHandler(),
            deactivate: async () => this.unsubscribeHandler(),
            getAutoActivateValue: () => this.autoActivate
        });
    }

    /** Handler to emit the message content */
    private onMessageReceivedHandler: any = (message: any): void => {

    };

    /** Subscribes to the configured raw driver message channel. */
    private async subscribeHandler(): Promise<void> {

    }

    /** Removes the raw driver message subscription. */
    private async unsubscribeHandler(): Promise<void> {

    }

    /** Applies default settings before the task is initialized. */
    public override async onBeforeInit(): Promise<void> {
        await super.onBeforeInit();
        this.sanitizeSettings(SETTINGS_DEFAULTS);
    }

}

// Add settings here
/** <%= $CLI_PARAM_ClassName %> Settings object */
export interface <%= $CLI_PARAM_ClassName %>Settings extends System.TaskDefaultSettings {
    autoActivate: boolean;
<%= $CLI_PARAM_SettingsInterface %>
}


//#elseif (isRequestReplyAnswerTaskBase)
import { Task, RequestReplyAnswerTaskBase } from "@criticalmanufacturing/connect-iot-controller-engine";
import type { System } from "@criticalmanufacturing/connect-iot-controller-engine";

/** Default values for settings */
export const SETTINGS_DEFAULTS: <%= $CLI_PARAM_ClassName %>Settings = {
    defaultReply: {},
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
 * ### Settings
 * See {@see <%= $CLI_PARAM_ClassName %>Settings}
 */
@Task.Task()
export class <%= $CLI_PARAM_ClassName %>Task extends RequestReplyAnswerTaskBase implements <%= $CLI_PARAM_ClassName %>Settings {

    /** Accessor helper for untyped properties and output emitters. */
    // [key: string]: any;

    /** **Inputs** */
<%= $CLI_PARAM_InputsInterface %>
    public reply: any = undefined;

    /** **Outputs** */
<%= $CLI_PARAM_OutputsInterface %>

    /** Properties Settings */
    /** The default reply to send if the reply is undefined */
    public defaultReply: any;
<%= $CLI_PARAM_SettingsInterface %>

    /**
     * Sends the prepared message bus reply when the task is activated.
     * @param changes Changes that triggered the activation.
     * @param activatedValue Activation value received by the task.
     */
    protected override async onActivate(changes: Task.Changes, activatedValue: any): Promise<void> {
        let replyObject = this.reply ?? this.defaultReply;
        this.reply = undefined;

        const normalizedReply = this.normalizeReplyMessage(replyObject);

        if (await this.sendReplyMessage(normalizedReply)) {
            this.success.emit(true);
        }
    }

}

// Add settings here
/** <%= $CLI_PARAM_ClassName %> Settings object */
export interface <%= $CLI_PARAM_ClassName %>Settings extends System.TaskDefaultSettings {
    /** The default reply to send if the reply is undefined */
    defaultReply: any;
<%= $CLI_PARAM_SettingsInterface %>
}

//#elseif (isRequestReplyListenerTaskBase)
import { Task, RequestReplyListenerTaskBase } from "@criticalmanufacturing/connect-iot-controller-engine";
import type { Communication, System } from "@criticalmanufacturing/connect-iot-controller-engine";

/** Default values for settings */
export const SETTINGS_DEFAULTS: <%= $CLI_PARAM_ClassName %>Settings = {
    autoEnable: true,
    subjectToSubscribe: "",
    replyTimeout: 60000,
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
 * ### Settings
 * See {@see <%= $CLI_PARAM_ClassName %>Settings}
 */
@Task.Task()
export class <%= $CLI_PARAM_ClassName %>Task extends RequestReplyListenerTaskBase implements <%= $CLI_PARAM_ClassName %>Settings {

    /** Accessor helper for untyped properties and output emitters. */
    // [key: string]: any;

    /** **Inputs** */
<%= $CLI_PARAM_InputsInterface %>

    /** **Outputs** */
<%= $CLI_PARAM_OutputsInterface %>

    /** Properties Settings */
    /** Should the subscription to the Message bus be activated at initialize time */
    public autoEnable: boolean = SETTINGS_DEFAULTS.autoEnable;
    /** What subject to subscribe */
    public subjectToSubscribe: string = SETTINGS_DEFAULTS.subjectToSubscribe;
    /** Amount of time allowed to wait for a reply before issuing a timeout */
    public replyTimeout: number = SETTINGS_DEFAULTS.replyTimeout;
<%= $CLI_PARAM_SettingsInterface %>


    // Current subscribed subject
    private _subscribedSubject?: string;

    public constructor() {
        super({
            activate: async () => this.subscribe(),
            deactivate: async () => this.unsubscribe(),
            getAutoActivateValue: () => this.autoEnable,
        });
    }

    /**
     * Event handler called when the MB callback is triggered for the action group currently set.
     */
    private _eventHandler: Communication.MessageBusCallback<any> = (subject: string, data: any, reply?: any): void => {
        data = this.tryParseJsonObject(data);

        const isRequest = reply != null;
        const sendReply = async (replyObject: any): Promise<void> => {
            this._logger.info(`Sending reply message '${JSON.stringify(replyObject)}' to the sendRequest: '${subject}'`);
            reply?.({ content: replyObject });
        };

        void this.handleRequest({
            subject,
            payload: data,
            timeout: this.replyTimeout,
            reply,
            originalData: {
                subject: subject,
                data: data,
                isRequest: isRequest,
            },
            context: {
                subject: subject,
                data: data,
                isRequest: isRequest,
            },
            outputs: {
                "subject": subject,
                "data": data,
                "isRequest": isRequest,
                "success": true,
            },
            onTimeout: async (errorMessage): Promise<void> => {
                this.logAndEmitError(errorMessage.message);
                if (isRequest) {
                    this._logger.info(`Sending reply message '${JSON.stringify(errorMessage)}' to the sendRequest: '${subject}'`);
                    reply?.({ content: errorMessage });
                }
            },
            onReply: sendReply,
            onError: async (errorMessage): Promise<void> => {
                if (!isRequest) {
                    this._logger.error(`Execution failed with error '${errorMessage.message}'`);
                } else {
                    this._logger.warning(`Execution failed with error '${errorMessage.message}'. Send error reply`);
                    this._logger.info(`Sending reply message '${JSON.stringify(errorMessage)}' to the sendRequest: '${subject}'`);
                    reply?.({ content: errorMessage });
                }
            },
        });
    };


    /**
     * Subscribes a subject to be listening to
     * @param newSubject Subject to subscribe
     */
    private subscribe(): void {
        this._messageBus.subscribe(this.subjectToSubscribe, this._eventHandler);
        this._logger.info(`Subscribing to '${this.subjectToSubscribe}'`);
    }

    /**
     * Unsubscribes from a subject (if there are active subscriptions)
     */
    private unsubscribe(): void {
        this._logger.info(`Unsubscribing from '${this._subscribedSubject}'`);
        this._messageBus.unsubscribe(this._subscribedSubject, this._eventHandler);
    }

    /**
     * Applies default settings before the task is initialized.
     */
    public override async onBeforeInit(): Promise<void> {
        await super.onBeforeInit();
        this.sanitizeSettings(SETTINGS_DEFAULTS);
    }

}

// Add settings here
/** <%= $CLI_PARAM_ClassName %> Settings object */
export interface <%= $CLI_PARAM_ClassName %>Settings extends System.TaskDefaultSettings {
    /** Should the subscription to the Message bus be activated at initialize time */
    autoEnable: boolean;
    /** What subject to subscribe */
    subjectToSubscribe: string;
    /** Amount of time allowed to wait for a reply before issuing a timeout */
    replyTimeout: number;
<%= $CLI_PARAM_SettingsInterface %>
}

//#elseif (isSystemOperationTaskBase)
import { Task, SystemOperationTaskBase } from "@criticalmanufacturing/connect-iot-controller-engine";
import type { System } from "@criticalmanufacturing/connect-iot-controller-engine";

/** Default values for settings */
export const SETTINGS_DEFAULTS: <%= $CLI_PARAM_ClassName %>Settings = {
    systemRetries: 10,
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
 * ### Settings
 * See {@see <%= $CLI_PARAM_ClassName %>Settings}
 */
@Task.Task()
export class <%= $CLI_PARAM_ClassName %>Task extends SystemOperationTaskBase implements <%= $CLI_PARAM_ClassName %>Settings {

    /** Accessor helper for untyped properties and output emitters. */
    // [key: string]: any;

    /** **Inputs** */
<%= $CLI_PARAM_InputsInterface %>

    /** **Outputs** */
<%= $CLI_PARAM_OutputsInterface %>

    /** Properties Settings */
<%= $CLI_PARAM_SettingsInterface %>
    public systemRetries: number = SETTINGS_DEFAULTS.systemRetries;

    /**
     * Triggers the system call
     * @param changes Changes that triggered the activation.
     * @param activatedValue Activation value received by the task.
     */
    protected override async onActivate(changes: Task.Changes, activatedValue: any): Promise<void> {
        // const input = new System.LBOS.Cmf.Foundation.BusinessOrchestration.SecurityManagement.InputObjects.GetRoleByNameInput();
        // input.Name = "User"

        // const output = await this.executeSystemCall<System.LBOS.Cmf.Foundation.BusinessOrchestration.SecurityManagement.OutputObjects.GetRoleByNameOutput>(input, {
        //     systemRetries: this.systemRetries,
        //     retries: this.retries, // defined in SystemOperationTaskBase
        //     sleepBetweenRetries: this.sleepBetweenRetries // defined in SystemOperationTaskBase
        // });

        // ... code here
        this.success.emit(true);
    }


}

// Add settings here
/** <%= $CLI_PARAM_ClassName %> Settings object */
export interface <%= $CLI_PARAM_ClassName %>Settings extends System.TaskDefaultSettings {
    /** Number of times that the system will retry to process the system call */
    systemRetries?: number;
<%= $CLI_PARAM_SettingsInterface %>
}

//#elseif (isSystemRequestListenerTaskBase)
import { Task, SystemRequestListenerTaskBase } from "@criticalmanufacturing/connect-iot-controller-engine";
import type { System } from "@criticalmanufacturing/connect-iot-controller-engine";

/** Default values for settings */
export const SETTINGS_DEFAULTS: <%= $CLI_PARAM_ClassName %>Settings = {
    autoEnable: true,
    subject: "", // The subject to subscribe to
    replyTimeout: 60000,
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
 * ### Settings
 * See {@see <%= $CLI_PARAM_ClassName %>Settings}
 */
@Task.Task()
export class <%= $CLI_PARAM_ClassName %>Task extends SystemRequestListenerTaskBase implements <%= $CLI_PARAM_ClassName %>Settings {

    /** Accessor helper for untyped properties and output emitters. */
    // [key: string]: any;

    /** **Inputs** */
<%= $CLI_PARAM_InputsInterface %>

    /** **Outputs** */
<%= $CLI_PARAM_OutputsInterface %>

    /** Properties Settings */
    /** Auto activate the event listeners */
    public autoEnable: boolean;
    /** Subject expected for the requested message */
    public subject: string;
    /** Amount of time allowed to wait for a reply before issuing a timeout */
    public replyTimeout: number;
<%= $CLI_PARAM_SettingsInterface %>

    public constructor() {
        super({
            getAutoActivateValue: () => this.autoEnable,
            getSubjectValue: () => this.subject,
            getHandlerValue: () => this._handleRequest,
        });
    }

    /** Normalizes the task settings before initialization completes. */
    public override async onBeforeInit(): Promise<void> {
        await super.onBeforeInit();
        this.sanitizeSettings(SETTINGS_DEFAULTS);
    }

    /** Handler triggered when a dock request is received */
    private _handleRequest: System.SystemEventCallback = async (data: { [key: string]: any }): Promise<any> => {
        const reply = data["reply"];

        void this.handleRequest({
            subject: this.subject,
            timeout: this.replyTimeout,
            reply,
            originalData: data,
            outputs: {
                // insert here the outputs
            },
            onReply: async (replyObject): Promise<void> => {
                this._logger.debug(`Sending reply message '${JSON.stringify({ content: replyObject })}' Request: '${this.subject}'`);
                reply?.({ content: replyObject });
            },
            onTimeout: async (errorMessage): Promise<void> => {
                this.logAndEmitError(errorMessage.message);
                reply?.({ content: errorMessage });
            },
            onError: async (errorMessage): Promise<void> => {
                this._logger.warning(`Execution failed with error '${errorMessage.message}'. Send error reply`);
                this._logger.info(`Sending reply message '${JSON.stringify(errorMessage)}' to the sendRequest: ${this.subject}`);
                reply?.({ content: errorMessage });
            },
        });

        return Promise.resolve(null);
    };


}

// Add settings here
/** <%= $CLI_PARAM_ClassName %> Settings object */
export interface <%= $CLI_PARAM_ClassName %>Settings extends System.TaskDefaultSettings {
    /** Auto activate the event listeners */
    autoEnable: boolean;
    /** Subject expected for the requested message */
    subject: string;
    /** reply Timeout */
    replyTimeout: number;
<%= $CLI_PARAM_SettingsInterface %>
}

//#elseif (isSystemRequestReplyTaskBase)
import { Task, System, SystemRequestReplyTaskBase } from "@criticalmanufacturing/connect-iot-controller-engine";

/** Default values for settings */
export const SETTINGS_DEFAULTS: <%= $CLI_PARAM_ClassName %>Settings = {
    defaultReply: {},
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
 * ### Settings
 * See {@see <%= $CLI_PARAM_ClassName %>Settings}
 */
@Task.Task()
export class <%= $CLI_PARAM_ClassName %>Task extends SystemRequestReplyTaskBase implements <%= $CLI_PARAM_ClassName %>Settings {

    /** Accessor helper for untyped properties and output emitters. */
    // [key: string]: any;

    /** **Inputs** */
    @Task.InputProperty(System.PropertyValueType.Object)
    public reply: object;
<%= $CLI_PARAM_InputsInterface %>

    /** **Outputs** */
<%= $CLI_PARAM_OutputsInterface %>

    /** Properties Settings */
    /** The default reply to send if the reply is not defined */
    public defaultReply: object;
<%= $CLI_PARAM_SettingsInterface %>

    constructor() {
        super();
    }

    /**
     * Sends the reply of dock when the task is activated.
     * @param changes Changes that triggered the activation.
     * @param activatedValue Activation value received by the task.
     */
    protected override async onActivate(changes: Task.Changes, activatedValue: any): Promise<void> {
        let replyObject = this.reply ?? this.defaultReply;
        this.reply = undefined;

        const normalizedReply = this.normalizeReplyMessage(replyObject);

        if (await this.sendReplyMessage(normalizedReply)) {
            this.success.emit(true);
        }
    }


}

// Add settings here
/** <%= $CLI_PARAM_ClassName %> Settings object */
export interface <%= $CLI_PARAM_ClassName %>Settings extends System.TaskDefaultSettings {
    /** The default reply to send if the reply is not defined */
    defaultReply: unknown;
<%= $CLI_PARAM_SettingsInterface %>
}
//#endif
