import type { Node, NodeProps } from "@xyflow/react";
import { Handle, Position } from "@xyflow/react";
import { NodeBase } from '@xyflow/system';
import ConditionSection from "./ConditionSection";
import { BaseNodeData } from "./BaseNode";

export type VariableNodeData = BaseNodeData & {
  variables?: Record<string, string>
}
export type VariableNode = NodeBase & Node<VariableNodeData>;

const { Top, Bottom, Left, Right } = Position;

export default function VariableNode({
  id, data
}: NodeProps<VariableNode>) {

  const isTreeHorizontal = data.direction === 'LR';

  const getTargetPosition = () => {
    if (data.isSpouse) {
      return isTreeHorizontal ? Top : Left;
    } else if (data.isSibling) {
      return isTreeHorizontal ? Bottom : Right;
    }
    return isTreeHorizontal ? Left : Top;
  };

  const treeProperties = data?.treeProperties;
  const isRootNode = treeProperties?.isRoot;
  const hasChildren = !!treeProperties?.children?.length;
  const hasSiblings = !!treeProperties?.siblings?.length;
  const hasSpouses = !!treeProperties?.spouses?.length;


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
    <div className="nodrag block max-w-sm p-6 bg-white border border-gray-200 rounded-lg shadow hover:bg-gray-100 dark:bg-gray-800 dark:border-gray-700 dark:hover:bg-gray-700">

      {hasChildren && (
        <Handle
          type="source"
          position={isTreeHorizontal ? Right : Bottom}
          id={isTreeHorizontal ? Right : Bottom}
        />
      )}

      {/* For spouses */}
      {hasSpouses && (
        <Handle
          type="source"
          position={isTreeHorizontal ? Bottom : Right}
          id={isTreeHorizontal ? Bottom : Right}
        />
      )}

      {/* For siblings */}
      {hasSiblings && (
        <Handle
          type="source"
          position={isTreeHorizontal ? Top : Left}
          id={isTreeHorizontal ? Top : Left}
        />
      )}

      {/* Target Handle */}
      {!isRootNode && (
        <Handle
          type={"target"}
          position={getTargetPosition()}
          id={getTargetPosition()}
        />
      )}

      <h5 className="text-xs font-bold dark:text-white">{data.label}</h5>

      <ConditionSection condition={data.condition} />
      {renderProperties(data)}
    </div>
  );
}
