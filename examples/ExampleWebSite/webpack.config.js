"use strict";

const path = require('path');
const MiniCssExtractPlugin = require('mini-css-extract-plugin');
const CssMinimizerPlugin = require("css-minimizer-webpack-plugin");
const TerserPlugin = require("terser-webpack-plugin");
const bundleOutputDir = './wwwroot/dist';

module.exports = (env) => {
    const isDevBuild = process.env.NODE_ENV !== "production";

    console.log(`NODE_ENV: "${process.env.NODE_ENV}"`);
    console.log(`isDevBuild: ${isDevBuild}`);

    return [{
        entry: {
            app: path.resolve(__dirname, '_client', 'index.ts')
        },
        resolve: { extensions: ['.js', '.jsx', '.ts', '.tsx', '.less'] },
        output: {
            path: path.join(__dirname, bundleOutputDir),
            filename: '[name].js',
            chunkFilename: isDevBuild ? '[name].js' : '[name].[contenthash].js',
            iife: true,
            clean: true,
            publicPath: 'dist/'
        },
        module: {
            rules: [
                {
                    test: /\.tsx?$/,
                    loader: 'ts-loader',
                    options: { allowTsInNodeModules: true }
                },
                {
                    test: /\.(le|c)ss$/,
                    include: /_client/,
                    use: [
                        { loader: MiniCssExtractPlugin.loader },
                        { loader: 'css-loader', options: { importLoaders: 2 } },
                        { loader: 'less-loader', options: { webpackImporter: true, lessOptions: { math: 'always' } } }
                    ]
                },
                {
                    test: /\.svg$/,
                    include: /_client/,
                    use: 'raw-loader'
                },
                {
                    test: /\.(png|jpg|jpeg|gif)$/,
                    include: /_client/,
                    use: 'url-loader?limit=25000'
                }
            ]
        },
        optimization: {
            minimize: !isDevBuild,
            minimizer: [
                new CssMinimizerPlugin(),
                new TerserPlugin({
                    terserOptions: {
                        compress: true,
                        keep_classnames: false,
                        keep_fnames: false,
                        format: {
                            comments: false
                        },
                    },
                    extractComments: false
                })
            ],
            removeAvailableModules: true,
            removeEmptyChunks: true,
            providedExports: false,
            usedExports: false
        },
        plugins: [
            new MiniCssExtractPlugin({
                filename: '[name].css',
                chunkFilename: isDevBuild ? '[id].css' : '[id].[contenthash].css'
            })
        ]
    }];
};