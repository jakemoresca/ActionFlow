export const treeRootId = "a";
export const initialTree = {
  a: {
    id: "a",
    type: "variable",
    name: "root",
    data: { label: "Flow", variables: {} },
    deletable: false,
    children: ["b"],
  },
  b: {
    id: "b",
    name: "output",
    type: "variable",
    data: { label: "Return" },
  },
};