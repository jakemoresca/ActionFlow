import { TreeData } from '../../components/tree/layout-elements';


export type Workflow = {
  workflowId: string;
  tree?: Record<string | number, TreeData>;
  treeRootId?: string;
};
