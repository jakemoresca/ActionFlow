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

import ControlFlowNode, {
  type ControlFlowNode as ControlFlowNodeType,
} from './ControlFlowNode';

import ForLoopNode, {
  type ForLoopNode as ForLoopNodeType,
} from './ForLoopNode';

export const NodeTypeKeys = {
  "variable": {
    "type": "variable",
    "name": "Set Variable"
  },
  "sendHttpCall": {
    "type": "sendHttpCall",
    "name": "Send Http Call"
  },
  "controlFlow": {
    "type": "controlFlow",
    "name": "Control Flow"
  },
  "forLoop": {
    "type": "forLoop",
    "name": "For Loop"
  }
}

export const nodeTypes = {
  'position-logger': PositionLoggerNode,
  [NodeTypeKeys.variable.type]: VariableNode,
  [NodeTypeKeys.sendHttpCall.type]: SendHttpCallNode,
  [NodeTypeKeys.controlFlow.type]: ControlFlowNode,
  [NodeTypeKeys.forLoop.type]: ForLoopNode
  // Add any of your custom nodes here!
} satisfies NodeTypes;

// Append the types of you custom edges to the BuiltInNode type
export type CustomNodeType = BuiltInNode | PositionLoggerNodeType | VariableNodeType | SendHttpCallNodeType | ControlFlowNodeType | ForLoopNodeType
