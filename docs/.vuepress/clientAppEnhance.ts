import { defineClientAppEnhance } from '@vuepress/client'

const delay = (t) => new Promise((r) => setTimeout(r, t))

export default defineClientAppEnhance(({ app, router, siteData }) => {
  router.options.scrollBehavior = async (to, from, saved) => {

      // Delay by a cycle to fix very inconsistent behavior around scrolling to fragments
      // when navigating within the same page 
      // (e.g. clicking a search result that is on the current page)
      // https://github.com/vuejs/router/issues/1411#issuecomment-1132582259
      await delay(0)

      let behavior: 'smooth' | 'auto' = 'smooth';
      console.log(from.path)
      if (from?.path && from?.path != '/' && from.path != to.path) {
        // When changing pages, perform the scroll when the transition is fully faded out before the new page fades in.
        await delay(200);
        behavior = "auto";
      }

      if (to.hash) {
        // If scrolling to a hash, wait until the target is rendered, up to 2 seconds
        for (let i = 0; i < 100; i++) {
          const foundEl = document.querySelector(to.hash);
          if (foundEl) {
            saved = null;
            break;
          }
          await delay(20)
        }
      }

      if (to.hash)
        return { el: to.hash, behavior };

      if (saved)
        return { ...saved, behavior };

      return { top: 0 };
  };
})