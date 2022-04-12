var path = require('path');

module.exports = {
  lintOnSave: false,
  outputDir: 'wwwroot',
  publicPath: "/",
  configureWebpack: {
    plugins: [
      require('unplugin-vue-components/webpack')({ resolvers: [
        //VuetifyResolver(), 
        require('coalesce-vue-vuetify/lib/build').CoalesceVuetifyResolver()
      ] }),
    ],
    resolve: {
      symlinks: false,
      alias: { 
        'coalesce-vue/lib': path.resolve(__dirname, '../coalesce-vue/lib'),
        'coalesce-vue': path.resolve(__dirname, '../coalesce-vue'),
        'coalesce-vue-vuetify/lib': path.resolve(__dirname, '../coalesce-vue-vuetify/lib'),
        'coalesce-vue-vuetify': path.resolve(__dirname, '../coalesce-vue-vuetify'),
        // 'coalesce-vue/lib': path.resolve(__dirname, '../coalesce-vue/src'),
        // 'coalesce-vue': path.resolve(__dirname, '../coalesce-vue/src'),
        // 'coalesce-vue-vuetify/lib': path.resolve(__dirname, '../coalesce-vue-vuetify/src/index.ts'),
        // 'coalesce-vue-vuetify': path.resolve(__dirname, '../coalesce-vue-vuetify/src/index.dist.ts'),
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