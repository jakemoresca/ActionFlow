import { useCallback, useState } from "react";
import { useRouter } from "next/navigation";
import {
  Background,
  Controls,
  MiniMap,
  ReactFlow,
  addEdge,
  useNodesState,
  useEdgesState,
  type OnConnect,
  Node,
  Edge,
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
import { getBranchHead, nodesToTree, treeToRequest } from "@/modules/workflows/mapper";
import {
  deleteWorkflow,
  testWorkflow,
  updateWorkflow,
  type TestRunResponse,
} from "@/modules/api/client";

export type FlowData = Workflow & { isNew?: boolean };

// Immutably set a (possibly dotted) path on a node data object.
function setDeep(
  source: Record<string, unknown>,
  path: string,
  value: unknown,
): Record<string, unknown> {
  const keys = path.split(".");
  const root = { ...source };
  let cursor: Record<string, unknown> = root;
  for (let i = 0; i < keys.length - 1; i++) {
    cursor[keys[i]] = { ...((cursor[keys[i]] as Record<string, unknown>) ?? {}) };
    cursor = cursor[keys[i]] as Record<string, unknown>;
  }
  cursor[keys[keys.length - 1]] = value;
  return root;
}

// Render a synchronous test-run result as a readable block for the Test dialog.
function formatTestResult(result: TestRunResponse): string {
  if (!result.success) {
    return `Run failed:\n${result.error ?? "Unknown error"}`;
  }

  return ["Output:", JSON.stringify(result.output ?? {}, null, 2)].join("\n");
}

export default function App(data: FlowData) {
  const router = useRouter();

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
  // Whether the next added action goes on the selected node's normal flow or
  // into its true/nested branch (control-flow / for-loop only).
  const [addTarget, setAddTarget] = useState<"normal" | "branch">("normal");

  const handleAddAction = (target: "normal" | "branch" = "normal") => {
    setAddTarget(target);
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
    // Rebuild the tree from live node data (preserving edits), then unlink the
    // selected nodes and relink around them.
    const { tree, rootId } = nodesToTree(nodes);

    selectedNodes.forEach((selectedNode) => {
      const deleted = tree[selectedNode.id];
      if (!deleted) return;
      // The entry marker and the Return terminal are structural — keep them.
      if (deleted.name === "root" || deleted.name === "output") return;

      const branchId = getBranchHead(deleted);
      const replacement = (deleted.children ?? []).find((c) => c !== branchId);

      Object.values(tree).forEach((node) => {
        const data = (node.data ?? {}) as Record<string, unknown>;
        if (data.branchChildId === selectedNode.id) {
          if (replacement) data.branchChildId = replacement;
          else delete data.branchChildId;
        }
        if (node.children) {
          const relinked = node.children
            .map((c) => (c === selectedNode.id ? replacement : c))
            .filter((c): c is string => !!c);
          node.children = [...new Set(relinked)];
        }
      });

      delete tree[selectedNode.id];
    });

    const { nodes: layoutedNodes, edges: layoutedEdges } = layoutElements(
      tree,
      rootId || initialTreeRootId,
      "LR",
    );

    setEdges(layoutedEdges);
    setNodes(layoutedNodes);
  }, [selectedNodes, nodes, setNodes, setEdges, initialTreeRootId]);

  const handleAddNode = useCallback(
    (nodeType: string) => {
      const parentSelection = selectedNodes[0];
      if (!parentSelection) return;

      // Rebuild the tree from live node data so existing edits are preserved.
      const { tree, rootId } = nodesToTree(nodes);
      const parent = tree[parentSelection.id];
      if (!parent) return;

      const generated = generateNode(nodeType, parentSelection);
      const newId = generated.id;

      const newNode: TreeData = {
        id: newId,
        type: generated.type!,
        name: (generated.data.label as string) ?? nodeType,
        data: { ...(generated.data as Record<string, unknown>) },
        children: [],
      };

      const parentData = (parent.data ?? {}) as Record<string, unknown>;
      const existingBranch = parentData.branchChildId as string | undefined;

      if (addTarget === "branch") {
        // Insert at the head of the parent's true branch.
        if (existingBranch) newNode.children = [existingBranch];
        parent.data = { ...parentData, branchChildId: newId };
        const normalNext = (parent.children ?? []).find(
          (c) => c !== existingBranch,
        );
        parent.children = [normalNext, newId].filter(
          (c): c is string => !!c,
        );
      } else {
        // Insert on the parent's normal flow, pushing the old next down.
        const normalNext = (parent.children ?? []).find(
          (c) => c !== existingBranch,
        );
        if (normalNext) newNode.children = [normalNext];
        parent.children = [newId, existingBranch].filter(
          (c): c is string => !!c,
        );
      }

      tree[newId] = newNode;

      const { nodes: layoutedNodes, edges: layoutedEdges } = layoutElements(
        tree,
        rootId || initialTreeRootId,
        "LR",
      );

      setEdges(layoutedEdges);
      setNodes(layoutedNodes);

      setSelectedNodes([{ ...generated }]);
      setOpenAddActionModal(false);
    },
    [selectedNodes, nodes, addTarget, setNodes, setEdges, initialTreeRootId],
  );

  const handleNodeDataChange = useCallback(
    (nodeId: string, propertyName: string, value: unknown) => {
      setNodes((nds) =>
        nds.map((node) => {
          if (node.id !== nodeId) return node;

          let newData = setDeep(
            node.data as Record<string, unknown>,
            propertyName,
            value,
          );

          // Keep the tree step name in sync with the editable label so saves
          // reflect the rename.
          if (propertyName === "label") {
            const treeProperties = {
              ...((newData.treeProperties as Record<string, unknown>) ?? {}),
              name: value,
            };
            newData = { ...newData, treeProperties };
          }

          return { ...node, data: newData } as CustomNodeType;
        }),
      );
    },
    [setNodes],
  );

  const [saving, setSaving] = useState(false);

  // Serialize the live tree and persist it. Returns the saved name, or null if
  // the user cancelled the name prompt. Does not navigate.
  const persistCurrentWorkflow = useCallback(async (): Promise<string | null> => {
    const name =
      data.workflowId || window.prompt("Workflow name")?.trim() || "";
    if (!name) return null;

    const { tree, rootId } = nodesToTree(nodes);
    const request = treeToRequest(name, tree, rootId || initialTreeRootId);
    await updateWorkflow(name, request);
    return name;
  }, [data.workflowId, nodes, initialTreeRootId]);

  const handleSaveWorkflow = useCallback(async () => {
    setSaving(true);
    try {
      const name = await persistCurrentWorkflow();
      if (!name) return;
      // Navigate to the (possibly new) workflow so its id is reflected in the URL.
      router.push(`/workflows/${encodeURIComponent(name)}`);
      router.refresh();
    } catch (err) {
      window.alert(`Failed to save workflow: ${(err as Error).message}`);
    } finally {
      setSaving(false);
    }
  }, [persistCurrentWorkflow, router]);

  // Save the current workflow, execute it with the given inputs, and poll for
  // the result. Returns a human-readable result string for the Test dialog.
  const handleRunWorkflow = useCallback(
    async (inputs: Record<string, string>): Promise<string> => {
      let name: string | null;
      try {
        name = await persistCurrentWorkflow();
      } catch (err) {
        return `Failed to save workflow before running:\n${(err as Error).message}`;
      }
      if (!name) return "Run cancelled — a workflow name is required.";

      try {
        const result = await testWorkflow(name, inputs);
        return formatTestResult(result);
      } catch (err) {
        return `Test run failed:\n${(err as Error).message}\n\nIs the API running?`;
      }
    },
    [persistCurrentWorkflow],
  );

  const handleDeleteWorkflow = useCallback(async () => {
    if (!data.workflowId) return;
    if (!window.confirm(`Delete workflow "${data.workflowId}"? This removes all versions.`))
      return;

    try {
      await deleteWorkflow(data.workflowId);
      router.push("/");
      router.refresh();
    } catch (err) {
      window.alert(`Failed to delete workflow: ${(err as Error).message}`);
    }
  }, [data.workflowId, router]);

  // Drive the properties panel from live node state (not the selection
  // snapshot) so edits are reflected immediately.
  const selectedNodeId = selectedNodes[0]?.id;
  const propertiesNode = selectedNodeId
    ? nodes.find((n) => n.id === selectedNodeId)
    : undefined;

  return (
    <div className="flex h-screen w-screen overflow-hidden">
      <aside className="flex h-full w-80 shrink-0 flex-col border-r border-gray-200 bg-white dark:border-gray-700 dark:bg-gray-800">
        <ActionDrawer
          workflowName={data.workflowId}
          onAddAction={handleAddAction}
          selectedNodes={selectedNodes}
          propertiesNode={propertiesNode}
          onNodeDataChange={handleNodeDataChange}
          onDeleteAction={handleDeleteNodes}
          onSaveWorkflow={handleSaveWorkflow}
          onDeleteWorkflow={handleDeleteWorkflow}
          onRunWorkflow={handleRunWorkflow}
          saving={saving}
        />
      </aside>

      <div className="h-full flex-1">
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
          <Controls />

          <AddActionModal
            showModal={showAddActionModal}
            onCloseModal={handleCloseAddActionModal}
            onAddNode={handleAddNode}
          />
        </ReactFlow>
      </div>
    </div>
  );
}
