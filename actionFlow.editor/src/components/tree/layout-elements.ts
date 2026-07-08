import { Position } from "@xyflow/react";
import { layoutFromMap } from "entitree-flex";
import type { Node, Edge } from "@xyflow/react";

const nodeWidth = 150;
const nodeHeight = 80;

const entitreeSettings = {
  clone: true, // returns a copy of the input, if your application does not allow editing the original object
  enableFlex: true, // has slightly better perfomance if turned off (node.width, node.height will not be read)
  firstDegreeSpacing: 100, // spacing in px between nodes belonging to the same source, eg children with same parent
  nextAfterAccessor: "spouses", // the side node prop used to go sideways, AFTER the current node
  nextAfterSpacing: 100, // the spacing of the "side" nodes AFTER the current node
  nextBeforeAccessor: "siblings", // the side node prop used to go sideways, BEFORE the current node
  nextBeforeSpacing: 100, // the spacing of the "side" nodes BEFORE the current node
  nodeHeight, // default node height in px
  nodeWidth, // default node width in px
  orientation: "vertical", // "vertical" to see parents top and children bottom, "horizontal" to see parents left and
  rootX: 0, // set root position if other than 0
  rootY: 0, // set root position if other than 0
  secondDegreeSpacing: 100, // spacing in px between nodes not belonging to same parent eg "cousin" nodes
  sourcesAccessor: "parents", // the prop used as the array of ancestors ids
  sourceTargetSpacing: 100, // the "vertical" spacing between nodes in vertical orientation, horizontal otherwise
  targetsAccessor: "children", // the prop used as the array of children ids
};

const { Top, Bottom, Left, Right } = Position;

export type TreeData = {
  id: string;
  isSpouse?: boolean;
  isSibling?: boolean;
  isRoot?: boolean;
  type: string;
  name: string;
  data?: Record<string, unknown>
  children?: string[];
  spouses?: string[];
  siblings?: string[]
};

export const layoutElements = (
  tree: Record<string | number, TreeData>,
  rootId: string,
  direction = "TB"
) => {
  const isTreeHorizontal = direction === "LR";

  const { nodes: entitreeNodes, rels: entitreeEdges } = layoutFromMap<TreeData>(
    rootId,
    tree,
    {
      ...entitreeSettings,
      orientation: isTreeHorizontal
        ? "horizontal"
        : "vertical",
    }
  );

  const nodes: Node[] = [],
    edges: Edge[] = [];

  entitreeEdges.forEach((edge) => {
    const sourceNode = edge.source.id;
    const targetNode = edge.target.id;

    const newEdge: Edge = {
      id: "e" + sourceNode + targetNode,
      source: sourceNode,
      target: targetNode,
      type: "smoothstep",
    };

    // Check if target node is spouse or sibling
    const isTargetSpouse = !!edge.target.isSpouse;
    const isTargetSibling = !!edge.target.isSibling;

    if (isTargetSpouse) {
      newEdge.sourceHandle = isTreeHorizontal ? Bottom : Right;
      newEdge.targetHandle = isTreeHorizontal ? Top : Left;
    } else if (isTargetSibling) {
      newEdge.sourceHandle = isTreeHorizontal ? Top : Left;
      newEdge.targetHandle = isTreeHorizontal ? Bottom : Right;
    } else {
      newEdge.sourceHandle = isTreeHorizontal ? Right : Bottom;
      newEdge.targetHandle = isTreeHorizontal ? Left : Top;
    }

    // Label / colour the branches of control-flow and for-loop nodes: the true
    // (nested) branch is green, the normal continuation is muted.
    const sourceTreeNode = tree[sourceNode];
    const branchChildId = (sourceTreeNode?.data as Record<string, unknown> | undefined)
      ?.branchChildId as string | undefined;
    const isBranchingSource =
      sourceTreeNode?.type === "controlFlow" ||
      sourceTreeNode?.type === "forLoop";

    if (branchChildId && branchChildId === targetNode) {
      newEdge.label = sourceTreeNode?.type === "forLoop" ? "loop" : "true";
      newEdge.style = { stroke: "#16a34a", strokeWidth: 2 };
      newEdge.labelStyle = { fill: "#15803d", fontWeight: 600, fontSize: 11 };
      newEdge.labelBgStyle = { fill: "#dcfce7" };
      newEdge.labelBgPadding = [6, 2];
      newEdge.labelBgBorderRadius = 4;
      newEdge.animated = true;
    } else if (isBranchingSource) {
      newEdge.label = sourceTreeNode?.type === "forLoop" ? "after" : "false";
      newEdge.style = { stroke: "#9ca3af" };
      newEdge.labelStyle = { fill: "#6b7280", fontWeight: 600, fontSize: 11 };
      newEdge.labelBgStyle = { fill: "#f3f4f6" };
      newEdge.labelBgPadding = [6, 2];
      newEdge.labelBgBorderRadius = 4;
    }

    edges.push(newEdge);
  });

  entitreeNodes.forEach((node) => {
    const isSpouse = !!node?.isSpouse;
    const isSibling = !!node?.isSibling;
    const isRoot = node?.id === rootId;

    const newNode: Node = {
      id: node.id,
      data: { direction, ...node.data, 
        treeProperties: {
          ...node!,
          isRoot
        }
      },
      position: {
        x: node.x,
        y: node.y,
      },
      width: nodeWidth,
      height: nodeHeight,
      type: node.type
    };

    if (isSpouse) {
      newNode.sourcePosition = isTreeHorizontal ? Bottom : Right;
      newNode.targetPosition = isTreeHorizontal ? Top : Left;
    } else if (isSibling) {
      newNode.sourcePosition = isTreeHorizontal ? Top : Left;
      newNode.targetPosition = isTreeHorizontal ? Bottom : Right;
    } else {
      newNode.sourcePosition = isTreeHorizontal ? Right : Bottom;
      newNode.targetPosition = isTreeHorizontal ? Left : Top;
    }

    nodes.push(newNode);
  });

  return { nodes, edges };
};
