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

const { Top, Bottom, Left, Right } = Position;

export default function ForLoopNode({ id, data }: NodeProps<ForLoopNode>) {

  const loopString = `For (${data.initializerVariable} = ${data.initialValue}; ${data.loopCondition}; ${data.iterator})`;

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

  return (
    <div className="block max-w-sm p-6 bg-white border border-gray-200 rounded-lg shadow hover:bg-gray-100 dark:bg-gray-800 dark:border-gray-700 dark:hover:bg-gray-700">

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

      <ul role="list" className="text-gray-500 dark:text-gray-400">
        <li className="flex space-x-2 rtl:space-x-reverse items-center">
          <span className="leading-tight text-xs">{loopString}</span>
        </li>
      </ul>
    </div>
  );
}
