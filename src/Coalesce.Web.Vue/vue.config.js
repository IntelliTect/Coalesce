var path = require('path');

module.exports = {
  lintOnSave: false,
  outputDir: 'wwwroot',
  baseUrl: "/",
  configureWebpack: {
    resolve: {
      symlinks: false,
      alias: { 
        'coalesce-vue': path.resolve(__dirname, 'node_modules/coalesce-vue'),
        'vue': path.resolve(__dirname, 'node_modules/vue'),
      }
    }
  },
  chainWebpack: config => {

    // This is a workaround for this issue. Remove when the issue is fixed: 
    // https://github.com/Realytics/fork-ts-checker-webpack-plugin/issues/111#issuecomment-401519194
    config.plugins.delete('fork-ts-checker');
    config.module
      .rule('ts')
      .use('ts-loader')
      .tap(options => { return {...options, 'transpileOnly': false }});
  }
}