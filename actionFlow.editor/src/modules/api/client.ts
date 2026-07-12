// Typed client for the ActionFlow.API workflow endpoints.
//
// Base URL comes from NEXT_PUBLIC_ACTIONFLOW_API (injected by the Aspire
// AppHost) and falls back to the standalone `http` launch profile port.
export const API_BASE =
  process.env.NEXT_PUBLIC_ACTIONFLOW_API?.replace(/\/$/, "") ??
  "http://localhost:5200";

// ---- Wire types (mirror ActionFlow.API/Contracts/Dtos.cs, camelCased) ----

export type ApiStep = {
  name: string;
  actionType: string;
  conditionExpression?: string | null;
  properties?: Record<string, unknown> | null;
};

export type ApiParameter = {
  name: string;
  expression: string;
};

export type WorkflowResponse = {
  name: string;
  version: number;
  isLatest: boolean;
  steps: ApiStep[];
  outputParameters: ApiParameter[];
  metadata: Record<string, string>;
  createdAt: string;
};

export type WorkflowSummary = {
  name: string;
  latestVersion: number;
  createdAt: string;
};

export type WorkflowDefinitionRequest = {
  name: string;
  steps: ApiStep[];
  outputParameters?: ApiParameter[];
  metadata?: Record<string, string>;
};

export type ValidationError = { location: string; message: string };
export type ValidationResponse = { isValid: boolean; errors: ValidationError[] };

export type ExecuteResponse = { executionId: string };

export type TestRunResponse = {
  success: boolean;
  output: Record<string, string>;
  error: string | null;
};

export type ExecutionStatusResponse = {
  executionId: string;
  workflowName: string;
  version: number | null;
  status: string;
  currentStepIndex: number;
  completedSteps: string[];
  error: string | null;
  outputParameters: Record<string, string>;
  requestedAt: string;
  startedAt: string | null;
  completedAt: string | null;
};

const TERMINAL_STATUSES = new Set(["Completed", "Failed", "Compensated"]);

async function request<T>(path: string, init?: RequestInit): Promise<T> {
  const res = await fetch(`${API_BASE}${path}`, {
    ...init,
    headers: {
      "Content-Type": "application/json",
      ...(init?.headers ?? {}),
    },
    cache: "no-store",
  });

  if (!res.ok) {
    const body = await res.text().catch(() => "");
    throw new Error(
      `ActionFlow API ${init?.method ?? "GET"} ${path} failed: ${res.status} ${res.statusText} ${body}`.trim(),
    );
  }

  if (res.status === 204) {
    return undefined as T;
  }

  return (await res.json()) as T;
}

/** List every workflow (latest version per name). Returns [] if the API is unreachable. */
export async function listWorkflows(): Promise<WorkflowSummary[]> {
  try {
    return await request<WorkflowSummary[]>("/workflows");
  } catch {
    return [];
  }
}

/** Load the latest version of a workflow, or null when it does not exist / API is unreachable. */
export async function getWorkflow(name: string): Promise<WorkflowResponse | null> {
  try {
    const res = await fetch(`${API_BASE}/workflows/${encodeURIComponent(name)}`, {
      cache: "no-store",
    });
    if (res.status === 404) return null;
    if (!res.ok) return null;
    return (await res.json()) as WorkflowResponse;
  } catch {
    return null;
  }
}

/** Create (publish version 1 of) a new workflow. */
export function createWorkflow(
  workflow: WorkflowDefinitionRequest,
): Promise<WorkflowResponse> {
  return request<WorkflowResponse>("/workflows", {
    method: "POST",
    body: JSON.stringify(workflow),
  });
}

/**
 * Save a workflow. `PUT /workflows/{name}` publishes the next version and also
 * creates version 1 when the workflow does not exist yet, so it works for both
 * create and update.
 */
export function updateWorkflow(
  name: string,
  workflow: WorkflowDefinitionRequest,
): Promise<WorkflowResponse> {
  return request<WorkflowResponse>(`/workflows/${encodeURIComponent(name)}`, {
    method: "PUT",
    body: JSON.stringify(workflow),
  });
}

