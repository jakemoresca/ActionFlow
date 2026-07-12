import { Workflow } from "@/modules/workflows/Workflow";
import { getWorkflow } from "@/modules/api/client";
import { responseToWorkflow } from "@/modules/workflows/mapper";
import ClientPage from "./ClientPage";

export const dynamic = "force-dynamic";

export type WorkflowsParam = {
  id: string;
};

export default async function Workflows({
  params,
}: {
  params: Promise<WorkflowsParam>;
}) {
  const { id } = await params;
  const name = decodeURIComponent(id);

  const response = await getWorkflow(name);
  const workflow: Workflow = response
    ? responseToWorkflow(response)
    : { workflowId: name };

  return <ClientPage workflow={workflow} isNew={!response} />;
}
