
[![Build Status](https://github.com/intelligentspaces/AdxUtils/actions/workflows/ci.yml/badge.svg)](https://github.com/intelligentspaces/AdxUtils/actions/workflows/ci.yml)

[![Publish To Nuget](https://github.com/intelligentspaces/AdxUtils/actions/workflows/publish.yml/badge.svg)](https://github.com/intelligentspaces/AdxUtils/actions/workflows/publish.yml)

# Azure Data Explorer Utilities

This is a command line application which is intended to provide a useful set of utilities for working with Azure Data Explorer. Mostly to make things easier where some tasks are not supported by the [Azure CLI](https://learn.microsoft.com/cli/azure/) or are needed to work in a way to support other activities.

## Authentication

The app can currently use either Client Secret Key authentication, or Azure CLI authentication. Using CLI authentication allows the application to make use of the existing sign-in information from the Azure CLI tool.

## Export

This is the first of the utilities developed which automates the generation of a CSL script for a given database. The script contains the commands needed to re-create the tables and functions, along with mapping information. It also supports renaming of tables and the ability to copy data using an `.ingest inline` command. The generated output is intended to work with the [Azure DevOps Task for Data Explorer](https://learn.microsoft.com/azure/data-explorer/devops) where the commands are split based on empty lines and executed individually against the target database. The generated script can just as easily be executed directly by using an `.execute database script`.

An example usage of the command is

```bash
> adxutils -c https://myinstance.region.kusto.windows.net/
           -d my-database
           --ignore Table3
           --export Table1,Table2
           --rename Table4=Table5
           --use-cli
```
