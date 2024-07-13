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
} from "@xyflow/react";

import "@xyflow/react/dist/style.css";

import { initialNodes, nodeTypes, type CustomNodeType } from "./nodes";
import { initialEdges, edgeTypes, type CustomEdgeType } from "./edges";
import { Button, Drawer, Modal } from "flowbite-react";
import ActionDrawer from "./left-pane/ActionDrawer";
import AddActionModal from "./left-pane/AddActionModal";

export default function App() {
  const [nodes, , onNodesChange] = useNodesState<CustomNodeType>(initialNodes);
  const [edges, setEdges, onEdgesChange] =
    useEdgesState<CustomEdgeType>(initialEdges);
  const onConnect: OnConnect = useCallback(
    (connection) => setEdges((edges) => addEdge(connection, edges)),
    [setEdges]
  );

  const [showAddActionModal, setOpenAddActionModal] = useState(false);

  const handleAddAction = () => {
    setOpenAddActionModal(true);
  }

  const handleCloseAddActionModal = () => {
    setOpenAddActionModal(false);
  }

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
      fitView
      className="bg-white dark:bg-gray-900 antialiased"
    >
      <Background />
      <MiniMap />

      <Panel position="top-left">
        <ActionDrawer addAction={handleAddAction} />
      </Panel>

      <Controls className="left-80" />

      <AddActionModal showModal={showAddActionModal} closeModal={handleCloseAddActionModal} />

    </ReactFlow>
  );
}
