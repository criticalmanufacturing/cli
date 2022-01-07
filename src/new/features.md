# Feature package scaffolding

A traditional project structure targets the scenario where one team does all of the development for a project with a single target, e.g. a customization team deploying MES to a single factory or a set of factories with the exact same customization.

However, some projects are more complex. A single team can target a multi-site deployment in which the sites target a set of common features, but complement them with site-specific features, or the common features need site-specific data packages to configure them.

In these cases, projects are usually structured in a **Feature Packages** arrangement, in that some packages are self-contained, or in a **Layered Project** arrangement, in which e.g. a *Site* package, which is specific to a given site/plant, depends on a *Core/Baseline* package, which is common among all sites.

`cmf new` allows assembling these package in whatever structure is necessary, with a few limitations:
- the test solution is always at the repository level, i.e. we do not have feature-level test packages
- you cannot mix traditional packages, which exist in the repository root, with these packages
- a feature package must always exist inside a global repository

`cmf new` will automatically register each package as a dependency of its parent (as per the folder structure).

## Initialize the Repository

In case we are creating the first package in the repository, we need to initialize it in the same way as we do for a traditional project. Check the **Initialize the repository** section of the [traditional scaffolding instructions](./single.md).

## Creating a Feature Package
A **Feature Package** is a metapackage that can include one or more layer packages, e.g. we can have a Feature package that includes only a Business package.

A Feature package should include all necessary layer packages (business, UI, Help) needed for the Feature to work.

To create a new Feature Package, run the `new feature` command at your repository root, specifying the new package name: e.g. to create the Baseline package for your project, run:

```bash
cmf new feature Cmf.Custom.Baseline 
```

The new feature package will be available in your repository at `Features\Cmf.Custom.Baseline`.

After creating a Feature package, it is not longer possible to create new layer packages in the repository root. Any `new` command for a layer package will fail:

```bash
> cmf new html
  Cannot create a root-level layer package when features already exist.
```

## Creating Layer packages in a Feature package

Creating layer packages works exactly in the same way as for a [traditional project](./single.md), but the `cmf new` command should run from inside the feature package:

```bash
> cd Features/Cmf.Custom.Baseline
> cmf new business
The template "Business Package" was created successfully.
```

The layer package name will include the feature package name fragment to distinguish it from other packages in the same repository.