const VuetifyLoaderPlugin = require('vuetify-loader/lib/plugin');
const nodeExternals = require('webpack-node-externals');


module.exports = {
  "lintOnSave": false,
  
  configureWebpack: {
    externals: [ nodeExternals() ]
  }
}