# Project Scaffolding

The CM CLI offers two primary structures for customizing your project: Traditional and Feature-Driven. Choosing the right approach depends on your project's complexity and organizational structure.

## Strategy 1: Traditional Scaffold

**Best for:** Simple projects with a single team targeting one or more sites with *identical* customizations.

This structure organizes your project into distinct layer packages:

* **Business:** Backend business logic.
* **Database:** Database schema and scripts.
* **Data:** Master data, DEEs and other exported objects.
* **Grafana (containers only):** Dashboards and monitoring configurations.
* **UI (HTML and Help):** User interface and documentation.
* **IoT:** Integrations with Internet of Things devices.
* **Reporting (for SQL Server Reporting Services):** Reporting components.
* **Tests:** Unit and integration tests.

Each layer is deployed as a separate package, allowing for granular deployments and minimizing disruption to unchanged components. This is particularly useful for agile development, where only modified layers are deployed during each sprint.

For more details on each package layer, see the [Layer Packages Concept](../layers-packages/index.md) page.

!!! note "Key Packages"
    The **Business**, **UI (HTML)**, **Help**, and **Data (Master Data)** packages are commonly used in customization projects.

### Demo: Traditional Scaffolding

This demo showcases the initial setup for a typical traditional project. Early sprints often focus on data modeling, making the **Data** package crucial. A **Test** package is also essential from the start. The **Business** package might be initialized early to enable compilation and validation of Process Rules within the **Data** package.

![Traditional Scaffolding Demo](./traditional.gif "Traditional Scaffolding Demo")

## Strategy 2: Feature-Driven Scaffold

**Best for:** Complex projects with multiple teams, multiple sites, or shared features with site-specific variations.

This approach organizes projects around *features*, promoting modularity, flexibility, and scalability. Two common arrangements are:

* **Feature Packages:** Self-contained packages focused on specific features.
* **Layered Projects:** A hierarchical structure where site-specific packages depend on a *Core/Baseline* package.

!!! warning "Limitations"

    * Test solutions reside at the repository root (no feature-level testing).
    * Traditional and feature-driven packages cannot be mixed within the same repository.
    * Feature packages must reside within a repository project folder.

The `cmf new` command facilitates the creation of these packages, but with some constraints:

* The test solution remains at the repository root; feature-level testing is not supported.
* Traditional packages (at the repository root) cannot coexist with feature-driven packages.
* Feature packages must be placed within a designated project folder within the repository.

!!! note "Automatic Dependency Registration"
    The `cmf new` command automatically registers package dependencies based on the folder structure.

### Repository Organization for Feature-Driven Projects

Feature-driven projects often benefit from a multi-repository structure, e.g.:

* **`Repo_Common`:** Enterprise-wide MES features (industry-agnostic).
* **`Repo_Medical_Equipment_01`:** Features specific to medical equipment sites, potentially reusing features from `Repo_Common`.
* **`Repo_Medical_Drugs_01`:** Features specific to medical drug sites, potentially reusing features from `Repo_Common`.
* **`Repo_SemiConductors_01`:** Features specific to semiconductor sites, potentially reusing features from `Repo_Common`.
* **`Repo_Site_Medical_01`:** Medical site-specific customizations, combining features from relevant repositories.
* **`Repo_Site_Medical_02`:** Another medical site-specific repository.
* **`Repo_Site_Semi_01`:** A site-specific repository for semiconductors.

This structure allows for clear separation of concerns and independent management of features. If a feature requires complete isolation (including its tests), consider moving it to a separate repository and refactoring existing repositories as needed.

### Demo: Feature-Driven Scaffolding

A **Feature Package** is a "meta-package" containing one or more layer packages (e.g., a Feature package might contain only a Business package). Ideally, a Feature package includes all necessary layer packages (Business, UI, Help, etc.) for the feature to function.

**Creating a Feature Package:**

Use the `cmf new feature` command at the repository root:

```powershell
cmf new feature Cmf.Custom.Baseline
```

This creates the `Cmf.Custom.Baseline` feature package in the `Features\Cmf.Custom.Baseline` directory.

**Creating Layer Packages within a Feature:**

After creating a feature, you cannot create layer packages directly in the repository root. You must navigate to the feature's directory:

```powershell
cd Features/Cmf.Custom.Baseline
cmf new business  # Creates the Business package for the Baseline feature
cmf new html      # Creates the HTML package for the Baseline Feature
```

Attempting to create a root-level layer package after creating features will result in an error:

```powershell
cmf new html
```

```log
Cannot create a root-level layer package when features already exist.
```

!!! important "Feature Naming and Layer Package Prefixing"
    Use the full, unique name for the feature when using `cmf new feature` command. The CLI does not add prefixes or suffixes. Layer packages within a feature should be prefixed with the feature name (e.g., `Cmf.Custom.Baseline.Business`, `Cmf.Custom.Baseline.UI`) to avoid naming conflicts.

## Choosing Scaffold Strategy

* **Traditional:** Simpler projects, single team, identical site customizations.
* **Feature-Driven:** Complex projects, multiple teams/sites, shared features with variations.

## Additional Considerations

### Package Dependencies

Each package layer dependencies are defined in `cmfpackage.json`. The CM CLI uses these definitions and the `repositories.json` file (at the project root) to resolve dependencies and their location.

### Git Repository Management

Tests are scoped to the entire repository. For Feature-Driven projects, consider these Git strategies:

* **Single Repository:** All common features in one repository. Good for compatibility testing but lacks isolation.
* **Multiple Repositories:** Each repository for a site or feature combination. Enables tailored testing and dependency management.

For each repository, define its scope, choose the appropriate scaffold, and ensure `cmfpackage.json` and `repositories.json` accurately reflect dependencies.
