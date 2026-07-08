// Converts between the ActionFlow.API workflow shape (a flat, ordered list of
// steps) and the editor's tree shape (`{ workflowId, tree, treeRootId }`).
//
// The editor renders a "root" entry marker, one node per step, and a terminal
// "output" (Return) node that carries the workflow's output parameters.
//
// Branching: control-flow / for-loop nodes may own a nested "true" branch. The
// branch head id is stored on the node's `data.branchChildId`, and `children`
// is ordered `[normalNext, branchHead]` so the layout stacks the branch below
// the main line. A for-loop branch serializes to the step's `Steps`; a
// control-flow branch to `Conditions[0].Steps` (with the condition expression).
//
// NOTE: this module is imported by a server component, so it must not pull in
// client-only React components. Node type keys are duplicated here as plain
// strings (they match `NodeTypeKeys` in components/nodes/index.ts).
import type { Node } from "@xyflow/react";
import type { TreeData } from "../../components/tree/layout-elements";
import type { Workflow } from "./Workflow";
import type {
  ApiParameter,
  ApiStep,
  WorkflowDefinitionRequest,
  WorkflowResponse,
} from "../api/client";

const NODE_TYPE = {
  variable: "variable",
  sendHttpCall: "sendHttpCall",
  controlFlow: "controlFlow",
  forLoop: "forLoop",
} as const;

const ROOT_ID = "root";
const OUTPUT_ID = "output";

const ACTION_TYPE_TO_NODE: Record<string, string> = {
  SetVariable: NODE_TYPE.variable,
  Variable: NODE_TYPE.variable,
  SendHttpCall: NODE_TYPE.sendHttpCall,
  ControlFlow: NODE_TYPE.controlFlow,
  ForLoop: NODE_TYPE.forLoop,
};

const NODE_TO_ACTION_TYPE: Record<string, string> = {
  [NODE_TYPE.variable]: "SetVariable",
  [NODE_TYPE.sendHttpCall]: "SendHttpCall",
  [NODE_TYPE.controlFlow]: "ControlFlow",
  [NODE_TYPE.forLoop]: "ForLoop",
};

export function isBranchingType(type?: string): boolean {
  return type === NODE_TYPE.controlFlow || type === NODE_TYPE.forLoop;
}

/** The next node on a node's normal (non-branch) flow. */
export function getNormalNext(node: TreeData): string | undefined {
  const branchId = (node.data as Record<string, unknown> | undefined)
    ?.branchChildId as string | undefined;
  return (node.children ?? []).find((c) => c !== branchId);
}

/** The head of a node's true/nested branch, if any. */
export function getBranchHead(node: TreeData): string | undefined {
  return (node.data as Record<string, unknown> | undefined)?.branchChildId as
    | string
    | undefined;
}

// ---- API response -> editor tree -----------------------------------------

function conditionExpressionOf(properties: Record<string, unknown>): string {
  const conds = properties.Conditions;
  if (Array.isArray(conds)) {
    return (conds[0]?.Expression as string) ?? "";
  }
  // Legacy/simple shape { expressions }.
  return ((conds as { expressions?: string })?.expressions as string) ?? "";
}

function stepToNodeData(step: ApiStep): Record<string, unknown> {
  const p = step.properties ?? {};
  const data: Record<string, unknown> = {
    label: step.name,
    actionType: step.actionType,
  };
  if (step.conditionExpression) data.condition = step.conditionExpression;

  switch (step.actionType) {
    case "SetVariable":
    case "Variable":
      data.variables = p.Variables ?? {};
      break;
    case "SendHttpCall":
      data.url = p.Url ?? "";
      data.method = p.Method ?? "GET";
      data.headers = p.Headers ?? {};
      if (p.Body !== undefined) data.body = p.Body;
      if (p.ResultVariable !== undefined) data.resultVariable = p.ResultVariable;
      break;
    case "ForLoop":
      data.initializerVariable = p.InitializerVariable ?? "";
      data.initialValue = p.InitialValue ?? "";
      data.loopCondition = p.Condition ?? "";
      data.iterator = p.Iterator ?? "";
      break;
    case "ControlFlow":
      data.conditions = { expressions: conditionExpressionOf(p) };
      break;
    default:
      // Unknown action type: keep the raw properties so a later save round-trips.
      data.properties = p;
      break;
  }

  return data;
}

/** Nested steps that should become a node's true branch, if any. */
function branchStepsOf(step: ApiStep): ApiStep[] | null {
  const p = step.properties ?? {};
  if (step.actionType === "ForLoop") {
    return Array.isArray(p.Steps) ? (p.Steps as ApiStep[]) : null;
  }
  if (step.actionType === "ControlFlow") {
    const conds = p.Conditions;
    const first = Array.isArray(conds) ? conds[0] : undefined;
    return Array.isArray(first?.Steps) ? (first!.Steps as ApiStep[]) : null;
  }
  return null;
}

