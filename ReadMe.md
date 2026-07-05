# Action Flow
[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=jakemoresca_ActionFlow&metric=alert_status)](https://sonarcloud.io/summary/new_code?id=jakemoresca_ActionFlow)

## Overview
Action Flow is a workflow engine that can be used to abstract, customize business rules, or add integration to an existing API.

## Components
1. ActionFlow (/ActionFlow) - main core component that contains functionalities for actions, action execution, action registration. It is intentionally just a class library as it won't offer a way to persist workflow, custom action registration, or endpoints. The only way to run it is to call the ActionFlowEngine itself. The idea is another component will act as a host for this service either as a monolithic service or a bunch of microservices (one for workflow execution, endpoints, etc).
Action compilation/optimization(built in) should be handled here. 
2. actionFlow.editor - Default editor for the ActionFlow ecosystem. This nextjs app will be used to interact with future ActionFlow components. (ActionFlow.API for now)
3. ActionFlow.Runner - (not yet created). This will be the service that will host ActionFlow component. Interaction with caching (can still be a third party). Communications will be handled by a broker (probably kafka).
The idea is it will contain bunch of interfaces (and a way to provide one) that can be used for hosting default or custom service mainly:
- WorkflowProvider - A way for loading the workflows
- ActionRegistry - A way for registering actions (with or without the default ones)
4. ActionFlow.API - (not yet created). Exposes endpoints for interacting with Workflows (either action execution, designing, debugging)
5. ActionFlow.DB - Is a document store that will be used by a DocumentStoreProvider inherited from IWorkflowProvider which is the workflow provider of the ActionFlow Engine. This will be the default provider of ActionFlow.Runner 

> **Implementation plan for Components 3, 4 & 5:** see [docs/microservices-plan.md](docs/microservices-plan.md) — a broker-based (Kafka), SAGA-coordinated microservice design using MartenDB (Marten + Wolverine).


## How to use
Action Flow is currently on it's infancy stage. But in the future it will offer a way to be used as a Class Library or as an API with a visual Action Flow editor.

## Features

1. Expressions everywhere - Using [DynamicExpresso](https://github.com/dynamicexpresso/DynamicExpresso), You can use expressions on setting variable value, add conditions, call external API.
2. Extensible actions - Creating a new action out of the default actions is easy.
3. Current actions:
	- Set Variable
	- Call HTTP
	- For Loop
	- Control Flow
	- Call Workflow

## Roadmap
**According to priority:**
1. Persistent storage - will use Postgresql on first iteration
2. Action Flow editor
3. Docker container support
