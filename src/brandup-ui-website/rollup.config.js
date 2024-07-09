import resolve from "@rollup/plugin-node-resolve";
import commonjs from "@rollup/plugin-commonjs";
import typescript from "rollup-plugin-typescript2";
import dts from "rollup-plugin-dts";
import terser from "@rollup/plugin-terser";
import peerDepsExternal from "rollup-plugin-peer-deps-external";

const packageJson = require("./package.json");
const mainFile = "source/index.ts";

export default [
  {
    input: mainFile,
    output: [
      {
        file: packageJson.main,
        format: "cjs",
        sourcemap: true
      },
      {
        file: packageJson.module,
        format: "esm",
        sourcemap: true
      },
    ],
    plugins: [
      peerDepsExternal(), // исключает лишние зависимости
      resolve(), // работа с node_modules
      commonjs(), // поддержка CommonJS
      typescript({ tsconfig: "./tsconfig.json" }), // поддержка typescript
      terser() // минификация сборки
    ]
  },
  {
    input: mainFile,
    output: [ { file: packageJson.types, format: "es" } ],
    plugins: [ dts.default() ],
  }
];