const plugins = [
	[
		'@babel/plugin-transform-runtime', {
			absoluteRuntime: false,
			corejs: false,
			helpers: true,
			useESModules: true
		}
	]
]; // '@babel/plugin-transform-runtime'

module.exports = {
  presets: [
    [
		"@babel/preset-env", {
			useBuiltIns: "usage",
			corejs: "3.37.1",
			debug: false
		}
	],
	"@babel/preset-typescript"
  ],
  plugins: plugins
};