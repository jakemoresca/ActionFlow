import ClientPage from "./ClientPage";
import type { Node, Edge } from '@xyflow/react';

export type WorkflowsParam = {
  id: string;
};

export type Workflow = {
  workflowId: string,
  nodes: Node[]
  edges: Edge[]
}

export default async function Workflows({ params }: {
  params: WorkflowsParam;
}) {
  const workflow: Workflow = {
    workflowId: params.id,
    nodes: [
      {
        id: "a",
        type: "input",
        position: { x: 0, y: 0 },
        data: { label: "Event Trigger" },
        deletable: false,
      },
      {
        id: "b",
        type: "variable",
        position: { x: 0, y: 100 },
        data: {
          label: "Initialize Variable",
          variables: {
            age: "1",
            canWalk: "true",
          },
        },
        selectable: true,
      },
      {
        id: "c",
        type: "variable",
        position: { x: 0, y: 200 },
        data: {
          label: "Test variable value",
          condition: "age == 1 \u0026\u0026 canWalk == true",
        },
        selectable: true,
      },
      {
        id: "d",
        type: "output",
        position: { x: 0, y: 300 },
        data: { label: "Return" },
      },
    ],
    edges: [
      { id: "a->b", source: "a", target: "b", animated: false, label: "event" },
      { id: "b->c", source: "b", target: "c", animated: false },
      { id: "c->d", source: "c", target: "d", animated: false },
    ],
  };

  return (
    <ClientPage {...workflow} />
  );
}