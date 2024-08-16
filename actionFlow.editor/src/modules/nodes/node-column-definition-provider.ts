import { TablePropertiesColumnDefinition } from "@/components/controls/table-properties";
import { NodePropertyType } from "@/components/left-pane/NodeProperties";
import { NodeTypeKeys } from "@/components/nodes";

export function getNodeColumnDefinition(nodeType: string, propertyName: string): TablePropertiesColumnDefinition[] {
    if (nodeType === NodeTypeKeys.variable.type && propertyName == "variables") {
        return getVariableNodeColumnDefinition();
    }

    return [];
}

function getVariableNodeColumnDefinition(): TablePropertiesColumnDefinition[] {
    const columnDefinitions: TablePropertiesColumnDefinition[] = [
        {
            name: "Name",
            type: NodePropertyType.TextField,
            index: 0,
            field: "key"
        },
        {
            name: "Value",
            type: NodePropertyType.TextField,
            index: 1,
            field: "value"
        }
    ]

    return columnDefinitions;
}

export function toTableProperties(data: Record<string, any>, propertyKey: string): Record<string, string>[] {
    var nodeProperties: Record<string, string>[] = [];

    for (const key in data[propertyKey]) {
        nodeProperties.push({
            "key": key,
            "value": data[propertyKey][key]
        })
    }

    return nodeProperties;
}