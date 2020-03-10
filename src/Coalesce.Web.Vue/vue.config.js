var path = require('path');

module.exports = {
  lintOnSave: false,
  outputDir: 'wwwroot',
  publicPath: "/",
  configureWebpack: {
    resolve: {
      symlinks: false,
      alias: { 
        'coalesce-vue/lib': path.resolve(__dirname, '../coalesce-vue/src'),
        'coalesce-vue': path.resolve(__dirname, '../coalesce-vue/src'),
        'coalesce-vue-vuetify': path.resolve(__dirname, '../coalesce-vue-vuetify/src'),
        'vue': path.resolve(__dirname, 'node_modules/vue'),
        'vue-router': path.resolve(__dirname, 'node_modules/vue-router'),
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