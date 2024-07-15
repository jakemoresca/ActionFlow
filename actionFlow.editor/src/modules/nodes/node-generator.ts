import { v4 as uuidv4 } from "uuid";
import { NodeTypeKeys } from "@/components/nodes";
import { VariableNode } from "@/components/nodes/VariableNode";
import { Node } from "@xyflow/react";

export function generateNode(nodeType: string, parentNode: Node): Node {
  if (nodeType === NodeTypeKeys.variable.type) {
    return generateVariableNode(parentNode);
  }

  return generateVariableNode(parentNode);
}

function generateVariableNode(parentNode: Node): VariableNode {
  const data: VariableNode = {
    id: uuidv4(),
    type: "variable",
    position: { x: parentNode.position.x, y: parentNode.position.y + 100 },
    data: {
      label: NodeTypeKeys.variable.name,
      properties: {
        age: "1",
        canWalk: "false"
      },
    },
    selectable: true,
  };

  return data;
}
