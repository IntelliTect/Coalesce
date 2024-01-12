<template>
  <h4 v-if="html" v-html="html" class="code-prop" :id="idAttr">
  </h4>
  <h4 v-else class="code-prop" :id="idAttr">
    <!-- Temporary uncolored content that matches the result as best as possible to avoid FOUC -->
    <pre class="shiki" style="line-height: 1.18; padding-top: 1px; padding-bottom: 4px;">
      <code>
        {{def}}
      </code>
    </pre>
  </h4>
</template>


<style lang="scss">

p + .code-prop {
  margin-top: 36px;
}
.code-prop {
  margin-top: 10px;
  font-weight: inherit !important;
  
  .shiki {
    margin: 0;
    padding: 2px 8px;
    white-space: normal;
    background-color: var(--vp-code-block-bg) !important;
    border-radius: 5px;
    .line {
      display: block;
      white-space: pre-wrap;
    }
  }

  $selector: ".code-prop,h1,h2,h3,h4,h5";

  + :not(#{$selector}),
  + :not(#{$selector}) + :not(#{$selector}),
  + :not(#{$selector}) + :not(#{$selector}) + :not(#{$selector}),
  + :not(#{$selector}) + :not(#{$selector}) + :not(#{$selector}) + :not(#{$selector}),
  + :not(#{$selector}) + :not(#{$selector}) + :not(#{$selector}) + :not(#{$selector}) + :not(#{$selector})
  {
    // Position the elements following the <Prop> to be left-indented past the code block,
    // and reduce the top margin to be a little closer to the code block.
    margin-top: 4px;
    margin-left: 20px;
  }
}
</style>


<script lang="ts">
import { defineComponent } from 'vue'
import type { Highlighter, renderToHtml } from 'shiki';

var highlighters = new Map<string, ReturnType<typeof highlighterFactory>>();
async function highlighterFactory(lang) {
  // Dynamic import to avoid dependency in production builds
  const shiki = await import('shiki')
  shiki.setCDN('https://unpkg.com/shiki/');
  const highlighter = await shiki.getHighlighter({
    theme: 'dark-plus',
    langs: [lang]
  });
  return { highlighter, shiki };
}

function getHighlighter(lang) {
  if (highlighters.has(lang)) return highlighters.get(lang);
  
  // cache the promise itself so multiple concurrent invocations
  // don't make multiple requests to the CDN
  const promise = highlighterFactory(lang);
  highlighters.set(lang, promise);
  return promise;
}

export default defineComponent({
  props: {
    def: {
      type: String,
      required: true,
    },
    lang: {
      type: String,
      default: 'c#',
    },
    ctor: {
      type: [Number, String],
      default: null,
    },
    noClass: {
      type: Boolean,
      default: false,
    },
    id: {
      type: String,
      default: null,
    },
    idPrefix: {
      type: String,
      default: 'member',
    },
  },


  data() {
    return {
      idAttr: '',
      html: ''
    }
  },

  async serverPrefetch() {
    await this.renderHtml();
  },
  beforeMount() {
    // Copy pre-rendered HTML from the static render if it is present.
    this.html = this.$el?.innerHTML
    this.idAttr = this.$el?.id

    if (!this.html) {
      // If pre-rendered HTML is not present, dynamically render it.
      // We set a watcher on the definition for better development experience
      this.$watch('def', () => this.renderHtml(), { immediate: true })
    }
  },

  methods: {
    async renderHtml() {
      var ctorDesc: string | null = null;
      if (this.ctor) {
        ctorDesc = '// Also settable via constructor parameter #' + this.ctor;

        if ((this.lang == "c#" || this.lang == "csharp") && (!this.def.match(/\bset\b/))) {
          ctorDesc = '// ONLY settable via constructor parameter #' + this.ctor;
        }
      }

      // Wrap in a class so the syntax is highlighted correctly
      const code = 
        (this.noClass ? '' : 'public class x {' )
        + (ctorDesc ? '\n' + ctorDesc : '')
        + '\n' + this.def 
        + (this.noClass ? '' : '\n}' );


      if (this.id) {
        this.idAttr = this.id
      } else if (this.lang == "ts") {
        // Typescript is pretty nice in that the member name is always first in its declaration.
        // No types on the left hand side.
        // "namespace" is included as part of the attr if present.
        const result = /(?:(?:readonly|public|static|protected|private|abstract|export) )*((?:namespace )?[\w$-]+)/.exec(this.def);
        this.idAttr = result ? this.idPrefix + '-' + result[1] : null
      } else if (this.lang == "c#") {
        // For C#, there's always a type on the left hand side.
        // The name should be immediately preceded by a space, 
        // and followed by a:
        // - `<` (generic method) 
        // - `(` (nongeneric method) 
        // - ` {` (prop definition) 
        // - `<eol>` (field definition, or prop where {get;set;} were omitted from the def)
        const result = / (\w+)(?:<|\(| \{|$)/.exec(this.def);
        this.idAttr = result ? this.idPrefix + '-' + result[1] : null
      }

      if (!this.idAttr) {
        throw new Error("Unable to compute id for Prop " + this.def)
      }

      this.idAttr = this.idAttr
        .toLowerCase()
        .replace(/[ &<>"']/g,'-')
        .replace(/\$/g,'_')

      const { highlighter, shiki } = await getHighlighter(this.lang)!;
      const tokens = highlighter.codeToThemedTokens(code, this.lang)
      
      if (!this.noClass) {
        // Strip out the fake class wrapper that was added to get the colors right:
        tokens.shift();
        tokens.pop();
      }

      const theme = highlighter.getTheme();
      let html = shiki.renderToHtml(tokens, {
        fg: theme.fg,
        bg: theme.bg
      })
      
      this.html = `<a class="header-anchor" href="#${this.idAttr}" aria-hidden="true"></a>${html}`;
    }
  }
});
</script>
