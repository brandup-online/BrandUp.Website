const plugins = [
  [
    '@babel/plugin-transform-runtime', {
      absoluteRuntime: false,
      corejs: false,
      helpers: true
    }
  ]
];

module.exports = {
  presets: [
    [
      "@babel/preset-env", {
        useBuiltIns: "usage",
        corejs: "3",
        // targets берётся из .browserslistrc (единый источник истины)
        debug: false
      }
    ],
    "@babel/preset-typescript"
  ],
  plugins: plugins
};
