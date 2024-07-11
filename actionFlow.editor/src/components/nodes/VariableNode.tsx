import type { Node, NodeProps } from "@xyflow/react";
import { Handle, Position } from "@xyflow/react";
import ConditionSection from "./ConditionSection";

export type VariableNodeData = {
  properties?: Map<string, string>
  label?: string;
  condition?: string
};

export type VariableNode = Node<VariableNodeData>;

export default function VariableNode({
  data,
}: NodeProps<VariableNode>) {

  const renderProperties = (data: VariableNodeData) => {
    var properties: React.ReactNode[] = [];

    if(data.properties == null)
      return properties;

    for (const key in data.properties) {
      properties.push(<div key={`variable_1_${key}`}>{key} = {data.properties[key]} </div>)
    }

    return properties;
  }

  return (
    // We add this class to use the same styles as React Flow's default nodes.
    <div className="react-flow__node-default">
      <Handle type="target" position={Position.Top} />
      <div>{data.label}</div>

      <ConditionSection condition={data.condition} />
      {renderProperties(data)}

      <Handle type="source" position={Position.Bottom} />
    </div>
  );
}
