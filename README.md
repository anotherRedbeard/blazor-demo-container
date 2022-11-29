# Blazor container demo

This application is an example Blazor client app that has been containerized so it can be deployed into a Azure Container App.

## Description

This project was created initially by using the sample Blazor todo list app from the Microsoft learn site:  [Build a Blazor todo list app](https://learn.microsoft.com/en-us/aspnet/core/blazor/tutorials/build-a-blazor-app?view=aspnetcore-6.0&pivots=webassembly).  I choose to use the Blazor WebAssembly so the code would be downloaded to the client.  The only changed I made was to have it call a Web Api Container App that I created for another demo.  You can find that here:  [Web Api Container Demo](https://github.com/anotherRedbeard/web-api-demo-container) . There is also a Dockerfile as part of this project that is used to containerize the app and push it up to Azure Container Registry(ACR).  Once the image is in ACR, it then creates/updates the Azure Container App for the Blazor demo web app.  All of this is done in the `deploy-package.yml` GitHub actions workflow.  See workflow section below for more specifics on the workflow.

## Badges

| Workflow Name     | Badge |
| ----------- | ----------- |
| Azure Container App Deployment | [![Trigger container apps deployment](https://github.com/anotherRedbeard/blazor-demo-container/actions/workflows/deploy-aca-package.yml/badge.svg)](https://github.com/anotherRedbeard/blazor-demo-container/actions/workflows/deploy-aca-package.yml) |
| Azure Kubernetes Service (AKS) Deployment | [![Trigger aks app deployment](https://github.com/anotherRedbeard/blazor-demo-container/actions/workflows/deploy-aks-package.yml/badge.svg)](https://github.com/anotherRedbeard/blazor-demo-container/actions/workflows/deploy-aks-package.yml) |

## How to use

This is meant to be a repo that you can clone and use as you like.  The only thing you will need to change is the variables in the `deploy-package.yml` workflow.  They will be in the `env` section of the workflow.  There will need to change to match the resource names you would like to use in your Azure Subscription.

### Requirements

- **Azure Subscription**
- **This repo cloned in your own GitHub repo**
- **Service principle with contributor access to the subscription created as a GitHub Secret**
  - This is only so you can create your resource group at the subscription level, if you don't want to give your service principle that kind of access you will need to have another way to create the resource group and then you can remove that step from the workflow
  - The credentials for this service principle need to be stored according to this document:  [Service Principal Secret](https://learn.microsoft.com/en-us/azure/developer/github/connect-from-azure?tabs=azure-portal%2Clinux#use-the-azure-login-action-with-a-service-principal-secret)
  - I have used the name `AZURE_CREDENTIALS` for the secret

## GitHub Workflows

There are a few workflows used by this project to demonstrate the different ways and resources you can deploy to.  Currently there are examples for an Azure Container App and Azure Kubernetes Service (AKS).

### `deploy-aca-package.yml`

The workflow will deploy everything it needs to a given resource group and into an Azure Container App.  It has 3 separate stages: build-infra, build, deploy

1. build-infra
    - Creates all the required infrastructure you need for Azure Container Apps using the `az cli`.  These actions are idempotent so they can be run multiple times.
2. build
    - Builds the container and tags the image
3. deploy
    - Uses the container image that was built and pushed to ACR and creates/updates that container app with that newly built image

### `deploy-aks-package.yml`

The workflow is assuming that you have everything in place from an infrastructure perspective (AKS Cluster, namespace, container registry, and log analytics workspace).  You can find an example of how to deploy that infrastructure in this GitHub repo.  I use that one with this one [AKS Cluster Deploy Example using Bicep](https://github.com/anotherRedbeard/web-api-demo-container/blob/main/.github/workflows/deploy-aks-package.yaml#L25).

This workflow has 2 separate stages: buildImage, deploy:

1. buildImage
    - We use the `az aks command invoke` command to get the endpoint for the api so we can set it in the `appsettings.json` file.  This way it can be dynamic when we deploy.
    - Builds the container and tags the image
2. deploy
    - We use a `sed` script to replace variables in the AKS deployment file so it can be dynamic to use the container image we just built.
    - Then we use the deployment file to setup the deployment in AKS
