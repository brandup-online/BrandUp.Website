import peerDepsExternal from "rollup-plugin-peer-deps-external";
import postcss from 'rollup-plugin-postcss'
import resolve from "@rollup/plugin-node-resolve";
import commonjs from "@rollup/plugin-commonjs";
import babel from "@rollup/plugin-babel";
import terser from "@rollup/plugin-terser";
import typescript from "rollup-plugin-typescript2";
const path = require('path');

const mainFile = "_client/index.ts";

export default [
    {
        input: mainFile,
        output: [
            {
                file: "wwwroot/dist/app.js",
                format: "iife",
                sourcemap: false,
                inlineDynamicImports: true,
                chunkFileNames: '[name].chunk.js'
            },
        ],
        plugins: [
            peerDepsExternal(), // исключает лишние зависимости
            postcss({
                minimize: true,
                modules: true,
                use: {
                    sass: null,
                    stylus: null,
                    less: { javascriptEnabled: true }
                }, 
                extract: false
            }),
            resolve({ moduleDirectories: [path.resolve(__dirname, 'node_modules')] }), // работа с node_modules
            commonjs(), // поддержка CommonJS
            babel({ babelHelpers: 'runtime' }),
            terser(),
            typescript({ tsconfig: "./tsconfig.json" }), // поддержка typescript
        ]
    }
];