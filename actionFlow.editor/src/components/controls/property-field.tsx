import { ChangeEvent } from "react";
import { NodePropertyDefinition, NodePropertyType } from "../left-pane/NodeProperties";
import { Label, TextInput } from "flowbite-react";
import TableProperties from "./table-properties";
import { getNodeColumnDefinition, toTableProperties } from "@/modules/nodes/node-column-definition-provider";

export type PropertyFieldData = {
    nodeType: string
    properties: Record<string, any>;
    propertyDefinition: NodePropertyDefinition
    handlePropertyChange?: (event: ChangeEvent<HTMLInputElement>) => void
  };
  
  export default function PropertyField({ nodeType, properties, propertyDefinition, handlePropertyChange }: PropertyFieldData) {

    switch(propertyDefinition.propertyType) {
        case NodePropertyType.Label:
            return <Label htmlFor="label" value={properties[propertyDefinition.propertyName]} />

        case NodePropertyType.TextField:
            return <TextInput name={propertyDefinition.propertyName} type="text" placeholder={propertyDefinition.propertyLabel} value={properties[propertyDefinition.propertyName]} onChange={handlePropertyChange} />

        case NodePropertyType.NumberField:
            return <TextInput name={propertyDefinition.propertyName} type="number" placeholder={propertyDefinition.propertyLabel} value={properties[propertyDefinition.propertyName]} onChange={handlePropertyChange} />

        case NodePropertyType.Properties:
            const nodeColumnDefinitions = getNodeColumnDefinition(nodeType, propertyDefinition.propertyName);
            const tableProperties = toTableProperties(properties, "variables");
            return <TableProperties columnDefinitions={nodeColumnDefinitions} properties={tableProperties} handlePropertyChange={handlePropertyChange} />
    }
  }
  