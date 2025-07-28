// https://vitepress.dev/guide/custom-theme
import { h } from "vue";
import type { Theme } from "vitepress";
import DefaultTheme from "vitepress/theme";
import "./style.scss";

import CodeTabs from "../components/CodeTabs.vue";
import Prop from "../components/Prop.vue";
import SiteFooter from "../components/SiteFooter.vue";
import Beta from "../components/Beta.vue";

const redirects = {
  "/Coalesce/modeling/model-types/external-types":
    "/Coalesce/modeling/model-types/simple-models",
};

export default {
  extends: DefaultTheme,
  Layout: () => {
    return h(DefaultTheme.Layout, null, {
      // https://vitepress.dev/guide/extending-default-theme#layout-slots
      "doc-bottom": () => h(SiteFooter, { class: "page-footer" }),
    });
  },
  enhanceApp({ app, router, siteData }) {
    // ...
    app.component("CodeTabs", CodeTabs);
    app.component("Prop", Prop);
    app.component("SiteFooter", SiteFooter);
    app.component("Beta", Beta);

    router.onBeforeRouteChange = (to: string) => {
      const path = to.replace(/\.html$/i, ""),
        toPath = redirects[path];

      if (toPath) {
        setTimeout(() => {
          router.go(toPath);
        });
        return false;
      } else {
        return true;
      }
    };
  },
} satisfies Theme;
