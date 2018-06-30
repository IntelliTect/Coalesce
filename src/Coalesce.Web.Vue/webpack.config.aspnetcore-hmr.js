
// This file evaluates to the effective webpack configuration for a vue-cli based project.
// See https://github.com/vuejs/vue-cli/blob/dev/docs/webpack.md#using-resolved-config-as-a-file
const config = require("@vue/cli-service/webpack.config.js")

// Tweaks to our webpack config that are needed for ASP.NET Core's Webpack middleware:

// Remove HotModuleReplacementPlugin that vue-cli adds by default.
// ASP.NET Core will add its own HMR plugin, so we would otherwise end up with two of them.
// Issue is similar to https://github.com/gaearon/react-hot-loader/issues/573#issuecomment-305714424
const HotModuleReplacementPlugin = require('webpack/lib/HotModuleReplacementPlugin')
config.plugins = config.plugins.filter(function(p) { return !( p instanceof HotModuleReplacementPlugin )});

// Workaround for https://github.com/aspnet/JavaScriptServices/issues/1495
// The exact path here doesn't matter, since this webpack config is only used for HMR hosted inside
// our ASP.NET Core site. See https://github.com/aspnet/JavaScriptServices/issues/1495#issuecomment-367689484.
config.output.publicPath = '/hmr-middleware/'


module.exports = config