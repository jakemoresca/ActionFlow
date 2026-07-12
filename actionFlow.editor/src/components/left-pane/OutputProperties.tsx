import { Node } from "@xyflow/react";
import { Checkbox, Label } from "flowbite-react";
import DictionaryEditor from "../controls/dictionary-editor";

export type OutputParam = { name: string; expression: string };

// Editor for the fixed "Return" node. Behaves like a variable action: the user
// declares output parameters (name = expression) that are evaluated against the
// final execution context, and/or toggles emitting the entire final context.
export default function OutputProperties({
  node,
  onChange,
}: {
  node: Node;
  onChange?: (propertyName: string, value: unknown) => void;
}) {
  const data = (node.data ?? {}) as Record<string, unknown>;
  const params = (data.outputParameters as OutputParam[] | undefined) ?? [];
  const outputAll = data.outputAll === true;

  const record: Record<string, string> = {};
  params.forEach((p) => {
    if (p?.name) record[p.name] = p.expression ?? "";
  });

  return (
    <div className="flex flex-col gap-3">
      <div className="flex items-center gap-2">
        <Checkbox
          id="outputAll"
          checked={outputAll}
          onChange={(e) => onChange?.("outputAll", e.target.checked)}
        />
        <Label htmlFor="outputAll">Output all current properties</Label>
      </div>

      {!outputAll && (
        <div>
          <div className="mb-1 block">
            <Label>Output parameters (name = expression)</Label>
          </div>
          <DictionaryEditor
            key={`${node.id}_outputs`}
            value={record}
            onChange={(rec) =>
              onChange?.(
                "outputParameters",
                Object.entries(rec).map(([name, expression]) => ({
                  name,
                  expression,
                })),
              )
            }
          />
        </div>
      )}

      <p className="text-xs text-gray-500 dark:text-gray-400">
        Expressions are evaluated against the final context (e.g.{" "}
        <code>age</code>, <code>result.Body</code>).
      </p>
    </div>
  );
}
