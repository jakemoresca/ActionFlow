import type { Node, NodeProps } from '@xyflow/react';
import { TreeData } from '../tree/layout-elements';

export type BaseNodeData = Record<string, string> & {
  label?: string;
  condition?: string;
  treeProperties?: TreeData
};

export type BaseNode = Node<BaseNodeData>;