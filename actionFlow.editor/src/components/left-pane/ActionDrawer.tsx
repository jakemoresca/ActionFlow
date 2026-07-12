import { useEffect, useState } from "react";
import {
  Button,
  ButtonGroup,
  Accordion,
  AccordionPanel,
  AccordionTitle,
  AccordionContent,
  ListGroup,
  ListGroupItem,
  Spinner,
} from "flowbite-react";
import { Node } from "@xyflow/react";
import { useRouter } from "next/navigation";
import Link from "next/link";
import NodeProperties from "./NodeProperties";
import TestWorkflowModal from "./TestWorkflowModal";
import { NodeTypeKeys } from "../nodes";
import {
  createWorkflow,
  listWorkflows,
  type WorkflowSummary,
} from "@/modules/api/client";

export type ActionDrawerData = {
  workflowName?: string;
  onAddAction?: (target?: "normal" | "branch") => void;
  onDeleteAction?: () => void;
  onSaveWorkflow?: () => void;
  onDeleteWorkflow?: () => void;
  onRunWorkflow?: (inputs: Record<string, string>) => Promise<string>;
  selectedNodes?: Node[];
  propertiesNode?: Node;
  onNodeDataChange?: (
    nodeId: string,
    propertyName: string,
    value: unknown,
  ) => void;
  saving?: boolean;
};

