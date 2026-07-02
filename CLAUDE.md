# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Overview

ActionFlow is a workflow engine that abstracts business rules and API integrations into declarative, expression-driven workflows. The repository has two independent parts:

- **`ActionFlow/`** + **`ActionFlow.Tests/`** — the core engine, a .NET 10 class library (not an app or API yet) with an MSTest test suite. This is the primary codebase.
- **`actionFlow.editor/`** — a Next.js 16 / React 19 visual editor (React Flow / `@xyflow/react`, flowbite-react + Tailwind CSS v4) for authoring workflows. Independent tooling with its own build; not wired to the engine at runtime.

## Commands

### .NET engine (run from repo root)
```bash
dotnet restore
dotnet build
dotnet test                                      # run all tests
dotnet test --filter "FullyQualifiedName~ActionFlowEngineTests"   # single test class
dotnet test --filter "Name=MethodName"           # single test method
```
Tests use MSTest 4 + FluentAssertions + NSubstitute. Note CI (`build.yml`) runs on `master` via SonarCloud; `dotnet.yml` targets the `main` branch which does not exist — treat `master` as the active branch. The `SendHttpCallActionTests` and `ApiClientTests` make real network calls to `http://httpbin.org` and will fail when that service is unavailable — they are integration tests, not unit tests.

### Visual editor (run from `actionFlow.editor/`)
```bash
npm install
npm run dev        # dev server on http://localhost:3000
npm run build
npm run lint       # eslint . (flat config, eslint.config.mjs) — Next 16 removed `next lint`
```

The editor uses **flowbite-react v0.12 with Tailwind CSS v4**. Setup pieces that must stay in sync: `next.config.js` wraps the config with `withFlowbiteReact` (from `flowbite-react/plugin/nextjs`), which auto-generates `.flowbite-react/class-list.json` on build/dev; `globals.css` uses `@import "tailwindcss"` plus flowbite's `@import`/`@source` directives and `@config "../../tailwind.config.js"` (the JS config only preserves `important: true` / `darkMode`). PostCSS uses `@tailwindcss/postcss`. If styling breaks, regenerate the class list with `npx flowbite-react build`.

## Core engine architecture

The execution pipeline flows: **Workflow → Steps → Actions**, all glued together by string-keyed lookups and expressions.

- **`ActionFlowEngine`** (`Engine/ActionFlowEngine.cs`) — entry point. `ExecuteWorkflowAsync(workflowName, params Parameter[])` loads workflows (cached in a dictionary keyed by name), then runs each `Step` through the `StepExecutionEvaluator`. Returns `ActionFlowEngineResult` whose `OutputParameters` are computed by evaluating the workflow's declared output expressions against the final context.

- **`ExecutionContext`** (`Engine/ExecutionContext.cs`) — wraps a DynamicExpresso `Interpreter`. Two distinct stores:
  - **Parameters** = persistent variables set on the interpreter (`AddOrUpdateParameter`), visible to all subsequent expressions.
  - **Action properties** = transient, per-step config (`AddOrUpdateActionProperty`), cleared after each step via `ClearActionProperties`. Actions read their config from here.
  Also exposes `GetCurrentEngine()` so actions can recurse into the engine (used by `ForLoop`/`CallWorkflow`).

- **`StepExecutionEvaluator`** — for each step: evaluates `ConditionExpression` (skip if false), copies the step's `Properties` into the context as action properties, resolves the action via the factory, runs it, then clears action properties.

- **`StepActionFactory`** (`Engine/Factories/`) — maps an `ActionType` string to an `IActionBase` instance. Populated from all DI-registered `IActionBase` implementations, keyed by each action's `ActionType` property. Supports runtime `AddOrUpdate`/`Remove` for custom actions.

- **`IWorkflowProvider`** — supplies `Workflow` definitions. `BlankWorkflowProvider` (default) returns none; `AddJsonWorkflowProvider` (`Engine/Providers/Extensions/`) deserializes workflows from a JSON file/array. Its `ObjectToStringConverter` coerces all step property values to strings, since properties are treated as DynamicExpresso expressions.

### Expressions
Everything is a DynamicExpresso expression: variable values, step conditions, output parameters, loop bounds, HTTP call fields. When adding a feature, assume string inputs are expressions evaluated against the current `ExecutionContext`, not literals.

## Writing a new action

1. Subclass `ActionBase`, override `ActionType` (a unique string) and `ExecuteAction()`.
2. Read config from `ExecutionContext.GetActionProperty<T>(key)`; write results with `AddOrUpdateParameter`.
3. Register it: `services.AddScoped<IActionBase, YourAction>()`, or register at runtime via `StepActionFactory.AddOrUpdate`.

Built-in actions and their `ActionType` keys — **note these do not always match the class name**, and the factory lookup is by this exact string:
- `SetVariableAction` → `"SetVariable"` (but sample/editor JSON use `"Variable"`)
- `SendHttpCallAction` → HTTP call
- `ForLoopAction` → `"ForLoop"` (recurses via `engine.GetStepExecutionEvaluator()`)
- `ControlFlowAction`
- `CallWorkflowAction` → `"CallWorkFlow"` (runs another workflow in a fresh `ExecutionContext`, maps its outputs back to a result variable)

## DI setup

`services.UseActionFlowEngine()` (`Extensions/ServiceCollectionExtensions.cs`) registers the engine, evaluator, factory, `ApiClient`, `AddHttpClient()`, a `BlankWorkflowProvider`, and all default actions. Override the workflow provider afterward (e.g. `AddJsonWorkflowProvider`) to supply real workflows.

## Editor architecture (`actionFlow.editor/`)

Next.js App Router. A workflow is modeled as a tree of React Flow nodes (`src/components/nodes/`, one component per action type, keyed in `nodes/index.ts` via `NodeTypeKeys`). `src/modules/nodes/node-generator.ts` creates default node data per type; tree layout lives in `src/components/tree/`. The editor's node/action types mirror the engine's actions but the two are not yet integrated.
