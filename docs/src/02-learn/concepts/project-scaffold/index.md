# Project Scaffold

The CM CLI offers two primary structures for customizing your project: 

* Traditional Scaffold;
* Feature-Driven scaffold.

Each approach handles different project complexities and organizational setups.

## Traditional Scaffold

**Ideal for:** Single teams targeting a single MES site or a group of sites with identical customizations.

These projects are usually composed of the following package layers:

* Business;
* Database;
* Data;
* Grafana (containers only);
* UI (Html and Help);
* IoT;
* Reporting (for SQL Server Reporting Services);
* IoT;
* Tests.

Each application layer is deployed in a different package. This allows the team to deliver only what was actively modified during a sprint, and keep the previous versions of the unchanged layers in an installed system.

Check the [Layers Packages Concept](../layers-packages/index.md) page to see the details for each package.

!!! note

    **Key Packages:** Business, UI (HTML), Help, and Data (Master Data) are commonly used in customization projects.

### Demo

This demo shows the usual initial setup for a new traditional project.
For the first sprints, which focus heavily on modelling, the Data
package is of the most importance. Obviously, a Test package is also
needed. As an extra, the Business package is also initialized. This
allows the Process Rules in the Data package to have a .NET solution
where they can be compiled for checking.

Note that the GIF is quite large and can take a bit to load.

![Traditional Scaffolding Demo](./traditional.gif "Traditional Scaffolding")

## Feature-Driven Scaffold

**Ideal for:** Complex projects involving multiple teams, multiple sites, or shared features with site-specific variations.

This structure organizes projects into feature-based packages, allowing for more flexibility and scalability. Two common arrangements are:
* **Feature Packages:** Self-contained packages focusing on specific features.
* **Layered Projects:** A hierarchical structure where site-specific packages depend on a *Core/Baseline* package.

!!! warning

    The `cmf new` command automatically registers package dependencies based on the folder structure.

**Limitation:**

* Test solutions reside at the repository root (there is not feature level testing).
* Traditional and feature-driven packages cannot be mixed in the same repository.
* Feature packages must be within a repository project folder.

The `cmf new` command allows creation of these packages in whatever structure is necessary, with a few limitations:

* The test solution is always at the root repository level, i.e. we do not have feature-level test packages;
* You cannot mix traditional packages, which exist in the repository root, with these packages;
* A feature package can only exist inside a global project folder.

!!! note

    The `cmf new` command will automatically register each package as a dependency of its parent (as per the folder structure).

### Demo

A **Feature Package** is a *meta package* that can include one or more layer packages, e.g. we can have a Feature package that includes only a Business package.

A Feature package should include all necessary layer packages (business, UI, Help) needed for the Feature to work.

To create a new Feature Package, run the `new feature` command at your repository root, specifying the new package name: e.g. to create the Baseline package for your project, run:

``` powershell
cmf new feature Cmf.Custom.Baseline 
```

The new feature package will be available in your repository at `Features\Cmf.Custom.Baseline`.

After creating a Feature package, it is no longer possible to create new layer packages
in the repository root. Any `new` command for a layer package will fail:

```PowerShell
cmf new html
```

```log
Cannot create a root-level layer package when features already exist.
```

Creating layer packages works exactly in the same way as for a [traditional project](#traditional-scaffold), but the `cmf new` command should run from inside the feature package directory:

``` powershell
# e.g. to create "business" package for Cmf.Custom.Baseline feature
cd Features/Cmf.Custom.Baseline
cmf new business
```

!!! important

    When creating a feature using `cmf new` you must provide the full unique name for the feature, as the CLI will not add any prefix or suffix.  
    The layer packages of each feature be prefixed with the feature name to
    distinguish it from other packages of same type in the same repository.

## Choosing the Right Scaffold

* **Traditional Scaffold:** Suitable for simpler projects with a single focus.
* **Feature-Driven Scaffold:** Ideal for complex projects with multiple teams, sites, or shared features.

## Additional Considerations

### Package Dependencies

Package dependencies within a project are specified in the `cmfpackage.json` file.
The CM CLI utilizes these definitions along with repository locations outlined in
the `repositories.json` file (found at the project's root) to resolve dependencies.

### Git Repository Management

Given that tests are scoped to the entire scaffold, they are primarily suitable for
scenarios where all features share a common initial state. However, Feature-Driven
projects often necessitate isolated testing environments for different feature
combinations or site-specific requirements. To address this, consider the following
Git repository strategies:

* **Single Git Repository:** Houses all common features, enabling efficient building
  and testing without isolation. This approach is beneficial for ensuring compatibility
  among features.
* **Multiple Git Repositories:** Each repository encapsulates a specific site or
  combination of features, allowing for tailored testing and dependency management
  for distinct production use cases.

For each Git repository, define its scope, establish the appropriate scaffold structure,
and meticulously review the `cmfpackage.json` and `repositories.json` files to
accurately reflect dependencies.
