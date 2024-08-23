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
} from "@xyflow/react";

import "@xyflow/react/dist/style.css";

import { nodeTypes, type CustomNodeType } from "./nodes";
import { edgeTypes, type CustomEdgeType } from "./edges";
import ActionDrawer from "./left-pane/ActionDrawer";
import AddActionModal from "./left-pane/AddActionModal";
import { generateNode } from "@/modules/nodes/node-generator";
import { Workflow } from "@/app/workflows/[id]/page";

export type FlowData = Workflow;

export default function App(data: FlowData) {
  const [nodes, setNodes, onNodesChange] =
    useNodesState<CustomNodeType>(data.nodes);
  const [edges, setEdges, onEdgesChange] =
    useEdgesState<CustomEdgeType>(data.edges);
  const onConnect: OnConnect = useCallback(
    (connection) => setEdges((edges) => addEdge(connection, edges)),
    [setEdges]
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
    selectedNodes.forEach((selectedNode) => {
      setEdges((edges) => {
        const incomers = getIncomers(selectedNode, nodes, edges);
        const outgoers = getOutgoers(selectedNode, nodes, edges);
        const connectedEdges = getConnectedEdges([selectedNode], edges);

        const remainingEdges = edges.filter(
          (edge) => !connectedEdges.includes(edge)
        );

        const createdEdges = incomers.flatMap(({ id: source }) =>
          outgoers.map(({ id: target }) => ({
            id: `${source}->${target}`,
            source,
            target,
          }))
        );

        return [...remainingEdges, ...createdEdges];
      });
    });

    setNodes((nodes) =>
      nodes.filter(
        (node) =>
          selectedNodes.find((selected) => selected.id == node.id) == undefined
      )
    );
  }, [selectedNodes, nodes, edges, setNodes, setEdges]);

  const handleAddNode = useCallback((nodeType: string) => {
    const parentNode = selectedNodes[0];
    const nodeToAdd = generateNode(nodeType, parentNode);

    setNodes((nds) => nds.concat(nodeToAdd));

    setEdges((edges) => {
      const connectedEdges = getConnectedEdges([parentNode], edges);

      const remainingEdges = edges.map(
        (edge) => connectedEdges.includes(edge) && edge.source === parentNode.id ? {
          ...edge,
          id: `${nodeToAdd.id}->${edge.target}`,
          source: nodeToAdd.id
        } : edge
      );

      const createdEdges = [{ id: `${parentNode.id}->${nodeToAdd.id}`, source: parentNode.id, target: nodeToAdd.id, animated: false }]

      return [...remainingEdges, ...createdEdges];
    });

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
