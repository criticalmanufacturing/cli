# Pipeline Import

To import the Continuous Integration pipelines, first make sure you have scaffolded your project successfully and can see a folder in the project root named Builds with several YAML files in it:

![Pipeline List](../images/pipelines/pipeline_list.png "Pipeline list")

If so, we can proceed. Push your repository if you haven't yet. It is important that the default branch contains the pipeline YAML files. In our setup, the default branch is always **development**.

For each of **PR-Changes**, **PR-Package**, **CI-Changes** and **CI-Package**, repeat these steps:

1. create a new pipeline

    ![New Pipeline](../images/pipelines/pipeline_new.png "New Pipeline")

1. Choose **Use the classic editor**

    ![Classic Editor](../images/pipelines/classic_editor.png "choose Use the classic editor")

1. Choose your Team Project, repository and default branch. Our generated pipelines are expecting the default branch to be **development**

    ![Configure Pipeline source](../images/pipelines/pipeline_config.png "configure pipeline source")

1. After that, choose Configuration as code > YAML

    ![Configuration as Code - YAML](../images/pipelines/pipeline_yaml.png "configuration as code - YAML")

1. and choose the corresponding YAML file in the Builds folder

    ![Choose YAML file](../images/pipelines/pipeline_yaml_select.png "YAML file select")


## Pull Request policies
To effectively leverage the Pull Request flow our pipelines enable, Azure DevOps must be configured to protect the default (in our case, **development**) branch.

Go to Repos > Branches, on the **development** row, click the More button on the far right and select Branch Policies

![Branch Policies](../images/pipelines/branch_policies.png "branch policies")

The policy options per-se are dependent on the team configuration and working mode. Here is an example of our policies

![Policies](../images/pipelines/policy_options.png "policies")

The important point here is to set the **PR-Changes** pipeline to run whenever a Pull Request is opened. To do so, add a new row in **Build validation** that looks like this

![Build validation](../images/pipelines/policy_build_validation.png "build validation with PR-Changes")

This will ensure the PR pipelines will run each time code is pushed to a Pull Request, and will block merging if the build is failing.

## Remaining pipelines
We do not support running the Continuous Delivery pipelines in a client setting. These pipelines are strongly opinionated on the environment in which they run and require virtual machines configured specifically for them. They are still provided as an example and guide so that each client or partner can create their own pipelines. However, they will not work out of the box and will hang on permissions issues if attempted to run.