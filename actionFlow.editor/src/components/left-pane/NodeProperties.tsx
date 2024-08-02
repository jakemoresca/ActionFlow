import { Node } from "@xyflow/react";
import { BaseNodeData } from "../nodes/BaseNode";
import { getNodeColumnDefinition, toTableProperties } from "@/modules/nodes/node-column-definition-provider";
import TableProperties from "../controls/table-properties";
import { Label, TextInput } from "flowbite-react";
import { ChangeEvent } from "react";

export type NodePropertiesData = {
    node: Node
}

export default function NodeProperties({ node }: NodePropertiesData) {

    const data = node.data as BaseNodeData;
    const nodeColumnDefinitions = getNodeColumnDefinition(node.type!)
    const tableProperties = toTableProperties(node.data as BaseNodeData);

    const label = data.label == null ? "" : data.label;
    const condition = data.condition == null ? "" : data.condition;

    const handleChange = (event: ChangeEvent<HTMLInputElement>) => {

    }

    return (
        <>
            <form className="flex max-w-md flex-col gap-4">
                <div>
                    <div className="mb-2 block">
                        <Label htmlFor="label" value="Label" />
                    </div>
                    <TextInput id="label" type="text" placeholder="Label" value={label} onChange={handleChange} />
                </div>
                <div>
                    <div className="mb-2 block">
                        <Label htmlFor="condition" value="Condition" />
                    </div>
                    <TextInput id="condition" type="text" placeholder="Condition" value={condition} onChange={handleChange} />
                </div>
            </form>

            <Label htmlFor="title" className="mb-2 block mt-4">
                Properties
            </Label>
            <TableProperties columnDefinitions={nodeColumnDefinitions} properties={tableProperties} handlePropertyChange={handleChange} />
        </>
    );
}