"use strict";

const path = require('path');
const webpack = require('webpack');
const CheckerPlugin = require('awesome-typescript-loader').CheckerPlugin;
const MiniCssExtractPlugin = require('mini-css-extract-plugin');
const OptimizeCSSAssetsPlugin = require('optimize-css-assets-webpack-plugin');
const UglifyJsPlugin = require('uglifyjs-webpack-plugin');
const bundleOutputDir = './wwwroot/dist';

module.exports = () => {
    const isDevBuild = process.env.NODE_ENV !== "production";

    console.log(`NODE_ENV: "${process.env.NODE_ENV}"`);
    console.log(`isDevBuild: ${isDevBuild}`);

    return {
        entry: {
            app: path.resolve(__dirname, '_client', 'index.ts')
        },
        resolve: { extensions: ['.js', '.jsx', '.ts', '.tsx'] },
        output: {
            path: path.join(__dirname, bundleOutputDir),
            filename: '[name].js',
            chunkFilename: isDevBuild ? '[name].js' : '[name].[hash:4].js',
            publicPath: '/dist/'
        },
        module: {
            rules: [
                {
                    test: /\.tsx?$/,
                    include: /_client/,
                    use: 'awesome-typescript-loader?silent=true'
                },
                {
                    test: /\.(le|c)ss$/,
                    use: [
                        {
                            loader: MiniCssExtractPlugin.loader
                        },
                        {
                            loader: 'css-loader',
                            options: {
                                minimize: !isDevBuild
                            }
                        },
                        {
                            loader: 'less-loader',
                            options: {
                                strictMath: false,
                                noIeCompat: true,
                                minimize: !isDevBuild
                            }
                        }
                    ]
                },
                {
                    test: /\.svg$/,
                    include: [
                        path.resolve(__dirname, "_client", "brandup.pages"),
                        path.resolve(__dirname, "_client", "designers"),
                        path.resolve(__dirname, "_client", "pages"),
                        path.resolve(__dirname, "_client", "svg")
                    ],
                    use: 'raw-loader'
                },
                {
                    test: /\.(png|jpg|jpeg|gif)$/,
                    use: 'url-loader?limit=25000'
                }
            ]
        },
        optimization: {
            minimize: true,
            minimizer: [new UglifyJsPlugin({
                cache: true,
                parallel: true,
                uglifyOptions: {
                    mangle: {
                        toplevel: true,
                        keep_fnames: false
                    },
                    keep_fnames: false,
                    keep_classnames: false,
                    ie8: false,
                    output: {
                        beautify: isDevBuild
                    }
                }
            })],
            namedModules: false,
            moduleIds: isDevBuild ? 'natural' : 'size',
            chunkIds: isDevBuild ? 'natural' : 'total-size',
            removeAvailableModules: true,
            removeEmptyChunks: true,
            occurrenceOrder: true,
            providedExports: false,
            usedExports: false
        },
        plugins: [
            new CheckerPlugin(),
            new MiniCssExtractPlugin({
                filename: "[name].css",
                chunkFilename: isDevBuild ? '[name].css' : '[name].[contenthash:4].css',
                minimize: !isDevBuild
            })
        ].concat(isDevBuild ? [
            new webpack.SourceMapDevToolPlugin({
                filename: '[file].map', // Remove this line if you prefer inline source maps
                moduleFilenameTemplate: path.relative(bundleOutputDir, '[resourcePath]') // Point sourcemap entries to the original file locations on disk
            })
        ] : [
                new OptimizeCSSAssetsPlugin({})
            ])
    };
};