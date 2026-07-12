import { Node } from "@xyflow/react";
import { BaseNodeData } from "../nodes/BaseNode";
import { Label } from "flowbite-react";
import { getNodePropertyDefinitions } from "@/modules/nodes/node-properties-definition-provider";
import PropertyField from "../controls/property-field";
import OutputProperties from "./OutputProperties";

export type NodePropertiesData = {
  node: Node;
  onChange?: (propertyName: string, value: unknown) => void;
};

export type NodePropertyDefinition = {
  propertyName: string;
  propertyType: NodePropertyType;
  propertyLabel: string;
  index: number;
  propertySources?: string[]
};

export enum NodePropertyType {
  Label,
  TextField,
  NumberField,
  Properties,
  List,
  TextArea,
}

export default function NodeProperties({ node, onChange }: NodePropertiesData) {
  const data = node.data as BaseNodeData;

  // The fixed terminal / entry nodes get dedicated treatment.
  const treeName = (node.data?.treeProperties as { name?: string } | undefined)
    ?.name;
  if (treeName === "output") {
    return <OutputProperties node={node} onChange={onChange} />;
  }
  if (treeName === "root") {
    return (
      <p className="text-sm text-gray-500 dark:text-gray-400">
        Workflow entry point — no editable properties.
      </p>
    );
  }

  const nodePropertyDefinitions = getNodePropertyDefinitions(node.type!);

  const createNodePropertyFields = () => {
    const fields = nodePropertyDefinitions.map((propertyDefinition) => {
      const field = (
        <div key={`node_${node.id}_${propertyDefinition.propertyName}_${propertyDefinition.index}`}>
          <div className="mb-2 block">
            <Label>{propertyDefinition.propertyLabel}</Label>
          </div>
          <PropertyField
            nodeId={node.id}
            properties={data}
            propertyDefinition={propertyDefinition}
            onChange={onChange}
          />
        </div>
      );

      return field;
    });

    return fields;
  };

  return (
    <>
      <form className="flex max-w-md flex-col gap-4">
        { createNodePropertyFields() }
      </form>
    </>
  );
}
