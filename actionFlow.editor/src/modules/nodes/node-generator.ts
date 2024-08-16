import { v4 as uuidv4 } from "uuid";
import { NodeTypeKeys } from "@/components/nodes";
import { VariableNodeData } from "@/components/nodes/VariableNode";
import { Node } from "@xyflow/react";

export function generateNode(nodeType: string, parentNode: Node): Node {
  if (nodeType === NodeTypeKeys.variable.type) {
    return generateVariableNode(parentNode);
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
    type: "variable",
    position: { x: parentNode.position.x, y: parentNode.position.y + 100 },
    data: nodeData,
    selectable: true,
  };

  return data;
}
