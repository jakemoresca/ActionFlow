import { useState } from "react";
import {
  Button,
  Label,
  Modal,
  ModalBody,
  ModalFooter,
  ModalHeader,
  Textarea,
} from "flowbite-react";
import DictionaryEditor from "../controls/dictionary-editor";

export type TestWorkflowModalData = {
  show: boolean;
  workflowName?: string;
  onClose: () => void;
  onRun: (inputs: Record<string, string>) => Promise<string>;
};

// Postman-style test runner: enter an initial context (key = expression), run
// the workflow through the engine, and show the returned output.
export default function TestWorkflowModal({
  show,
  workflowName,
  onClose,
  onRun,
}: TestWorkflowModalData) {
  const [inputs, setInputs] = useState<Record<string, string>>({});
  const [result, setResult] = useState("");
  const [running, setRunning] = useState(false);

  const handleRun = async () => {
    setRunning(true);
    setResult("Running…");
    try {
      setResult(await onRun(inputs));
    } finally {
      setRunning(false);
    }
  };

  return (
    <Modal show={show} onClose={onClose} size="lg" dismissible>
      <ModalHeader>
        Test workflow{workflowName ? `: ${workflowName}` : ""}
      </ModalHeader>
      <ModalBody>
        <div className="flex flex-col gap-4">
          <div>
            <div className="mb-1 block">
              <Label>Initial context</Label>
            </div>
            <p className="mb-2 text-xs text-gray-500 dark:text-gray-400">
              Each value is an engine expression, e.g. <code>5</code>,{" "}
              <code>true</code>, <code>&quot;hello&quot;</code>. Saves the
              current workflow before running.
            </p>
            <DictionaryEditor value={inputs} onChange={setInputs} />
          </div>

          <div>
            <Button onClick={handleRun} disabled={running}>
              {running ? "Running…" : "Run"}
            </Button>
          </div>

          <div>
            <div className="mb-1 block">
              <Label>Result</Label>
            </div>
            <Textarea
              readOnly
              rows={12}
              className="font-mono text-xs"
              placeholder="Run the workflow to see its output here."
              value={result}
            />
          </div>
        </div>
      </ModalBody>
      <ModalFooter>
        <Button color="gray" onClick={onClose}>
          Close
        </Button>
      </ModalFooter>
    </Modal>
  );
}
