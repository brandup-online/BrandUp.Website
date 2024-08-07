const config = {
	verbose: true,
	testMatch: ["**/test/**/*.test.ts"],
	testEnvironment: './FixJSDOMEnvironment.ts',
	transform: {
		"^.+\\.[jt]sx?$": "babel-jest",
		".+\\.(css|styl|less|sass|scss|png|jpg|ttf|woff|woff2)$": "jest-transform-stub",
	},
	moduleFileExtensions: ["js", "ts"],
	//moduleDirectories: ["node_modules", "bower_components", "shared"],
	//transformIgnorePatterns: ["/node_modules/(?!(brandup-ui?([a-z,-])*)/)"]
};

module.exports = config;