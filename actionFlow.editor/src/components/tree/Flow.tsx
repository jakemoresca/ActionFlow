import { useCallback, useState } from "react";
import {
  Background,
  Controls,
  MiniMap,
  ReactFlow,
  addEdge,
  useNodesState,
  useEdgesState,
  type OnConnect,
  Panel,
  Node,
  Edge,
  getIncomers,
  getOutgoers,
  getConnectedEdges,
  ConnectionLineType,
} from "@xyflow/react";

import "@xyflow/react/dist/style.css";

import { nodeTypes, type CustomNodeType } from "../nodes";
import { edgeTypes, type CustomEdgeType } from "../edges";
import ActionDrawer from "../left-pane/ActionDrawer";
import AddActionModal from "../left-pane/AddActionModal";
import { generateNode } from "@/modules/nodes/node-generator";
import { layoutElements, TreeData } from "./layout-elements";
import { initialTree as defaultInitialTree, treeRootId as defaultTreeRootId } from "./nodes-edges";
import { Workflow } from '@/modules/workflows/Workflow';

export type FlowData = Workflow;

export default function App(data: FlowData) {

  const initialTree = data.tree ?? defaultInitialTree;
  const initialTreeRootId = data.treeRootId ?? defaultTreeRootId;

  const { nodes: layoutedNodes, edges: layoutedEdges } = layoutElements(initialTree, initialTreeRootId, 'LR');

  const [nodes, setNodes, onNodesChange] =
    useNodesState<CustomNodeType>(layoutedNodes);
  const [edges, setEdges, onEdgesChange] =
    useEdgesState<CustomEdgeType>(layoutedEdges);

  const onConnect: OnConnect = useCallback(
    (params) =>
      setEdges((eds) =>
        addEdge(
          { ...params, type: ConnectionLineType.SmoothStep, animated: false },
          eds,
        ),
      ),
    [],
  );

  const [showAddActionModal, setOpenAddActionModal] = useState(false);
  const [selectedNodes, setSelectedNodes] = useState<Node[]>([]);

  const handleAddAction = () => {
    setOpenAddActionModal(true);
  };

  const handleCloseAddActionModal = () => {
    setOpenAddActionModal(false);
  };

  const handleFlowSelectionChange = (params: {
    nodes: Node[];
    edges: Edge[];
  }) => {
    setSelectedNodes(params.nodes);
  };

  const handleDeleteNodes = useCallback(() => {
    const updatedNodes: Record<string | number, TreeData> = {};

    selectedNodes.forEach((selectedNode) => {
      const selectedNodeTreeProperties = selectedNode.data.treeProperties as TreeData
      const incomers = getIncomers(selectedNode, nodes, edges);

      incomers.forEach(incomer => {
        const incomerTreeProperties = incomer.data.treeProperties as TreeData;
        incomerTreeProperties.children = selectedNodeTreeProperties.children;

        updatedNodes[incomer.id] = {
          ...incomer,
          data: incomer.data,
          ...incomerTreeProperties
        };
      });

      nodes.forEach(node => {
        const nodeTreeProperties = (node as Node).data.treeProperties as TreeData

        if (node.id == selectedNode.id) {
          //delete updatedNodes[node.id]
        }
        else {
          updatedNodes[node.id] = {
            ...node,
            data: node.data,
            ...nodeTreeProperties
          }
        }
      })
    });

    const { nodes: layoutedNodes, edges: layoutedEdges } = layoutElements(updatedNodes, initialTreeRootId, 'LR');

    setEdges(layoutedEdges);
    setNodes(layoutedNodes);
  }, [selectedNodes, nodes, edges, setNodes, setEdges]);

  const handleAddNode = useCallback((nodeType: string) => {
    const parentNode = selectedNodes[0];
    const nodeToAdd = generateNode(nodeType, parentNode);

    const parentTreeProperties = parentNode.data.treeProperties as TreeData;
    const hasChildren = !!parentTreeProperties?.children?.length;

    const parentChildTreeProperties: TreeData = {
      id: nodeToAdd.id,
      type: nodeToAdd.type!,
      name: nodeToAdd.data.label as string
    }

    if (hasChildren) {
      parentChildTreeProperties.children = parentTreeProperties.children;
      parentTreeProperties.children = [nodeToAdd.id]
    }
    else {
      parentTreeProperties.children = [nodeToAdd.id];
    }

    const updatedNodes: Record<string | number, TreeData> = {};
    nodes.forEach(node => {
      const nodeTreeProperties = (node as Node).data.treeProperties as TreeData

      if (node.id == parentNode.id) {
        updatedNodes[node.id] = {
          ...parentNode,
          data: parentNode.data,
          ...parentTreeProperties
        };
      }
      else {
        updatedNodes[node.id] = {
          ...node,
          data: node.data,
          ...nodeTreeProperties
        }
      }
    })

    updatedNodes[nodeToAdd.id] = {
      ...nodeToAdd,
      data: nodeToAdd.data,
      ...parentChildTreeProperties
    }

    const { nodes: layoutedNodes, edges: layoutedEdges } = layoutElements(updatedNodes, initialTreeRootId, 'LR');

    setEdges(layoutedEdges);
    setNodes(layoutedNodes);

    setSelectedNodes([nodeToAdd]);
    setOpenAddActionModal(false);

  }, [selectedNodes, nodes, edges, setNodes, setEdges, setSelectedNodes, setOpenAddActionModal])

  return (
    <ReactFlow<CustomNodeType, CustomEdgeType>
      nodes={nodes}
      nodeTypes={nodeTypes}
      onNodesChange={onNodesChange}
      edges={edges}
      edgeTypes={edgeTypes}
      onEdgesChange={onEdgesChange}
      onConnect={onConnect}
      nodesConnectable={true}
      nodesDraggable={true}
      elementsSelectable={true}
      onSelectionChange={handleFlowSelectionChange}
      fitView
      className="bg-white dark:bg-gray-900 antialiased"
    >
      <Background />
      <MiniMap />

      <Panel position="top-left">
        <ActionDrawer
          onAddAction={handleAddAction}
          selectedNodes={selectedNodes}
          onDeleteAction={handleDeleteNodes}
        />
      </Panel>

      <Controls className="left-80" />

      <AddActionModal
        showModal={showAddActionModal}
        onCloseModal={handleCloseAddActionModal}
        onAddNode={handleAddNode}
      />
    </ReactFlow>
  );
}
