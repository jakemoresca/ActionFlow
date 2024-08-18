import type { BuiltInNode, Node, NodeTypes } from '@xyflow/react';

import PositionLoggerNode, {
  type PositionLoggerNode as PositionLoggerNodeType,
} from './PositionLoggerNode';

import VariableNode, {
  type VariableNode as VariableNodeType,
} from './VariableNode';

import SendHttpCallNode, {
  type SendHttpCallNode as SendHttpCallNodeType,
} from './SendHttpCallNode';

export const initialNodes = [
  {
    id: 'a',
    type: 'input',
    position: { x: 0, y: 0 },
    data: { label: 'Event Trigger' },
    deletable: false
  },
  {
    id: 'b',
    type: 'variable',
    position: { x: 0, y: 100 },
    data: { 
      label: 'Initialize Variable',
      variables: {
        "age": "1",
        "canWalk": "true"
      }
    },
    selectable: true,
  },
  {
    id: 'c',
    type: 'variable',
    position: { x: 0, y: 200 },
    data: { label: 'Test variable value', condition: "age == 1 \u0026\u0026 canWalk == true" },
    selectable: true,
  },
  {
    id: 'd',
    type: 'output',
    position: { x: 0, y: 300 },
    data: { label: 'Return' },
  },
] satisfies Node[];

export const NodeTypeKeys = {
  "variable": {
    "type": "variable",
    "name": "Set Variable"
  },
  "sendHttpCall": {
    "type": "sendHttpCall",
    "name": "Send Http Call"
  }
}

export const nodeTypes = {
  'position-logger': PositionLoggerNode,
  [NodeTypeKeys.variable.type]: VariableNode,
  [NodeTypeKeys.sendHttpCall.type]: SendHttpCallNode
  // Add any of your custom nodes here!
} satisfies NodeTypes;

// Append the types of you custom edges to the BuiltInNode type
export type CustomNodeType = BuiltInNode | PositionLoggerNodeType | VariableNodeType;
