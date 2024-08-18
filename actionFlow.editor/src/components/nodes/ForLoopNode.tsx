import type { Node, NodeProps } from "@xyflow/react";
import { Handle, Position } from "@xyflow/react";
import { NodeBase } from "@xyflow/system";
import ConditionSection from "./ConditionSection";
import { BaseNodeData } from "./BaseNode";

export type ForLoopNodeData = BaseNodeData & {
  initializerVariable?: string;
  initialValue?: string;
  loopCondition?: string;
  iterator?: string;
};

export type ForLoopNode = NodeBase & Node<ForLoopNodeData>;

export default function ForLoopNode({ id, data }: NodeProps<ForLoopNode>) {

  const loopString = `For (${data.initializerVariable} = ${data.initialValue}; ${data.loopCondition}; ${data.iterator})`;

  return (
    <div className="block max-w-sm p-6 bg-white border border-gray-200 rounded-lg shadow hover:bg-gray-100 dark:bg-gray-800 dark:border-gray-700 dark:hover:bg-gray-700">
      <Handle type="target" position={Position.Top} id={`node_${id}_top`} />
      <h5 className="text-xs font-bold dark:text-white">{data.label}</h5>

      <ConditionSection condition={data.condition} />
      
      <ul role="list" className="text-gray-500 dark:text-gray-400">
        <li className="flex space-x-2 rtl:space-x-reverse items-center">
          <span className="leading-tight text-xs">{loopString}</span>
        </li>
      </ul>

      <Handle type="source" position={Position.Bottom} id={`node_${id}_bottom`} />
      <Handle type="source" position={Position.Right} id={`node_${id}_right`} />
    </div>
  );
}
