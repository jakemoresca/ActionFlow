import { ChangeEvent } from "react";
import { NodePropertyDefinition, NodePropertyType } from "../left-pane/NodeProperties";
import { Label, Select, Textarea, TextInput } from "flowbite-react";
import TableProperties from "./table-properties";
import { getNodeColumnDefinition, toTableProperties } from "@/modules/nodes/node-column-definition-provider";

export type PropertyFieldData = {
    nodeType: string
    properties: Record<string, any>;
    propertyDefinition: NodePropertyDefinition
    handlePropertyChange?: (event: ChangeEvent) => void
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
            const tableProperties = toTableProperties(properties, propertyDefinition.propertyName);
            return <TableProperties columnDefinitions={nodeColumnDefinitions} properties={tableProperties} handlePropertyChange={handlePropertyChange} />

        case NodePropertyType.List:
            return (<Select name={propertyDefinition.propertyName} onChange={handlePropertyChange} >
                {propertyDefinition.propertySources?.map(x => {
                    return (<option>{x}</option>)
                })}
            </Select>)

        case NodePropertyType.TextArea:
            return <Textarea name={propertyDefinition.propertyName} placeholder={propertyDefinition.propertyLabel} rows={4} />
    }
  }
  