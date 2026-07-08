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

/** Validate workflow expressions without persisting. */
export function validateWorkflow(
  workflow: WorkflowDefinitionRequest,
): Promise<ValidationResponse> {
  return request<ValidationResponse>("/workflows/validate", {
    method: "POST",
    body: JSON.stringify(workflow),
  });
}
