import { NodePropertyDefinition, NodePropertyType } from "@/components/left-pane/NodeProperties";
import { NodeTypeKeys } from "@/components/nodes";

export function getNodePropertyDefinitions(nodeType: string): NodePropertyDefinition[] {
    let propertyDefinitions = getBasePropertyDefinition();
    
    if (nodeType === NodeTypeKeys.variable.type) {
        propertyDefinitions = propertyDefinitions.concat(getVariableNodePropertyDefinition());
    }
    else if (nodeType === NodeTypeKeys.sendHttpCall.type) {
        propertyDefinitions = propertyDefinitions.concat(getSendHttpCallPropertyDefinition());
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

function getSendHttpCallPropertyDefinition(): NodePropertyDefinition[] {
    const propertyDefinitions: NodePropertyDefinition[] = [
        {
            propertyName: "url",
            propertyLabel: "Url",
            propertyType: NodePropertyType.TextField,
            index: 2
        },
        {
            propertyName: "method",
            propertyLabel: "Method",
            propertyType: NodePropertyType.List,
            propertySources: ["GET", "POST", "PUT", "DELETE"],
            index: 3
        },
        {
            propertyName: "headers",
            propertyLabel: "Headers",
            propertyType: NodePropertyType.Properties,
            index: 4
        },
        {
            propertyName: "body",
            propertyLabel: "Body",
            propertyType: NodePropertyType.TextArea,
            index: 5
        },
        {
            propertyName: "resultVariable",
            propertyLabel: "Result Variable",
            propertyType: NodePropertyType.TextField,
            index: 6
        }
    ]

    return propertyDefinitions;
}