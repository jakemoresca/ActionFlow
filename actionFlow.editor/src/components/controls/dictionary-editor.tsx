import { useState } from "react";
import { Button, TextInput } from "flowbite-react";

export type DictionaryEditorData = {
  value?: Record<string, string>;
  onChange?: (value: Record<string, string>) => void;
};

type Row = { key: string; value: string };

// Editable key/value dictionary (used for Set Variable `variables` and
// Send Http Call `headers`). Rows are kept in local state so editing a key
// doesn't reorder/remount inputs mid-keystroke; the parent receives the
// rebuilt record on every change. Remount (reset) happens via a `key` prop on
// the element when a different node is selected.
export default function DictionaryEditor({
  value,
  onChange,
}: DictionaryEditorData) {
  const [rows, setRows] = useState<Row[]>(() =>
    Object.entries(value ?? {}).map(([key, val]) => ({
      key,
      value: String(val ?? ""),
    })),
  );

  const commit = (next: Row[]) => {
    setRows(next);
    const record: Record<string, string> = {};
    next.forEach((row) => {
      if (row.key.trim() !== "") record[row.key] = row.value;
    });
    onChange?.(record);
  };

  const updateRow = (index: number, patch: Partial<Row>) => {
    commit(rows.map((row, i) => (i === index ? { ...row, ...patch } : row)));
  };

  const removeRow = (index: number) => {
    commit(rows.filter((_, i) => i !== index));
  };

  const addRow = () => {
    commit([...rows, { key: "", value: "" }]);
  };

  return (
    <div className="mt-2 flex flex-col gap-2">
      {rows.map((row, index) => (
        <div key={index} className="flex items-center gap-2">
          <TextInput
            className="flex-1"
            sizing="sm"
            placeholder="Name"
            value={row.key}
            onChange={(e) => updateRow(index, { key: e.target.value })}
          />
          <TextInput
            className="flex-1"
            sizing="sm"
            placeholder="Value"
            value={row.value}
            onChange={(e) => updateRow(index, { value: e.target.value })}
          />
          <Button
            size="xs"
            color="gray"
            onClick={() => removeRow(index)}
            aria-label="Remove"
          >
            ✕
          </Button>
        </div>
      ))}
      <div>
        <Button size="xs" color="gray" onClick={addRow}>
          Add
        </Button>
      </div>
    </div>
  );
}
