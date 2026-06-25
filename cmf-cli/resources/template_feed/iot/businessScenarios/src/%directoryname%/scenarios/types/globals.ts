// ---------------------------------------------------------------------------
// ScriptScopeBase — extend this in every wrapper class
// ---------------------------------------------------------------------------

import { ScriptScopeLboUtilities } from "cmf-core-chatbot/lib/side-bar-bot/script-executer/scope.lbo.utilities";
import { ScriptScopeIoTUtilities } from "cmf-core-chatbot/lib/side-bar-bot/script-executer/scope.iot.utilities";
import { MasterdataDirector } from "cmf-core-chatbot/lib/side-bar-bot/script-executer/scope.masterdata.utilities";
import { ControlFlowBuilder, WorkflowBuilder } from "cmf-core-chatbot/lib/side-bar-bot/script-executer/scope.workflow-builder.utilities";
import { Cmf } from "cmf-lbos";
import { DataType } from "cmf-core-chatbot/lib/side-bar-bot/script-executer/utils";
/**
 * Base class that provides typed access to all runtime-injected utilities.
 *
 * Usage:
 * ```ts
 * class MyScriptWrapper extends ScriptScopeBase {
 *     [key: string]: any;
 *     private myScript() { ... }
 * }
 * ```
 */
declare class ScriptScopeBase {
    /** Orchestrates creation of protocols, driver definitions, and controllers. */
    masterdataDirector: MasterdataDirector;

    /** Builds and manages automation workflows composed of tasks. */
    workflowBuilder: ControlFlowBuilder;

    /** Utilities for IoT entities, events, properties, and commands. */
    iotUtilities: ScriptScopeIoTUtilities;

    /** Utilities for LBO data, state models, and master data. */
    lboUtilities: ScriptScopeLboUtilities;

    /** Answers collected from the user during the business scenario wizard. */
    answers: Record<string, any>;

    /** Low-level system call accessor (CMF service layer). */
    System: any;
}

export { ScriptScopeBase };