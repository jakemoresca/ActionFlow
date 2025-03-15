export const treeRootId = "a";
export const initialTree = {
  a: {
    id: "a",
    type: "variable",
    name: "root",
    data: { label: "Event Trigger", variables: {} },
    deletable: false,
    children: ["b"],
  },
  b: {
    id: "b",
    type: "variable",
    name: "variable",
    data: {
      label: "Initialize Variable",
      variables: {
        age: "1",
        canWalk: "true",
      },
    },
    selectable: true,
    children: ["c"],
   },
  c: {
    id: "c",
    name: "variable",
    type: "variable",
    data: {
      label: "Test variable value",
      condition: "age == 1 \u0026\u0026 canWalk == true",
    },
    selectable: true,
    children: ["d"],
  },
  d: {
    id: "d",
    name: "output",
    type: "variable",
    data: { label: "Return" },
  },
};