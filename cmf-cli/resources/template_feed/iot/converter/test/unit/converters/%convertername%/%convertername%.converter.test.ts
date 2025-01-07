import "reflect-metadata";
import { Converter } from "@criticalmanufacturing/connect-iot-controller-engine";
import EngineTestSuite from "@criticalmanufacturing/connect-iot-controller-engine/test";
// import * as chai from "chai";
import { <%= $CLI_PARAM_ClassName %>Converter } from "../../../../src/converters/<%= $CLI_PARAM_ConverterName %>/<%= $CLI_PARAM_ConverterName %>.converter";

describe("<%= $CLI_PARAM_Title %> converter", () => {

    let converter: Converter.ConverterContainer;

    beforeEach(async () => {
        converter = await EngineTestSuite.createConverter({
            class: <%= $CLI_PARAM_ClassName %>Converter
        });
    });

    it("should convert", async (done) => {
        /* Example int to string
        let result: string = await converter.execute(123, {
            parameter: "something"
        });

        chai.expect(result).to.equal("123");
        */
        done();
    });

});
