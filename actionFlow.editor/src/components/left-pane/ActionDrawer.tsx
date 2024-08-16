import { Drawer, Button, Label } from "flowbite-react"
import { Node } from "@xyflow/react";
import { getNodeColumnDefinition, toTableProperties } from "@/modules/nodes/node-column-definition-provider";
import { BaseNodeData } from "../nodes/BaseNode";
import TableProperties from "../controls/table-properties";
import NodeProperties from "./NodeProperties";

export type ActionDrawerData = {
  onAddAction?: () => void;
  onDeleteAction?: () => void;
  selectedNodes?: Node[];
}

export default function ActionDrawer({ onAddAction: addAction, onDeleteAction, selectedNodes }: ActionDrawerData) {

  const validNodeTypesToDelete = [
    "variable"
  ]

  const validNodeTypesToAddTo = [
    "input",
    "variable"
  ]

  const canDelete = selectedNodes && selectedNodes.length > 0 && selectedNodes?.every(x => {
    return x.type && validNodeTypesToDelete.includes(x.type);
  })

  const canAdd = selectedNodes && selectedNodes.length == 1 && selectedNodes[0].type && validNodeTypesToAddTo.includes(selectedNodes[0].type);

  const createPropetiesSection = () => {
    const selectedNode = selectedNodes && selectedNodes[0]

    if (!selectedNode) return (<></>)

    return (<NodeProperties node={selectedNode}></NodeProperties>)
  }

  return (
    <Drawer open={true} onClose={() => { }} backdrop={false}>
      <Drawer.Header title="Action Flow Editor" />
      <Drawer.Items>
        <Button.Group>
          <Button color="gray" disabled={!canAdd} onClick={() => addAction && addAction()}>Add</Button>
          <Button color="gray" disabled={!canDelete} onClick={() => onDeleteAction && onDeleteAction()}>Delete</Button>
        </Button.Group>
        <div className="mb-6 mt-5">
          {createPropetiesSection()}
        </div>
      </Drawer.Items>
    </Drawer>
  )
}