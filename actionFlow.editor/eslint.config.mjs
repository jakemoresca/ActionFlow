import next from "eslint-config-next";

const eslintConfig = [
  ...next,
  {
    ignores: [".next/**", ".flowbite-react/**", "node_modules/**"],
  },
];

export default eslintConfig;
