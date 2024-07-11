import type { BuiltInEdge, Edge, EdgeTypes } from "@xyflow/react";

import ButtonEdge, { type ButtonEdge as ButtonEdgeType } from "./ButtonEdge";

export const initialEdges = [
  { id: "a->b", source: "a", target: "b", animated: false, label: "event" },
  { id: "b->c", source: "b", target: "c", animated: false },
  { id: "c->d", source: "c", target: "d", animated: false },
] satisfies Edge[];

export const edgeTypes = {
  // Add your custom edge types here!
  "button-edge": ButtonEdge,
} satisfies EdgeTypes;

// Append the types of you custom edges to the BuiltInEdge type
export type CustomEdgeType = BuiltInEdge | ButtonEdgeType;
