const path = require('path');
const webpack = require('webpack');
const ExtractTextPlugin = require('extract-text-webpack-plugin');
const bundleOutputDir = './wwwroot/dist';

const typescript = require('typescript');
console.log("Typescript version: " + typescript.version)

module.exports = (env) => {
    const isDevBuild = !(env && env.prod);

    return [{
        resolve: {
            // Extensions that can be imported without an extension in their "import ... from './path/to/file".
            // .ts and .js are the only ones here to mirror what Typescript does.
            extensions: ['.ts', '.js'],
            
            // Specify that all modules should be resolved from node_modules in the current directory.
            // This allows our locally-linked copy of package "coalesce-vue" to resolve its dependencies correctly.
            // If this wasn't here, we would end up with two copies of vue in our bundle because without this,
            // the "import Vue from 'vue'" statements in "coalesce-vue" will resolve to "coalesce-vue"'s own node_modules,
            // which is a different directory than our project's node_modules.
            // This is an implementation of https://github.com/webpack/webpack/issues/966#issuecomment-91562352 (resolve.root) for webpack 3.0.0
            modules: [path.join(__dirname, "node_modules")],
            alias: {
                // If it is desired to use the full build of Vue (which allows for client-side template compilation),
                // uncomment the following line: https://vuejs.org/v2/guide/installation.html
                // 'vue$': 'vue/dist/vue.esm.js'
            }
        },
        entry: { 'main': './ClientApp/boot.ts' },
        output: {
            path: path.join(__dirname, bundleOutputDir),
            filename: '[name].js',
            publicPath: 'dist/'
        },
        module: {
            rules: [
                {
                    test: /\.vue$/,
                    loader: 'vue-loader',
                    options: {
                        loaders: {
                            // Since sass-loader (weirdly) has SCSS as its default parse mode, we map
                            // the "scss" and "sass" values for the lang attribute to the right configs here.
                            // other preprocessors should work out of the box, no loader config like this necessary.
                            'scss': 'vue-style-loader!css-loader!sass-loader',
                            'sass': 'vue-style-loader!css-loader!sass-loader?indentedSyntax',
                        }
                        // other vue-loader options go here
                    }
                },
                {
                    test: /\.tsx?$/,
                    loader: 'ts-loader',
                    exclude: /node_modules/,
                    options: {
                        appendTsSuffixTo: [/\.vue$/],
                    }
                },
                {
                    test: /\.css$/,
                    loader: ['style-loader', 'css-loader']
                },
                {
                    test: /\.(scss)$/,
                    use: [{
                        loader: 'style-loader', // inject CSS to page
                    }, {
                        loader: 'css-loader', // translates CSS into CommonJS modules
                    }, {
                        loader: 'postcss-loader', // Run post css actions
                        options: {
                            plugins: function () { // post css plugins, can be exported to postcss.config.js
                                return [
                                    require('precss'),
                                    require('autoprefixer')
                                ];
                            }
                        }
                    }, {
                        loader: 'sass-loader' // compiles Sass to CSS
                    }]
                },
                {
                    test: /\.(png|woff|woff2|eot|ttf|svg)$/,
                    loader: 'file-loader',
                    options: {
                        name: '[name].[ext]?[hash]'
                    }
                }
            ]
        },
        plugins: [
            // Exclude unneeded moment locales - these can be very large.
            // (commented because we're not using moment anymore)
            // new webpack.ContextReplacementPlugin(/moment[\/\\]locale$/, /en|es|fr/),
            
            new webpack.DefinePlugin({
                'process.env': {
                    NODE_ENV: JSON.stringify(isDevBuild ? 'development' : 'production')
                }
            })
        ].concat(isDevBuild ? [
            // Plugins that apply in development builds only
            new webpack.SourceMapDevToolPlugin({
                filename: '[file].map', // Remove this line if you prefer inline source maps
                moduleFilenameTemplate: path.relative(bundleOutputDir, '[resourcePath]') // Point sourcemap entries to the original file locations on disk
            }),

            // Turn on in order to display a visual representation of the size and contents of the bundle.
            // new (require('webpack-bundle-analyzer').BundleAnalyzerPlugin)()
        ] : [
                // Plugins that apply in production builds only
                new webpack.optimize.UglifyJsPlugin(),
                new ExtractTextPlugin('site.css')
            ])
    }];
};
