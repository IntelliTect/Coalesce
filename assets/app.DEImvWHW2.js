// Redirect from intellitect.github.io/Coalesce/* to https://coalesce.intellitect.com/*
if (typeof window !== 'undefined' && window.location.hostname === 'intellitect.github.io' && window.location.pathname.startsWith('/Coalesce')) {
  const newPath = window.location.pathname.replace('/Coalesce', '');
  const newUrl = 'https://coalesce.intellitect.com' + newPath + window.location.search + window.location.hash;
  window.location.replace(newUrl);
  return;
}

import { R as p } from "./chunks/theme.Dbfa4Io4.js";
import {
  U as o,
  ac as u,
  ad as l,
  ae as c,
  af as f,
  ag as d,
  ah as m,
  ai as h,
  aj as g,
  ak as A,
  al as y,
  d as P,
  u as v,
  k as w,
  y as C,
  am as R,
  an as b,
  ao as E,
  a8 as S,
} from "./chunks/framework.BkavzUpE.js";



function i(e) {
  if (e.extends) {
    const a = i(e.extends);
    return {
      ...a,
      ...e,
      async enhanceApp(t) {
        a.enhanceApp && (await a.enhanceApp(t)),
          e.enhanceApp && (await e.enhanceApp(t));
      },
    };
  }
  return e;
}
const s = i(p),
  T = P({
    name: "VitePressApp",
    setup() {
      const { site: e, lang: a, dir: t } = v();
      return (
        w(() => {
          C(() => {
            (document.documentElement.lang = a.value),
              (document.documentElement.dir = t.value);
          });
        }),
        e.value.router.prefetchLinks && R(),
        b(),
        E(),
        s.setup && s.setup(),
        () => S(s.Layout)
      );
    },
  });
async function j() {
  globalThis.__VITEPRESS__ = !0;
  const e = _(),
    a = D();
  a.provide(l, e);
  const t = c(e.route);
  return (
    a.provide(f, t),
    a.component("Content", d),
    a.component("ClientOnly", m),
    Object.defineProperties(a.config.globalProperties, {
      $frontmatter: {
        get() {
          return t.frontmatter.value;
        },
      },
      $params: {
        get() {
          return t.page.value.params;
        },
      },
    }),
    s.enhanceApp && (await s.enhanceApp({ app: a, router: e, siteData: h })),
    { app: a, router: e, data: t }
  );
}
function D() {
  return g(T);
}
function _() {
  let e = o,
    a;
  return A((t) => {
    let n = y(t),
      r = null;
    return (
      n &&
        (e && (a = n),
        (e || a === n) && (n = n.replace(/\.js$/, ".lean.js")),
        (r = import(n))),
      o && (e = !1),
      r
    );
  }, s.NotFound);
}
o &&
  j().then(({ app: e, router: a, data: t }) => {
    a.go().then(() => {
      u(a.route, t.site), e.mount("#app");
    });
  });
export { j as createApp };
