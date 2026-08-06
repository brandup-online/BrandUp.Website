const plugins = [
  '@babel/plugin-transform-runtime',
  [
    // Babel 8 убрал corejs/useBuiltIns из preset-env и transform-runtime —
    // инъекция полифилов core-js вынесена в отдельный плагин.
    'polyfill-corejs3', {
      method: 'usage-global',
      version: '3.50'
    }
  ]
];

module.exports = {
  presets: [
    [
      "@babel/preset-env", {
        // targets берётся из .browserslistrc (единый источник истины)
        debug: false
      }
    ],
    "@babel/preset-typescript"
  ],
  plugins: plugins
};
