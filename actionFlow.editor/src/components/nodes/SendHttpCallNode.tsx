import type { Node, NodeProps } from "@xyflow/react";
import { Handle, Position } from "@xyflow/react";
import { NodeBase } from "@xyflow/system";
import ConditionSection from "./ConditionSection";
import { BaseNodeData } from "./BaseNode";

export type SendHttpCallNodeData = BaseNodeData & {
  headers?: Record<string, string>;
  url?: string;
  method?: string;
  body?: string;
  resultVariable?: string;
};
export type SendHttpCallNode = NodeBase & Node<SendHttpCallNodeData>;

export default function VariableNode({ data }: NodeProps<SendHttpCallNode>) {
  return (
    <div className="block max-w-sm p-6 bg-white border border-gray-200 rounded-lg shadow hover:bg-gray-100 dark:bg-gray-800 dark:border-gray-700 dark:hover:bg-gray-700">
      <Handle type="target" position={Position.Top} />
      <h5 className="text-xs font-bold dark:text-white">{data.label}</h5>

      <ConditionSection condition={data.condition} />
      <ul role="list" className="text-gray-500 dark:text-gray-400">
        <li className="flex space-x-2 rtl:space-x-reverse items-center">
          <span className="leading-tight text-xs">{data.url}</span>
        </li>
      </ul>

      <Handle type="source" position={Position.Bottom} />
    </div>
  );
}
