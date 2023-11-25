# Action Flow
[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=rydersir_action-flow&metric=alert_status)](https://sonarcloud.io/summary/new_code?id=rydersir_action-flow)

## Overview
Action Flow is a workflow engine that can be used to abstract, customize business rules, or add integration to an existing API.


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
