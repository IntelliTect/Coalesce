const VuetifyLoaderPlugin = require("vuetify-loader/lib/plugin");
const nodeExternals = require("webpack-node-externals");

module.exports = {
  lintOnSave: false,

  configureWebpack: {
    externals: [nodeExternals({
      allowlist: [
        /get\-current\-script/
      ]
    })]
  },

  chainWebpack: config => {
    config.plugin("fork-ts-checker").tap(args => {
      args[0].tsconfig = "./tsconfig.build.json";
      return args;
    });
  }
};
