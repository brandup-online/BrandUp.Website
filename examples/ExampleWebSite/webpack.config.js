"use strict";

const path = require('path');
const MiniCssExtractPlugin = require('mini-css-extract-plugin');
const CleanCSSPlugin = require("less-plugin-clean-css");
const TerserPlugin = require("terser-webpack-plugin");
const { WebpackManifestPlugin } = require('webpack-manifest-plugin');
const bundleOutputDir = './wwwroot/dist';

const lessLoaderOptions = { webpackImporter: true, lessOptions: { math: 'always', plugins: [new CleanCSSPlugin({ advanced: true })] } };
var splitChunks = {
    cacheGroups: {
        vendors: {
            test: /[\\/]node_modules[\\/]/,
            reuseExistingChunk: false,
            enforce: true
        },
        styles: {
            test: /\.(css|scss|less)$/, // нужно чтобы import`ы на одинаковые файла less не дублировались на выходе
            reuseExistingChunk: false,
            enforce: true
        },
        images: {
            test: /\.(svg|jpg|png)$/,
            reuseExistingChunk: false,
            enforce: true
        }
    }
};

module.exports = (env) => {
    const isDevBuild = process.env.NODE_ENV !== "production";

    console.log(`NODE_ENV: "${process.env.NODE_ENV}"`);
    console.log(`isDevBuild: ${isDevBuild}`);

    return [{
        entry: {
            app: path.resolve(__dirname, '_client', 'index.ts')
        },
        resolve: {
            cache: true,
            extensions: ['.js', '.jsx', '.ts', '.tsx', '.less'],
            modules: [path.resolve(__dirname, 'node_modules')]
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
                    use: [{ loader: "raw-loader" }]
                },
                {
                    test: /\.svg$/,
                    use: [
                        { loader: "raw-loader" },
                        {
                            loader: "svgo-loader",
                            options: {
                                configFile: __dirname + "/svgo.config.mjs",
                                floatPrecision: 2,
                            }
                        }
                    ]
                },
                {
                    test: /\.(png|jpg|jpeg|gif)$/,
                    use: 'url-loader?limit=25000'
                }
            ]
        },
        optimization: {
            splitChunks: splitChunks,
            minimize: !isDevBuild,
            minimizer: [
                new TerserPlugin({
                    terserOptions: {
                        compress: true,
                        keep_classnames: false,
                        keep_fnames: false,
                        format: {
                            comments: false
                        },
                        sourceMap: false
                    },
                    extractComments: false
                })
            ],
            removeAvailableModules: false,
            removeEmptyChunks: true,
            providedExports: false,
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