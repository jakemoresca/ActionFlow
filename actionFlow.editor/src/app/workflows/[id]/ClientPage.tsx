"use client";

import Flow from "../../../components/tree/Flow";
import { Workflow } from '@/modules/workflows/Workflow';

export default async function Workflows(workflow: Workflow) {
  return <Flow workflowId={workflow.workflowId} tree={workflow.tree} treeRootId={workflow.treeRootId} />
}