export default function ActionDrawer({
  workflowName,
  onAddAction: addAction,
  onDeleteAction,
  onSaveWorkflow,
  onDeleteWorkflow,
  onRunWorkflow,
  selectedNodes,
  propertiesNode,
  onNodeDataChange,
  saving,
}: ActionDrawerData) {
  const router = useRouter();
  const [workflows, setWorkflows] = useState<WorkflowSummary[]>([]);
  const [loadingList, setLoadingList] = useState(true);
  const [showTestModal, setShowTestModal] = useState(false);

  useEffect(() => {
    let active = true;
    listWorkflows()
      .then((items) => active && setWorkflows(items))
      .finally(() => active && setLoadingList(false));
    return () => {
      active = false;
    };
  }, [workflowName]);

  const validNodeTypesToDelete = [
    NodeTypeKeys.variable.type,
    NodeTypeKeys.sendHttpCall.type,
    NodeTypeKeys.controlFlow.type,
    NodeTypeKeys.forLoop.type,
  ];

  const validNodeTypesToAddTo = [
    "input",
    NodeTypeKeys.variable.type,
    NodeTypeKeys.sendHttpCall.type,
    NodeTypeKeys.controlFlow.type,
    NodeTypeKeys.forLoop.type,
  ];

  const canDelete =
    selectedNodes &&
    selectedNodes.length > 0 &&
    selectedNodes?.every((x) => {
      return x.type && validNodeTypesToDelete.includes(x.type);
    });

  const canAdd =
    selectedNodes &&
    selectedNodes.length == 1 &&
    selectedNodes[0].type &&
    validNodeTypesToAddTo.includes(selectedNodes[0].type);

  // Control-flow / for-loop nodes can host a nested "true" branch.
  const canAddBranch =
    selectedNodes?.length === 1 &&
    (selectedNodes[0].type === NodeTypeKeys.controlFlow.type ||
      selectedNodes[0].type === NodeTypeKeys.forLoop.type);

  const handleNewWorkflow = async () => {
    const name = window.prompt("New workflow name")?.trim();
    if (!name) return;
    try {
      await createWorkflow({ name, steps: [] });
      router.push(`/workflows/${encodeURIComponent(name)}`);
      router.refresh();
    } catch (err) {
      window.alert(`Failed to create workflow: ${(err as Error).message}`);
    }
  };

  const createPropetiesSection = () => {
    if (!propertiesNode) {
      return (
        <p className="text-sm text-gray-500 dark:text-gray-400">
          Select a node to edit its properties.
        </p>
      );
    }

    return (
      <NodeProperties
        node={propertiesNode}
        onChange={(propertyName, value) =>
          onNodeDataChange?.(propertiesNode.id, propertyName, value)
        }
      />
    );
  };

  return (
    <div className="flex h-full flex-col">
      <div className="border-b border-gray-200 px-4 py-3 dark:border-gray-700">
        <h2 className="text-lg font-semibold text-gray-900 dark:text-white">
          Action Flow Editor
        </h2>
        {workflowName ? (
          <p className="truncate text-xs text-gray-500 dark:text-gray-400">
            {workflowName}
          </p>
        ) : (
          <p className="text-xs text-gray-400 dark:text-gray-500">
            Unsaved workflow
          </p>
        )}
      </div>

      <div className="flex-1 overflow-y-auto">
        <Accordion>
          <AccordionPanel>
            <AccordionTitle>Workflows</AccordionTitle>
            <AccordionContent>
              <div className="mb-3 flex justify-end">
                <Button size="xs" color="gray" onClick={handleNewWorkflow}>
                  New
                </Button>
              </div>
              {loadingList ? (
                <div className="flex justify-center py-4">
                  <Spinner size="sm" />
                </div>
              ) : workflows.length === 0 ? (
                <p className="py-2 text-sm text-gray-500 dark:text-gray-400">
                  No workflows yet.
                </p>
              ) : (
                <ListGroup>
                  {workflows.map((wf) => (
                    <Link
                      key={wf.name}
                      href={`/workflows/${encodeURIComponent(wf.name)}`}
                    >
                      <ListGroupItem active={wf.name === workflowName}>
                        <span className="flex w-full items-center justify-between gap-2">
                          <span className="truncate">{wf.name}</span>
                          <span className="text-xs text-gray-400">
                            v{wf.latestVersion}
                          </span>
                        </span>
                      </ListGroupItem>
                    </Link>
                  ))}
                </ListGroup>
              )}
            </AccordionContent>
          </AccordionPanel>
          <AccordionPanel>
            <AccordionTitle>Action Properties</AccordionTitle>
            <AccordionContent>
              <ButtonGroup>
                <Button
                  color="gray"
                  disabled={!canAdd}
                  onClick={() => addAction && addAction("normal")}
                >
                  Add
                </Button>
                <Button
                  color="gray"
                  disabled={!canDelete}
                  onClick={() => onDeleteAction && onDeleteAction()}
                >
                  Delete
                </Button>
              </ButtonGroup>
              {canAddBranch && (
                <div className="mt-2">
                  <Button
                    size="xs"
                    color="green"
                    onClick={() => addAction && addAction("branch")}
                  >
                    + Add nested action (true branch)
                  </Button>
                </div>
              )}
              <div className="mb-6 mt-5">{createPropetiesSection()}</div>
            </AccordionContent>
          </AccordionPanel>
        </Accordion>
      </div>

      <div className="flex flex-col gap-2 border-t border-gray-200 p-3 dark:border-gray-700">
        <Button
          color="green"
          onClick={() => setShowTestModal(true)}
          disabled={!onRunWorkflow}
        >
          Test
        </Button>
        <div className="flex gap-2">
          <Button
            className="flex-1"
            disabled={saving}
            onClick={() => onSaveWorkflow && onSaveWorkflow()}
          >
            {saving ? "Saving…" : "Save"}
          </Button>
          <Button
            color="red"
            disabled={!workflowName}
            onClick={() => onDeleteWorkflow && onDeleteWorkflow()}
          >
            Delete
          </Button>
        </div>
      </div>

      <TestWorkflowModal
        show={showTestModal}
        workflowName={workflowName}
        onClose={() => setShowTestModal(false)}
        onRun={(inputs) =>
          onRunWorkflow
            ? onRunWorkflow(inputs)
            : Promise.resolve("Run is unavailable.")
        }
      />
    </div>
  );
}
