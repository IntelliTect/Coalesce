const { readdirSync } = require("fs");
var path = require("path");

/** A local fork of https://github.com/softwareventures/resolve-typescript-plugin,
 * inlined because that package requires webpack5.
 * This is needed because we resolve coalesce-vue to its source code,
 * which contains imports with `js` extensions rather than `ts`.
 * This is valid typescript and is required to produce correct ESM output, but ts-loader doesn't understand this.
 */
class ResolveTypescriptPlugin {
  apply(resolver) {
    const target = resolver.ensureHook("file");
    for (const extension of [".ts", ".tsx"]) {
      resolver
        .getHook("raw-file")
        .tapAsync(
          "ResolveTypescriptPlugin",
          (request, resolveContext, callback) => {
            if (
              typeof request.path !== "string" ||
              request.path.match(/(^|[\\/])node_modules($|[\\/])/u) != null
            ) {
              callback();
              return;
            }

            const path = request.path.replace(/\.jsx?$/u, extension);
            if (path === request.path) {
              callback();
            } else {
              resolver.doResolve(
                target,
                {
                  ...request,
                  path,
                  relativePath: request.relativePath?.replace(
                    /\.jsx?$/u,
                    extension
                  )
                },
                `using path: ${path}`,
                resolveContext,
                callback
              );
            }
          }
        );
    }
  }
}

module.exports = {
  lintOnSave: false,
  outputDir: "wwwroot",
  publicPath: "/",
  configureWebpack: {
    plugins: [
      // This project is just a little bit too messed up to _also_ get alacarte components functional.
      // coalesce-vue-vuetify is included from source, but this doesn't seem able to properly transform
      // the vuetify component references contained within it.
      // require("unplugin-vue-components/webpack")({
      //   resolvers: [
      //     require("unplugin-vue-components/resolvers").VuetifyResolver(),
      //     require("coalesce-vue-vuetify/lib/build").CoalesceVuetifyResolver()
      //   ],
      // })
    ],
    resolve: {
      symlinks: false,
      plugins: [new ResolveTypescriptPlugin()],
      alias: {
        "coalesce-vue/lib": path.resolve(__dirname, "../coalesce-vue/src"),
        "coalesce-vue": path.resolve(__dirname, "../coalesce-vue/src"),
        "coalesce-vue-vuetify/lib": path.resolve(__dirname, "../coalesce-vue-vuetify/src/index.ts"),
        "coalesce-vue-vuetify": path.resolve(__dirname, "../coalesce-vue-vuetify/src/index.dist.ts"),
        "vue": path.resolve(__dirname, "node_modules/vue"),
        "vue-router": path.resolve(__dirname, "node_modules/vue-router")
      }
    }
  },
  chainWebpack: config => {
    // This is a workaround for this issue. Remove when the issue is fixed:
    // https://github.com/Realytics/fork-ts-checker-webpack-plugin/issues/111#issuecomment-401519194
    config.plugins.delete("fork-ts-checker");
    config.module
      .rule("ts")
      .use("ts-loader")
      .tap(options => {
        return { ...options, transpileOnly: false };
      });
  }
};
