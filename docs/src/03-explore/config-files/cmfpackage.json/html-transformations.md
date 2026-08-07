# HTML package config transformations

The `Transform` action has a special case for HTML packages. It lets you add or change custom properties in the UI `config.json` without replacing the out-of-the-box file.

The `source` is a JSON transformation fragment, such as `my-transforms.json` (example name), and the `target` value is ignored by the packer. The transformation is generated into the package's root `config.json` during packing and is applied to the product-managed `assets/config.json` during deployment.

HTML package transformations use [Microsoft JSON Document Transforms (JDT)](https://github.com/microsoft/json-document-transforms). By default, JDT merges the transformation into the existing JSON document: properties in the transformation are added or override properties at the same path, while properties that are not mentioned are preserved. JDT also supports more advanced operations such as [rename](https://github.com/microsoft/json-document-transforms/wiki/Rename-Transformation), [remove](https://github.com/microsoft/json-document-transforms/wiki/Remove-Transformation), [merge](https://github.com/microsoft/json-document-transforms/wiki/Merge-Transformation), and [replace](https://github.com/microsoft/json-document-transforms/wiki/Replace-Transformation).

The source file is injected into the generated `config.json`, so it must contain JSON properties without an additional outer object. For example, `my-transforms.json` can contain:

```json
"advancedCustomization": true
```

Reference it from `cmfpackage.json` as follows:

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

For nested properties, use the same JSON structure as the target document. For example, this transform changes the REST timeout and adds a custom property:

```json
"host": {
    "rest": {
        "timeout": 180000
    }
},
"advancedCustomization": true
```

During deployment, the transformation is applied to the existing product-managed `assets/config.json`. For example, given this relevant part of the original file:

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

the resulting file contains the transformed values while preserving properties that were not included in the transformation:

```json
{
    "debug": {
        "isEnabled": false
    },
    "host": {
        "rest": {
            "timeout": 180000
        }
    },
    "advancedCustomization": true
}
```

For a transform that only adds the custom property, the existing timeout remains `120000`:

```json
"advancedCustomization": true
```

becomes:

```json
{
    "debug": {
        "isEnabled": false
    },
    "host": {
        "rest": {
            "timeout": 120000
        }
    },
    "advancedCustomization": true
}
```

The generated package applies the transformation during deployment, preserving the existing product-managed settings and variables.
