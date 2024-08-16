import type { Node, NodeProps } from "@xyflow/react";
import { Handle, Position } from "@xyflow/react";
import { NodeBase } from '@xyflow/system';
import ConditionSection from "./ConditionSection";
import { BaseNode, BaseNodeData } from "./BaseNode";

export type VariableNodeData = BaseNodeData & {
  variables?: Record<string, string>
}
export type VariableNode = NodeBase & Node<VariableNodeData>;

export default function VariableNode({
  data
}: NodeProps<VariableNode>) {

  const renderProperties = (data: VariableNodeData) => {
    var variables: React.ReactNode[] = [];

    if (data.variables == null)
      return variables;

    for (const key in data.variables) {
      variables.push(<li key={`variable_1_${key}`} className="flex space-x-2 rtl:space-x-reverse items-center">
        <span className="leading-tight text-xs">{key} = {data.variables[key]}</span>
      </li>)
    }

    var list = (
      <>
        <ul role="list" className="text-gray-500 dark:text-gray-400">
          {variables}
        </ul>
      </>)

    return list;
  }

  return (
    // We add this class to use the same styles as React Flow's default nodes.
    <div className="block max-w-sm p-6 bg-white border border-gray-200 rounded-lg shadow hover:bg-gray-100 dark:bg-gray-800 dark:border-gray-700 dark:hover:bg-gray-700">
      <Handle type="target" position={Position.Top} />
      <h5 className="text-xs font-bold dark:text-white">{data.label}</h5>

      <ConditionSection condition={data.condition} />
      {renderProperties(data)}

      <Handle type="source" position={Position.Bottom} />
    </div>
  );
}
