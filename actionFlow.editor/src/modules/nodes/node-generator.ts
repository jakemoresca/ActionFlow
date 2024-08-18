import { v4 as uuidv4 } from "uuid";
import { NodeTypeKeys } from "@/components/nodes";
import { VariableNodeData } from "@/components/nodes/VariableNode";
import { Node } from "@xyflow/react";
import { SendHttpCallNodeData } from "@/components/nodes/SendHttpCallNode";
import { ControlFlowNodeData } from "@/components/nodes/ControlFlowNode";

export function generateNode(nodeType: string, parentNode: Node): Node {
  if (nodeType === NodeTypeKeys.variable.type) {
    return generateVariableNode(parentNode);
  }
  else if (nodeType === NodeTypeKeys.sendHttpCall.type) {
    return generateSendHttpCallNode(parentNode);
  }
  else if (nodeType === NodeTypeKeys.controlFlow.type) {
    return generateControlFlowNode(parentNode);
  }

  return generateVariableNode(parentNode);
}

function generateVariableNode(parentNode: Node): Node {
  const nodeData: VariableNodeData = {
    label: NodeTypeKeys.variable.name
  }

  nodeData.variables = {
    age: "1",
    canWalk: "false"
  }

  const data: Node = {
    id: uuidv4(),
    type: NodeTypeKeys.variable.type,
    position: { x: parentNode.position.x, y: parentNode.position.y + 100 },
    data: nodeData,
    selectable: true,
  };

  return data;
}

function generateSendHttpCallNode(parentNode: Node): Node {
  const nodeData: SendHttpCallNodeData = {
    label: NodeTypeKeys.sendHttpCall.name,
    url: "http://localhost",
    method: "GET"
  }

  nodeData.headers = {
  }

  const data: Node = {
    id: uuidv4(),
    type: NodeTypeKeys.sendHttpCall.type,
    position: { x: parentNode.position.x, y: parentNode.position.y + 100 },
    data: nodeData,
    selectable: true,
  };

  return data;
}

function generateControlFlowNode(parentNode: Node): Node {
  const nodeData: ControlFlowNodeData = {
    label: NodeTypeKeys.controlFlow.name
  }

  nodeData.conditions = {
    expressions: ""
  }

  const data: Node = {
    id: uuidv4(),
    type: NodeTypeKeys.controlFlow.type,
    position: { x: parentNode.position.x, y: parentNode.position.y + 100 },
    data: nodeData,
    selectable: true,
  };

  return data;
}
