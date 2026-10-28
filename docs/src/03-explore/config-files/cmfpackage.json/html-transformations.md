# HTML package config transformations

The `Transform` action has a special case for HTML packages. It lets you add or change custom properties in the UI `config.json` without replacing the out-of-the-box file.

The `source` is a JSON transformation fragment, such as `my-transforms.json` (example name), and the `target` value is ignored by the packer. The path is resolved relative to the `cmfpackage.json` of the HTML package layer. The transformation is generated into the package's root `config.json` during packing and is applied to the product-managed `assets/config.json` during deployment.

HTML package transformations use [Microsoft JSON Document Transforms (JDT)](https://github.com/microsoft/json-document-transforms). By default, JDT merges the transformation into the existing JSON document: properties in the transformation are added or override properties at the same path, while properties that are not mentioned are preserved. JDT also supports more advanced operations such as [rename](https://github.com/microsoft/json-document-transforms/wiki/Rename-Transformation), [remove](https://github.com/microsoft/json-document-transforms/wiki/Remove-Transformation), [merge](https://github.com/microsoft/json-document-transforms/wiki/Merge-Transformation), and [replace](https://github.com/microsoft/json-document-transforms/wiki/Replace-Transformation).

The source file is injected into the generated `config.json`, so it must contain JSON properties without an additional outer object. Multiple `Transform` entries are allowed and are concatenated into the same generated file.

The generated `config.json` also contains a `customizationVersion` property, set to the package version, which is applied together with your transformation.

## Example 1 - Set timeout and enable advanced customization

The UI reads the advanced customization flag only from `debug.advancedCustomization`.

Consider a `my-transforms.json` file with the following contents. It sets the REST timeout to 180 seconds and turns on advanced customization:

```json
"host": {
    "rest": {
        "timeout": 180000
    }
},
"debug": {
    "advancedCustomization": true
}
```

Reference it from the `cmfpackage.json` of the HTML package layer, as follows:

```json
{
    "contentToPack": [
        {
            "source": "my-transforms.json",
            "action": "Transform"
        }
    ]
}
```

During deployment, the transformation is applied to the existing product-managed `assets/config.json`. For example, imagine that the relevant part of its original content is:

```json
{
    "debug": {
        "isEnabled": false
    },
    "host": {
        "rest": {
            "timeout": 120000
        }
    }
}
```

After the transformation is applied, the resulting file contains the new values while preserving the properties that were not mentioned:

```json
{
    "debug": {
        "isEnabled": false,
        "advancedCustomization": true
    },
    "host": {
        "rest": {
            "timeout": 180000
        }
    }
}
```

## Example 2 - Enable advanced customization only

Imagine the same original `config.json` as in the example above, but now with a transformation that only turns on advanced customization:

```json
"debug": {
    "advancedCustomization": true
}
```

The resulting `config.json` keeps the original timeout (`120000`) and the `debug.isEnabled` value, and only `debug.advancedCustomization` is added:

```json
{
    "debug": {
        "isEnabled": false,
        "advancedCustomization": true
    },
    "host": {
        "rest": {
            "timeout": 120000
        }
    }
}
```

The generated package applies the transformation during deployment, preserving the existing product-managed settings and variables.
