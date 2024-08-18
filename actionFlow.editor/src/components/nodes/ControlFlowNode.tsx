import type { Node, NodeProps } from "@xyflow/react";
import { Handle, Position } from "@xyflow/react";
import { NodeBase } from "@xyflow/system";
import ConditionSection from "./ConditionSection";
import { BaseNodeData } from "./BaseNode";

export type ControlFlowNodeData = BaseNodeData & {
  conditions?: ControlFlowConditionsData;
};

export type ControlFlowConditionsData = {
  expressions?: string;
}

export type ControlFlowNode = NodeBase & Node<ControlFlowNodeData>;

export default function ControlFlowNode({ id, data }: NodeProps<ControlFlowNode>) {
  return (
    <div className="block max-w-sm p-6 bg-white border border-gray-200 rounded-lg shadow hover:bg-gray-100 dark:bg-gray-800 dark:border-gray-700 dark:hover:bg-gray-700">
      <Handle type="target" position={Position.Top} id={`node_${id}_top`} />
      <h5 className="text-xs font-bold dark:text-white">{data.label}</h5>

      <ConditionSection condition={data.conditions?.expressions} />

      <Handle type="source" position={Position.Bottom} id={`node_${id}_bottom`} />
      <Handle type="source" position={Position.Right} id={`node_${id}_right`} />
    </div>
  );
}
