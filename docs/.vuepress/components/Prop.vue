<template>
  <h4 v-if="html" v-html="html" class="c-sharp-prop">
  </h4>
  <h4 v-else class="c-sharp-prop">
    <pre class="shiki" style="background-color: #1E1E1E; line-height: 1.18; padding-top: 1px; padding-bottom: 4px;">
      <code>
        {{def}}
      </code>
    </pre>
  </h4>
</template>


<style lang="scss">
.c-sharp-prop {
  margin-top: 10px;
  .shiki {
    margin: 0;
    padding: 2px 8px;
    white-space: normal;
    .line {
      display: block;
      white-space: pre-wrap;
    }
  }
  .line.hidden {
    display: none;
  }

  $selector: ".c-sharp-prop,h1,h2,h3,h4,h5";

  + :not(#{$selector}),
  + :not(#{$selector}) + :not(#{$selector}),
  + :not(#{$selector}) + :not(#{$selector}) + :not(#{$selector}),
  + :not(#{$selector}) + :not(#{$selector}) + :not(#{$selector}) + :not(#{$selector}),
  + :not(#{$selector}) + :not(#{$selector}) + :not(#{$selector}) + :not(#{$selector}) + :not(#{$selector})
  {
    margin-top: 4px;
    margin-left: 20px;
  }
}
</style>


<script lang="ts">
import { defineComponent } from 'vue'
import type {Highlighter} from 'shiki';

var highlighters = new Map<string, Highlighter>();
async function getHighlighter(lang) {
  if (highlighters.has(lang)) return highlighters.get(lang);
  
  // Dynamic import to avoid dependency in production builds
  const shiki = await import('shiki')
  shiki.setCDN('https://unpkg.com/shiki/');
  const highlighter = await shiki.getHighlighter({
    theme: 'dark-plus',
    langs: [lang]
  });
  highlighters.set(lang, highlighter);
  return highlighter;
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
  },


  data() {
    return {
      html: ''
    }
  },

  async serverPrefetch() {
    await this.renderHtml();
  },
  beforeMount() {
    // Copy pre-rendered HTML from the static render if it is present.
    this.html = this.$el?.innerHTML
    if (!this.html) {
      // If pre-rendered HTML is not present, dynamically render it.
      // We set a watcher on the definition for better development experience
      this.$watch('def', () => this.renderHtml(), { immediate: true })
    }
  },

  methods: {
    async renderHtml() {
      const highlighter = this.highlighter ?? await getHighlighter(this.lang);
      // Wrap in a class so the syntax is highlighted correctly

      var ctorDesc = null;
      if (this.ctor) {
        ctorDesc = '// Also settable via constructor parameter #' + this.ctor;

        if ((this.lang == "c#" || this.lang == "csharp") && (!this.def.match(/\bset\b/))) {
          ctorDesc = '// ONLY settable via constructor parameter #' + this.ctor;
        }
      }

      const code = 
        (this.noClass ? '' : 'public class x {' )
        + (ctorDesc ? '\n' + ctorDesc : '')
        + '\n' + this.def 
        + (this.noClass ? '' : '\n}' );

      this.html = highlighter.codeToHtml(code, 
      { 
        lang: this.lang,
        lineOptions: this.noClass ? [] : [
          {line: 1, classes: ['hidden']},
          {line: code.split('\n').length, classes: ['hidden']},
        ]
      })
    }
  }
});
</script>
