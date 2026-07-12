import {
  NodePropertyDefinition,
  NodePropertyType,
} from "../left-pane/NodeProperties";
import { Label, Select, Textarea, TextInput } from "flowbite-react";
import DictionaryEditor from "./dictionary-editor";

export type PropertyFieldData = {
  nodeId: string;
  properties: Record<string, any>;
  propertyDefinition: NodePropertyDefinition;
  // Reports an edit as (propertyName, newValue). propertyName may be a dotted
  // path (e.g. "conditions.expressions").
  onChange?: (propertyName: string, value: unknown) => void;
};

export default function PropertyField({
  nodeId,
  properties,
  propertyDefinition,
  onChange,
}: PropertyFieldData) {
  const value = getValue(properties, propertyDefinition);
  const name = propertyDefinition.propertyName;

  switch (propertyDefinition.propertyType) {
    case NodePropertyType.Label:
      return <Label htmlFor="label">{value}</Label>;

    case NodePropertyType.TextField:
      return (
        <TextInput
          name={name}
          type="text"
          placeholder={propertyDefinition.propertyLabel}
          value={value ?? ""}
          onChange={(e) => onChange?.(name, e.target.value)}
        />
      );

    case NodePropertyType.NumberField:
      return (
        <TextInput
          name={name}
          type="number"
          placeholder={propertyDefinition.propertyLabel}
          value={value ?? ""}
          onChange={(e) => onChange?.(name, e.target.value)}
        />
      );

    case NodePropertyType.Properties:
      return (
        <DictionaryEditor
          key={`${nodeId}_${name}`}
          value={(value ?? {}) as Record<string, string>}
          onChange={(record) => onChange?.(name, record)}
        />
      );

    case NodePropertyType.List:
      return (
        <Select
          name={name}
          value={value ?? ""}
          onChange={(e) => onChange?.(name, e.target.value)}
        >
          {propertyDefinition.propertySources?.map((x, index) => {
            return <option key={`${name}_source_${index}`}>{x}</option>;
          })}
        </Select>
      );

    case NodePropertyType.TextArea:
      return (
        <Textarea
          name={name}
          placeholder={propertyDefinition.propertyLabel}
          rows={4}
          value={value ?? ""}
          onChange={(e) => onChange?.(name, e.target.value)}
        />
      );
  }
}

function getValue(
  properties: Record<string, any>,
  propertyDefinition: NodePropertyDefinition,
) {
  const propertyDefinitionValueSource = propertyDefinition.propertyName;

  if (propertyDefinitionValueSource.includes(".")) {
    const valuePropertyPaths = propertyDefinitionValueSource.split(".");
    let propertySource = properties;
    let value;

    for (let index = 0; index < valuePropertyPaths.length; index++) {
      value = propertySource?.[valuePropertyPaths[index]];
      propertySource = value;
    }

    return value;
  }

  return properties[propertyDefinitionValueSource];
}