export function responseToWorkflow(res: WorkflowResponse): Workflow {
  const tree: Record<string, TreeData> = {};
  let counter = 0;
  const nextId = () => `step-${counter++}`;

  // Build a chain of nodes and return the id of the first one (or `terminalId`
  // when the chain is empty). Nodes are built back-to-front so each points at
  // the next.
  const buildChain = (
    steps: ApiStep[],
    terminalId: string | undefined,
  ): string | undefined => {
    let firstId = terminalId;

    for (let i = steps.length - 1; i >= 0; i--) {
      const step = steps[i];
      const id = nextId();
      const data = stepToNodeData(step);
      const children: string[] = [];

      const normalNext = firstId;
      if (normalNext) children.push(normalNext);

      const branch = branchStepsOf(step);
      if (branch && branch.length) {
        const branchHead = buildChain(branch, undefined);
        if (branchHead) {
          children.push(branchHead);
          data.branchChildId = branchHead;
        }
      }

      tree[id] = {
        id,
        type: ACTION_TYPE_TO_NODE[step.actionType] ?? NODE_TYPE.variable,
        name: step.name,
        data,
        children,
      };
      firstId = id;
    }

    return firstId;
  };

  const firstMain = buildChain(res.steps, OUTPUT_ID) ?? OUTPUT_ID;

  tree[ROOT_ID] = {
    id: ROOT_ID,
    type: NODE_TYPE.variable,
    name: "root",
    data: {
      label: res.name || "Flow",
      variables: {},
      metadata: res.metadata ?? {},
    },
    children: [firstMain],
  };

  tree[OUTPUT_ID] = {
    id: OUTPUT_ID,
    type: NODE_TYPE.variable,
    name: "output",
    data: {
      label: "Return",
      outputParameters: res.outputParameters ?? [],
    },
  };

  return { workflowId: res.name, tree, treeRootId: ROOT_ID };
}

// ---- editor tree -> API request -------------------------------------------

function nodeDataToProperties(
  actionType: string,
  data: Record<string, unknown>,
): Record<string, unknown> {
  switch (actionType) {
    case "SetVariable":
    case "Variable":
      return { Variables: data.variables ?? {} };
    case "SendHttpCall": {
      const props: Record<string, unknown> = {
        Url: data.url ?? "",
        Method: data.method ?? "GET",
        Headers: data.headers ?? {},
      };
      if (data.body !== undefined) props.Body = data.body;
      if (data.resultVariable !== undefined)
        props.ResultVariable = data.resultVariable;
      return props;
    }
    case "ForLoop":
      // `Steps` is added by the tree walk from the node's branch.
      return {
        InitializerVariable: data.initializerVariable ?? "",
        InitialValue: data.initialValue ?? "",
        Condition: data.loopCondition ?? "",
        Iterator: data.iterator ?? "",
      };
    case "ControlFlow":
      // `Conditions` is added by the tree walk from the node's branch.
      return {};
    default:
      return (data.properties as Record<string, unknown>) ?? {};
  }
}

function nodeToStep(node: TreeData): ApiStep {
  const data = (node.data ?? {}) as Record<string, unknown>;
  const actionType =
    (data.actionType as string) ??
    NODE_TO_ACTION_TYPE[node.type] ??
    "SetVariable";

  const step: ApiStep = {
    name: node.name,
    actionType,
    properties: nodeDataToProperties(actionType, data),
  };
  if (data.condition) step.conditionExpression = data.condition as string;
  return step;
}

export function treeToRequest(
  name: string,
  tree: Record<string | number, TreeData>,
  rootId: string,
): WorkflowDefinitionRequest {
  const root = tree[rootId];
  const metadata =
    ((root?.data as Record<string, unknown> | undefined)
      ?.metadata as Record<string, string>) ?? {};
  let outputParameters: ApiParameter[] = [];

  const visited = new Set<string>();

  // Walk the normal flow from `startId`, recursing into branches.
  const walk = (startId: string | undefined): ApiStep[] => {
    const steps: ApiStep[] = [];
    let currentId = startId;

    while (currentId && tree[currentId] && !visited.has(currentId)) {
      visited.add(currentId);
      const node = tree[currentId];
      const data = (node.data ?? {}) as Record<string, unknown>;

      if (node.name === "output") {
        outputParameters =
          (data.outputParameters as ApiParameter[]) ?? outputParameters;
        break;
      }

      const step = nodeToStep(node);
      const branchId = getBranchHead(node);

      if (step.actionType === "ForLoop") {
        const branchSteps = branchId ? walk(branchId) : [];
        step.properties = { ...step.properties, Steps: branchSteps };
      } else if (step.actionType === "ControlFlow") {
        const branchSteps = branchId ? walk(branchId) : [];
        step.properties = {
          ...step.properties,
          Conditions: [
            {
              Expression:
                (data.conditions as { expressions?: string } | undefined)
                  ?.expressions ?? "",
              Steps: branchSteps,
            },
          ],
        };
      }

      steps.push(step);
      currentId = getNormalNext(node);
    }

    return steps;
  };

  const steps = walk(root?.children?.[0]);
  return { name, steps, outputParameters, metadata };
}

// ---- React Flow nodes -> editor tree map ----------------------------------

/**
 * Rebuild the `{ tree, rootId }` adjacency map from the live React Flow nodes.
 * Each node carries its tree relationships in `data.treeProperties` (stamped by
 * layoutElements); the rest of `data` is the node payload (including any
 * `branchChildId`).
 */
export function nodesToTree(nodes: Node[]): {
  tree: Record<string, TreeData>;
  rootId: string;
} {
  const tree: Record<string, TreeData> = {};
  let rootId = "";

  for (const node of nodes) {
    const treeProps = (node.data?.treeProperties ?? {}) as TreeData & {
      isRoot?: boolean;
    };

    const payload = { ...(node.data as Record<string, unknown>) };
    delete payload.treeProperties;
    delete payload.direction;

    tree[node.id] = {
      id: node.id,
      type: node.type ?? treeProps.type ?? NODE_TYPE.variable,
      name: treeProps.name ?? node.id,
      data: payload,
      children: treeProps.children ? [...treeProps.children] : undefined,
    };

    if (treeProps.isRoot) rootId = node.id;
  }

  return { tree, rootId };
}
