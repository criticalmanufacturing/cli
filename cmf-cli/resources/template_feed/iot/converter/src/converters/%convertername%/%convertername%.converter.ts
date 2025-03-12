import { Converter, DI, Dependencies, TYPES } from "@criticalmanufacturing/connect-iot-controller-engine";

/**
 * @whatItDoes
 *
 * >>TODO: Add description
 *
 */
@Converter.Converter()
export class <%= $CLI_PARAM_ClassName %>Converter implements Converter.ConverterInstance <<%= $CLI_PARAM_InputAsJS %>, <%= $CLI_PARAM_OutputAsJS %>> {

    @DI.Inject(TYPES.Dependencies.Logger)
    private _logger: Dependencies.Logger;

    /**
     * >>TODO: Enter description here!
     * @param value <%= $CLI_PARAM_InputAsJS %> value
     * @param parameters Transformation parameters
     */
    transform(value: <%= $CLI_PARAM_InputAsJS %>, parameters: { [key: string]: any; }): <%= $CLI_PARAM_OutputAsJS %> {

        // >>TODO: Add converter code
        this._logger.error("The code for the converter was not yet developed");
        throw new Error(">>TODO: Not implemented yet");

    }
}
