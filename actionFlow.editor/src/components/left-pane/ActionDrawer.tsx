import {
  Drawer,
  DrawerHeader,
  DrawerItems,
  Button,
  ButtonGroup,
  Accordion,
  AccordionPanel,
  AccordionTitle,
  AccordionContent,
  ListGroup,
  ListGroupItem,
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
      <DrawerHeader title="Action Flow Editor" />
      <DrawerItems>
        <Accordion>
          <AccordionPanel>
            <AccordionTitle>Workflows</AccordionTitle>
            <AccordionContent>
              <ListGroup className="w-100">
                <Link href="/workflows/1">
                  <ListGroupItem active>Profile</ListGroupItem>
                </Link>
                <Link href="/workflows/2">
                  <ListGroupItem>Settings</ListGroupItem>
                </Link>
                <Link href="/workflows/3">
                  <ListGroupItem>Messages</ListGroupItem>
                </Link>
                <Link href="/workflows/4">
                  <ListGroupItem>Download</ListGroupItem>
                </Link>
              </ListGroup>
            </AccordionContent>
          </AccordionPanel>
          <AccordionPanel>
            <AccordionTitle>Action Properties</AccordionTitle>
            <AccordionContent>
              <ButtonGroup>
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
              </ButtonGroup>
              <div className="mb-6 mt-5">{createPropetiesSection()}</div>
            </AccordionContent>
          </AccordionPanel>
        </Accordion>
      </DrawerItems>
    </Drawer>
  );
}
