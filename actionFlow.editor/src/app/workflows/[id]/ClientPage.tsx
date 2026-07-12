"use client";

import Flow from "../../../components/tree/Flow";
import { Workflow } from "@/modules/workflows/Workflow";

export default function ClientPage({
  workflow,
  isNew,
}: {
  workflow: Workflow;
  isNew: boolean;
}) {
  return (
    <Flow
      workflowId={workflow.workflowId}
      tree={workflow.tree}
      treeRootId={workflow.treeRootId}
      isNew={isNew}
    />
  );
}
