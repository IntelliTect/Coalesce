module.exports = {
  lintOnSave: false,
  outputDir: 'wwwroot',
  baseUrl: "/",
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