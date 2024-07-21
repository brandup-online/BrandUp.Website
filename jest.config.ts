import type { Config } from 'jest';

const config: Config = {
	testMatch: ["**/test/**/*.test.ts"],
	testEnvironment: "jsdom",

	verbose: true,
	transform: {
		"^.+\\.ts?$": "ts-jest",
		"^.+\\.[jt]sx?$": "babel-jest",
		".+\\.(css|styl|less|sass|scss|png|jpg|ttf|woff|woff2)$": "jest-transform-stub",
	},
	moduleFileExtensions: ["js", "ts"],
	//moduleDirectories: ["node_modules", "bower_components", "shared"],
	//transformIgnorePatterns: ["/node_modules/(?!(brandup-ui?([a-z,-])*)/)"]
};

export default config;