/** Delete every version of a workflow. */
export function deleteWorkflow(name: string): Promise<void> {
  return request<void>(`/workflows/${encodeURIComponent(name)}`, {
    method: "DELETE",
  });
}

/**
 * Run a workflow synchronously in the API and get its output back immediately (the editor's Test
 * path). Bypasses the async Runner/Kafka pipeline, so the result is deterministic and does not
 * require polling the execution read model.
 */
export function testWorkflow(
  name: string,
  inputs: Record<string, string>,
): Promise<TestRunResponse> {
  return request<TestRunResponse>(
    `/workflows/${encodeURIComponent(name)}/test`,
    {
      method: "POST",
      body: JSON.stringify({ inputs }),
    },
  );
}

/** Fire a workflow execution against the latest persisted version. Returns the execution id. */
export function executeWorkflow(
  name: string,
  inputs: Record<string, string>,
): Promise<ExecuteResponse> {
  return request<ExecuteResponse>(
    `/workflows/${encodeURIComponent(name)}/execute`,
    {
      method: "POST",
      body: JSON.stringify({ inputs }),
    },
  );
}

/** Read the current status/outputs of an execution, or null if not found. */
export async function getExecution(
  id: string,
): Promise<ExecutionStatusResponse | null> {
  try {
    const res = await fetch(`${API_BASE}/executions/${encodeURIComponent(id)}`, {
      cache: "no-store",
    });
    if (!res.ok) return null;
    return (await res.json()) as ExecutionStatusResponse;
  } catch {
    return null;
  }
}

/**
 * Execute a workflow and poll until it reaches a terminal status (execution is
 * asynchronous — the Runner processes it off a Kafka queue). Resolves with the
 * last observed status and whether polling timed out.
 */
export async function runWorkflow(
  name: string,
  inputs: Record<string, string>,
  options?: { timeoutMs?: number; intervalMs?: number },
): Promise<{
  executionId: string;
  status: ExecutionStatusResponse | null;
  timedOut: boolean;
}> {
  const { executionId } = await executeWorkflow(name, inputs);

  // Execution is asynchronous through Kafka + the Runner; the first run after a
  // cold start also pays Kafka consumer-group join latency, so poll generously.
  const timeoutMs = options?.timeoutMs ?? 60000;
  const intervalMs = options?.intervalMs ?? 1000;
  const deadline = Date.now() + timeoutMs;

  let last: ExecutionStatusResponse | null = null;
  while (Date.now() < deadline) {
    await new Promise((resolve) => setTimeout(resolve, intervalMs));
    last = await getExecution(executionId);
    if (last && TERMINAL_STATUSES.has(last.status)) {
      return { executionId, status: last, timedOut: false };
    }
  }

  return { executionId, status: last, timedOut: true };
}

/** Poll an existing execution once more to its terminal state (for a manual re-check). */
export async function pollExecution(
  executionId: string,
  options?: { timeoutMs?: number; intervalMs?: number },
): Promise<{ status: ExecutionStatusResponse | null; timedOut: boolean }> {
  const timeoutMs = options?.timeoutMs ?? 60000;
  const intervalMs = options?.intervalMs ?? 1000;
  const deadline = Date.now() + timeoutMs;

  let last: ExecutionStatusResponse | null = await getExecution(executionId);
  if (last && TERMINAL_STATUSES.has(last.status)) return { status: last, timedOut: false };

  while (Date.now() < deadline) {
    await new Promise((resolve) => setTimeout(resolve, intervalMs));
    last = await getExecution(executionId);
    if (last && TERMINAL_STATUSES.has(last.status)) {
      return { status: last, timedOut: false };
    }
  }
  return { status: last, timedOut: true };
}

/** Validate workflow expressions without persisting. */
export function validateWorkflow(
  workflow: WorkflowDefinitionRequest,
): Promise<ValidationResponse> {
  return request<ValidationResponse>("/workflows/validate", {
    method: "POST",
    body: JSON.stringify(workflow),
  });
}
