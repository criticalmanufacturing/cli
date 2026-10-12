import { ScriptScopeBase } from "../types/globals";

// eslint-disable-next-line @typescript-eslint/no-unused-vars
class DummyWrapper extends ScriptScopeBase {
    [key: string]: any;

    private dummy() {
        // PackagePacker: Start of Script
        (async () => {
        })();
        // PackagePacker: End of Script
    }
}

