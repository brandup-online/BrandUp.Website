"use strict";

const path = require('path');
const MiniCssExtractPlugin = require('mini-css-extract-plugin');
const CleanCSSPlugin = require("less-plugin-clean-css");
const TerserPlugin = require("terser-webpack-plugin");
const { WebpackManifestPlugin } = require('webpack-manifest-plugin');
const bundleOutputDir = '../../src/ExampleWebSite/wwwroot/dist';

const lessLoaderOptions = {
    webpackImporter: true,
    lessOptions: {
        math: 'always',
        plugins: [new CleanCSSPlugin({ advanced: false })]
    }
};

module.exports = (env) => {
    const isDevBuild = process.env.NODE_ENV !== "production";

    console.log(`NODE_ENV: "${process.env.NODE_ENV}"`);
    console.log(`isDevBuild: ${isDevBuild}`);

    return [{
        mode: isDevBuild ? 'development' : 'production',
        entry: {
            app: path.resolve(__dirname, 'src', 'index.ts')
        },
        resolve: {
            cache: true,
            extensions: ['.js', '.jsx', '.ts', '.tsx', '.less'],
            modules: [path.resolve(__dirname, 'node_modules'), 'node_modules']
        },
        output: {
            path: path.join(__dirname, bundleOutputDir),
            filename: '[name].js',
            chunkFilename: isDevBuild ? '[name].js' : '[name].[contenthash].js',
            iife: true,
            clean: true,
            publicPath: '/dist/'
        },
        module: {
            rules: [
                {
                    test: /\.(?:ts|js|mjs|cjs)$/,
                    exclude: {
                        and: [/node_modules/],
                        not: [/@brandup/]
                    },
                    use: {
                        loader: 'babel-loader'
                    }
                },
                {
                    test: /\.(le|c)ss$/,
                    use: [
                        { loader: MiniCssExtractPlugin.loader },
                        { loader: 'css-loader', options: { importLoaders: 2 } },
                        { loader: 'less-loader', options: lessLoaderOptions }
                    ]
                },
                {
                    test: /\.html$/,
                    include: /pages/,
                    type: 'asset/source'
                },
                {
                    test: /\.svg$/,
                    type: 'asset/source'
                },
                {
                    test: /\.(png|jpg|jpeg|gif)$/,
                    type: 'asset',
                    parser: {
                        dataUrlCondition: { maxSize: 25000 }
                    }
                }
            ]
        },
        optimization: {
            // Точка входа собирается в самодостаточный app.js (runtime + vendors внутри),
            // т.к. layout'ы подключают только app.js. Делим лишь ленивые (async) чанки страниц.
            splitChunks: {
                chunks: 'async'
            },
            minimize: !isDevBuild,
            minimizer: [
                new TerserPlugin({
                    terserOptions: {
                        compress: true,
                        keep_classnames: false,
                        keep_fnames: false,
                        format: {
                            comments: false
                        }
                    },
                    extractComments: false
                })
            ],
            removeEmptyChunks: true,
            usedExports: true
        },
        plugins: [
            new MiniCssExtractPlugin({
                filename: '[name].css',
                chunkFilename: isDevBuild ? '[id].css' : '[id].[contenthash].css'
            }),
            new WebpackManifestPlugin({
                fileName: 'manifest.json',
            })
        ]
    }];
};
