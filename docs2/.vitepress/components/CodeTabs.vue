<template>
  <div class="code-tabs">
    <div class="code-tabs__nav">
      <ul class="code-tabs__ul">
        <li
          v-for="(name, shorthand) in actualLanguages"
          :key="shorthand"
          class="code-tabs__li"
        >
          <button class="code-tabs__nav-tab" 
          :class="{ 'code-tabs__nav-tab-active': selectedLanguage === shorthand }"
          :aria-pressed="selectedLanguage === shorthand"
          :aria-expanded="selectedLanguage === shorthand"
          @click="switchLanguage(shorthand)"
          >
          {{ name }}
          </button>
        </li>
      </ul>
    </div>

    <div
      :key="shorthand"
      v-for="(name, shorthand) in actualLanguages"
      v-show="shorthand === selectedLanguage"
      class="code-tabs-item"
      :class="{ 'code-tabs-item__active': selectedLanguage === shorthand }"
      :aria-selected="selectedLanguage === shorthand"
    >
      <slot :name="shorthand"></slot>
    </div>
  </div>
</template>


<style lang="scss">

/**
 * code-tabs
 */
.code-tabs__nav {
  margin-top: 0.85rem;
  // 2 * margin + border-radius of <pre> tag
  margin-bottom: calc(-1.7rem - 6px);
  padding-bottom: calc(1.7rem - 6px);
  padding-left: 10px;
  padding-top: 10px;
  border-top-left-radius: 6px;
  border-top-right-radius: 6px;
  background-color: var(--code-bg-color);
}

.code-tabs__ul {
  margin: auto 0px;
  margin-bottom: 5px;
  padding-left: 0;
  display: inline-flex;
  list-style: none;
}

.code-tabs__nav-tab {
  border: 0;
  padding: 5px 10px;
  cursor: pointer;
  background-color: transparent;
  font-size: 0.90em;
  line-height: 1.4;
  color: rgba(255, 255, 255, 0.9);
  font-weight: 600;
}

.code-tabs__nav-tab:focus {
  outline: none;
}

.code-tabs__nav-tab:focus-visible {
  outline: 1px solid rgba(255, 255, 255, 0.9);
}

.code-tabs__nav-tab-active {
  border-bottom: var(--c-brand) 2px solid;
}

@media (max-width: 500px) {
  .code-tabs__nav {
    margin-left: -1.5rem;
    margin-right: -1.5rem;
    border-radius: 0;
  }
}

.code-tabs-item {
  color: #adbac7;
  background-color: var(--code-bg-color);
  div[class*=language-]::before {
    top: 5px;
  }
  
  // Give some horizontal padding to items that arent code
  > :not(.line-numbers-mode) {
    padding: 0 20px
  }

  pre, pre + div {
    // Reduce extra whitespace on top
    padding-top: 8px !important;
  }
}

</style>


<script>
// Code tabs plugin inspired by 
// https://github.com/padarom/vuepress-plugin-code-switcher,
// updated to work with Vuepress2/Vue3.

import { reactive, effect } from 'vue'

const options = {
  groups: {
    default: { vue: "Vue", knockout: "Knockout" },
    vue: { options: "Options API", setup: "Composition API" },
    'vue-bundler': { 'vue-cli': 'Vue CLI', vite: 'Vite'}
  },
};

const selected = reactive({});

export default {
  props: {
    name: {
      type: String,
      default: "default",
    },
    isolated: {
      type: Boolean,
      default: false,
    },
    languages: {
      type: Object,
      required: false,
    },
  },

  data() {
    return {
      selectedLanguage: null,
      actualLanguages: {},
    };
  },

  computed: {
    localStorageKey() {
      return `vuepress-plugin-code-switcher@${this.name}`;
    },
  },

  methods: {
    switchLanguage(value) {
      if (this.isolated) {
        return (this.selectedLanguage = value);
      }

      if (typeof localStorage !== "undefined") {
        localStorage.setItem(this.localStorageKey, value);
      }
      selected[this.name] = value;
    },

    setConfiguredDefaultLanguages() {
      // No need to override the language list if we already have manually
      // specified languages
      if (this.languages) {
        this.actualLanguages = this.languages;
      } else if (options && options.groups && options.groups[this.name]) {
        this.actualLanguages = options.groups[this.name];
      }

      this.selectedLanguage = Object.keys(this.actualLanguages)[0];

      // Set default selected tab for this group
      if (!selected[this.name]) {
        selected[this.name] = this.selectedLanguage;
      }
    },
  },

  created() {
    if (this.isolated) return;

    this.setConfiguredDefaultLanguages();
    if (!this.actualLanguages) {
      throw new Error(
        'You must specify either the "languages" prop or use the "groups" option when configuring the plugin.'
      );
    }

    if (typeof localStorage !== "undefined") {
      let selected = localStorage.getItem(this.localStorageKey);
      if (
        selected &&
        Object.keys(this.actualLanguages).indexOf(selected) !== -1
      )
        this.selectedLanguage = selected;
    }

    effect(() => this.selectedLanguage = selected[this.name] )
  },
};
</script>
