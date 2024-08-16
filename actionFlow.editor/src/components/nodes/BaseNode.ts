import type { Node, NodeProps } from '@xyflow/react';

export type BaseNodeData = Record<string, string> & {
  label?: string;
  condition?: string
};

export type BaseNode = Node<BaseNodeData>;