import { Node } from "@xyflow/react";
import { BaseNodeData } from "../nodes/BaseNode";
import { Label } from "flowbite-react";
import { getNodePropertyDefinitions } from "@/modules/nodes/node-properties-definition-provider";
import PropertyField from "../controls/property-field";

export type NodePropertiesData = {
  node: Node;
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

export default function NodeProperties({ node }: NodePropertiesData) {
  const data = node.data as BaseNodeData;
  const nodePropertyDefinitions = getNodePropertyDefinitions(node.type!);

  const handleChange = () => {};

  const createNodePropertyFields = () => {
    const fields = nodePropertyDefinitions.map((propertyDefinition) => {
      const field = (
        <div key={`node_${node.id}_${propertyDefinition.propertyName}_${propertyDefinition.index}`}>
          <div className="mb-2 block">
            <Label value={propertyDefinition.propertyLabel} />
          </div>
          <PropertyField
            nodeType={node.type!}
            properties={data}
            propertyDefinition={propertyDefinition}
            handlePropertyChange={handleChange}
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
