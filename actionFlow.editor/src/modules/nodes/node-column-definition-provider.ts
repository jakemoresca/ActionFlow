import { TablePropertiesColumnDefinition, TablePropertiesColumnType } from "@/components/controls/table-properties";
import { NodeTypeKeys } from "@/components/nodes";
import { BaseNode, BaseNodeData } from "@/components/nodes/BaseNode";
import type { Node, NodeProps } from '@xyflow/react';

export function getNodeColumnDefinition(nodeType: string): TablePropertiesColumnDefinition[] {
    if (nodeType === NodeTypeKeys.variable.type) {
        return getVariableNodeColumnDefinition();
    }

    return getVariableNodeColumnDefinition();
}

function getVariableNodeColumnDefinition(): TablePropertiesColumnDefinition[] {
    const columnDefinitions: TablePropertiesColumnDefinition[] = [
        {
            name: "Name",
            type: TablePropertiesColumnType.TextField,
            index: 0,
            field: "key"
        },
        {
            name: "Value",
            type: TablePropertiesColumnType.TextField,
            index: 1,
            field: "value"
        }
    ]

    return columnDefinitions;
}

export function toTableProperties(data: BaseNodeData): Record<string, string>[] {
    var nodeProperties: Record<string, string>[] = [];

    for (const key in data.properties) {
        nodeProperties.push({
            "key": key,
            "value": data.properties[key]
        })
    }

    return nodeProperties;
}