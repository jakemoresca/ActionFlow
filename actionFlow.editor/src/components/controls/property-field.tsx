import { ChangeEvent } from "react";
import {
  NodePropertyDefinition,
  NodePropertyType,
} from "../left-pane/NodeProperties";
import { Label, Select, Textarea, TextInput } from "flowbite-react";
import TableProperties from "./table-properties";
import {
  getNodeColumnDefinition,
  toTableProperties,
} from "@/modules/nodes/node-column-definition-provider";

export type PropertyFieldData = {
  nodeType: string;
  properties: Record<string, any>;
  propertyDefinition: NodePropertyDefinition;
  handlePropertyChange?: (event: ChangeEvent) => void;
};

export default function PropertyField({
  nodeType,
  properties,
  propertyDefinition,
  handlePropertyChange,
}: PropertyFieldData) {

  const value = getValue(properties, propertyDefinition);

  switch (propertyDefinition.propertyType) {
    case NodePropertyType.Label:
      return (
        <Label
          htmlFor="label"
          value={value}
        />
      );

    case NodePropertyType.TextField:
      return (
        <TextInput
          name={propertyDefinition.propertyName}
          type="text"
          placeholder={propertyDefinition.propertyLabel}
          value={value}
          onChange={handlePropertyChange}
        />
      );

    case NodePropertyType.NumberField:
      return (
        <TextInput
          name={propertyDefinition.propertyName}
          type="number"
          placeholder={propertyDefinition.propertyLabel}
          value={value}
          onChange={handlePropertyChange}
        />
      );

    case NodePropertyType.Properties:
      const nodeColumnDefinitions = getNodeColumnDefinition(
        nodeType,
        propertyDefinition.propertyName
      );
      const tableProperties = toTableProperties(
        properties,
        propertyDefinition.propertyName
      );
      return (
        <TableProperties
          columnDefinitions={nodeColumnDefinitions}
          properties={tableProperties}
          handlePropertyChange={handlePropertyChange}
        />
      );

    case NodePropertyType.List:
      return (
        <Select
          name={propertyDefinition.propertyName}
          onChange={handlePropertyChange}
          value={value}
        >
          {propertyDefinition.propertySources?.map((x, index) => {
            return (
              <option
                key={`${propertyDefinition.propertyName}_source_${index}`}
              >
                {x}
              </option>
            );
          })}
        </Select>
      );

    case NodePropertyType.TextArea:
      return (
        <Textarea
          name={propertyDefinition.propertyName}
          placeholder={propertyDefinition.propertyLabel}
          rows={4}
          value={value}
        />
      );
  }
}

function getValue(properties: Record<string, any>, propertyDefinition: NodePropertyDefinition) {
    const propertyDefinitionValueSource = propertyDefinition.propertyName;

    if(propertyDefinitionValueSource.includes(".")) {
        const valuePropertyPaths = propertyDefinitionValueSource.split(".");
        let propertySource = properties;
        let value;

        for (let index = 0; index < valuePropertyPaths.length; index++) {
            value = propertySource[valuePropertyPaths[index]];
            propertySource = value;
        }

        return value;
    }

    return properties[propertyDefinitionValueSource];
}
