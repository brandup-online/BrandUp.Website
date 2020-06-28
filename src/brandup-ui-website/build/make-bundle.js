const path = require("path");
const dtsBundle = require('dts-bundle');
const pkg = require("../package.json");

const compiledPath = path.join(__dirname, "..", "source");
const distNpmPath = path.join(__dirname, "..", "dist");

async function build() {
    dtsBundle.bundle({
        name: pkg.name,
        main: path.join(compiledPath, 'index.d.ts'),
        out: path.join(distNpmPath, 'index.d.ts')
    });
}

build().then(() => {
    console.log("done");
}, err => console.log(err.message, err.stack));