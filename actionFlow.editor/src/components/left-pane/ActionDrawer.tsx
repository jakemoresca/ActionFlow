import {
  Drawer,
  Button,
  Label,
  Accordion,
  AccordionPanel,
  ListGroup,
} from "flowbite-react";
import { Node } from "@xyflow/react";
import NodeProperties from "./NodeProperties";
import { NodeTypeKeys } from "../nodes";
import Link from "next/link";

export type ActionDrawerData = {
  onAddAction?: () => void;
  onDeleteAction?: () => void;
  selectedNodes?: Node[];
};

export default function ActionDrawer({
  onAddAction: addAction,
  onDeleteAction,
  selectedNodes,
}: ActionDrawerData) {
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

  const createPropetiesSection = () => {
    const selectedNode = selectedNodes && selectedNodes[0];

    if (!selectedNode) return <></>;

    return <NodeProperties node={selectedNode}></NodeProperties>;
  };

  return (
    <Drawer open={true} onClose={() => {}} backdrop={false}>
      <Drawer.Header title="Action Flow Editor" />
      <Drawer.Items>
        <Accordion>
          <AccordionPanel>
            <Accordion.Title>Workflows</Accordion.Title>
            <Accordion.Content>
              <ListGroup className="w-100">
                <Link href="/workflows/1">
                  <ListGroup.Item active>Profile</ListGroup.Item>
                </Link>
                <Link href="/workflows/2">
                  <ListGroup.Item>Settings</ListGroup.Item>
                </Link>
                <Link href="/workflows/3">
                  <ListGroup.Item>Messages</ListGroup.Item>
                </Link>
                <Link href="/workflows/4">
                  <ListGroup.Item>Download</ListGroup.Item>
                </Link>
              </ListGroup>
            </Accordion.Content>
          </AccordionPanel>
          <AccordionPanel>
            <Accordion.Title>Action Properties</Accordion.Title>
            <Accordion.Content>
              <Button.Group>
                <Button
                  color="gray"
                  disabled={!canAdd}
                  onClick={() => addAction && addAction()}
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
              </Button.Group>
              <div className="mb-6 mt-5">{createPropetiesSection()}</div>
            </Accordion.Content>
          </AccordionPanel>
        </Accordion>
      </Drawer.Items>
    </Drawer>
  );
}
