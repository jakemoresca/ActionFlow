import { NodePropertyDefinition, NodePropertyType } from "@/components/left-pane/NodeProperties";
import { NodeTypeKeys } from "@/components/nodes";

export function getNodePropertyDefinitions(nodeType: string): NodePropertyDefinition[] {
    let propertyDefinitions = getBasePropertyDefinition();
    
    if (nodeType === NodeTypeKeys.variable.type) {
        propertyDefinitions = propertyDefinitions.concat(getVariableNodePropertyDefinition());
    }

    return propertyDefinitions.sort(x => x.index);
}

function getBasePropertyDefinition(): NodePropertyDefinition[] {
    const propertyDefinitions: NodePropertyDefinition[] = [
        {
            propertyName: "label",
            propertyLabel: "Label",
            propertyType: NodePropertyType.TextField,
            index: 0
        },
        {
            propertyName: "condition",
            propertyLabel: "Condition",
            propertyType: NodePropertyType.TextField,
            index: 1
        }
    ]

    return propertyDefinitions;
}

function getVariableNodePropertyDefinition(): NodePropertyDefinition[] {
    const propertyDefinitions: NodePropertyDefinition[] = [
        {
            propertyName: "variables",
            propertyLabel: "Variables",
            propertyType: NodePropertyType.Properties,
            index: 2
        }
    ]

    return propertyDefinitions;
}