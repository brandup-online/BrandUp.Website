import nodeResolve from "@rollup/plugin-node-resolve";
import typescript from '@rollup/plugin-typescript';
import dts from "rollup-plugin-dts";

const pkg = require("./package.json");
const mainFile = "source/index.ts";

const externals = [
  ...Object.keys(pkg.dependencies || {}),
  ...Object.keys(pkg.peerDependencies || {})
];

export default [
  {
    input: mainFile,
    output: [
      {
        file: pkg.main,
        format: "cjs",
        assetFileNames: '[name][extname]',
        sourcemap: true
      },
      {
        file: pkg.module,
        format: "esm",
        assetFileNames: '[name][extname]',
        sourcemap: true
      }
    ],
    external: id => externals.some(name => id.startsWith(name)),
    plugins: [
      nodeResolve({
        extensions: ['.mjs', '.js', '.json', '.node', '.ts', '.mts'],
        preferBuiltins: true
      }),
      typescript({ tsconfig: "./tsconfig.json" })
    ]
  },
  {
    input: mainFile,
    output: [{ file: pkg.types, format: "es" }],
    plugins: [dts.default()]
  }
];