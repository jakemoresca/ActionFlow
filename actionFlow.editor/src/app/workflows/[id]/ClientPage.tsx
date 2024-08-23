"use client";

import Flow from "../../../components/Flow";
import { Workflow } from "./page";

export default async function Workflows(workflow: Workflow) {
  return <Flow workflowId={workflow.workflowId} nodes={workflow.nodes} edges={workflow.edges} />
}
