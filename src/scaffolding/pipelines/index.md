# Pipelines
This page gives an overview of our pipeline design and working mode. For specific details, you can consult the generated pipelines, by running the `cmf init` command as detailed [here](./index.md).

> Please remember that these pipelines are tailored to CM's internal structure and are not expected to run unmodified in your infrastructure. They are provided as examples and should not replace your own pipelines adapted to you specific process.

## Pull Request (PR) process
### Runs each time a Pull Request is opened or code is pushed to the request branch. It is comprised of two pipelines. Check all the details [here](PR/index.md).

## Continuous Integration (CI) process
### Pipelines responsible for generating packages anytime main branch is updated. Check all the details [here](CI/index.md).

## Continuous Delivery (CD) process
### Our [CD process](CD/index.md) doesn't actually deliver to client environments, but to our production-like Integration environments. It can be executed by one of two pipelines, depending if we're targeting a Windows machine or a Container installation. However, both pipelines implement different approaches to the same process. Check all the details [here](CD/index.md).