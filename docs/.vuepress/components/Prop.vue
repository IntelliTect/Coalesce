<template>
  <h4 v-html="html" class="c-sharp-prop">
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


<script>
import * as shiki from 'shiki'

shiki.setCDN('https://unpkg.com/shiki/');

export default {
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
      type: Number,
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

  watch: {
    def: {
      immediate: true,
      handler() {
        shiki.getHighlighter({
          theme: 'dark-plus',
          langs: [this.lang]
        })
        .then(highlighter => {
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
        })
      }
    }
  },
};
</script>
