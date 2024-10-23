import{_ as e,c as t,o as a,a7 as o}from"./chunks/framework.BkavzUpE.js";const f=JSON.parse('{"title":"Vuetify Components","description":"","frontmatter":{},"headers":[],"relativePath":"stacks/vue/coalesce-vue-vuetify/overview.md","filePath":"stacks/vue/coalesce-vue-vuetify/overview.md"}'),s={name:"stacks/vue/coalesce-vue-vuetify/overview.md"},l=o('<h1 id="vuetify-components" tabindex="-1">Vuetify Components <a class="header-anchor" href="#vuetify-components" aria-label="Permalink to &quot;Vuetify Components&quot;">​</a></h1><p><a href="https://www.npmjs.com/package/coalesce-vue-vuetify2" target="_blank" rel="noreferrer"><img src="https://img.shields.io/npm/v/coalesce-vue-vuetify2/latest?color=0176b5&amp;label=coalesce-vue-vuetify2%40latest&amp;logo=npm" alt=""></a><a href="https://www.npmjs.com/package/coalesce-vue-vuetify3" target="_blank" rel="noreferrer"><img src="https://img.shields.io/npm/v/coalesce-vue-vuetify3/latest?color=0176b5&amp;label=coalesce-vue-vuetify3%40latest&amp;logo=npm" alt=""></a></p><p>The <a href="https://vuejs.org/" target="_blank" rel="noreferrer">Vue</a> stack for Coalesce provides <a href="/Coalesce/stacks/vue/coalesce-vue-vuetify/overview.html">a set of components</a> based on <a href="https://vuetifyjs.com/" target="_blank" rel="noreferrer">Vuetify</a>, packaged up in an NPM package <a href="https://www.npmjs.com/package/coalesce-vue-vuetify2" target="_blank" rel="noreferrer">coalesce-vue-vuetify2</a> or <a href="https://www.npmjs.com/package/coalesce-vue-vuetify3" target="_blank" rel="noreferrer">coalesce-vue-vuetify3</a>. These components are driven primarily by the <a href="/Coalesce/stacks/vue/layers/metadata.html">Metadata Layer</a>, and include both low level input and display components like <a href="/Coalesce/stacks/vue/coalesce-vue-vuetify/components/c-input.html">c-input</a> and <a href="/Coalesce/stacks/vue/coalesce-vue-vuetify/components/c-display.html">c-display</a> that are highly reusable in the custom pages you&#39;ll build in your application, as well as high-level components like <a href="/Coalesce/stacks/vue/coalesce-vue-vuetify/components/c-admin-table-page.html">c-admin-table-page</a> and <a href="/Coalesce/stacks/vue/coalesce-vue-vuetify/components/c-admin-editor-page.html">c-admin-editor-page</a> that constitute entire pages.</p><h2 id="setup" tabindex="-1">Setup <a class="header-anchor" href="#setup" aria-label="Permalink to &quot;Setup&quot;">​</a></h2><p>All Coalesce projects should be started from the template described in <a href="/Coalesce/stacks/vue/getting-started.html">Getting Started with Vue</a>, and will therefore have all the setup completed for you.</p><p>If for whatever reason you find yourself adding Coalesce to an existing project, use the template as a reference for what configuration needs to be added to your project.</p><h2 id="display-components" tabindex="-1">Display Components <a class="header-anchor" href="#display-components" aria-label="Permalink to &quot;Display Components&quot;">​</a></h2><table><thead><tr><th width="170px">Component</th><th>Description</th></tr></thead><tr><td><p><a href="./components/c-display.html">c-display</a></p></td><td><p>A general-purpose component for displaying any <a href="/Coalesce/stacks/vue/layers/metadata.html#value">Value</a> by rendering the value to a string with the <a href="/Coalesce/stacks/vue/layers/models.html#VueModelDisplayFunctions">display functions from the Models Layer</a>. For plain string and number <a href="/Coalesce/stacks/vue/layers/metadata.html">values</a>, usage of this component is largely superfluous. For all other value types including dates, booleans, enums, objects, and collections, it is very handy.</p></td></tr><tr><td><p><a href="./components/c-loader-status.html">c-loader-status</a></p></td><td><p>A component for displaying progress and error information for one or more <a href="/Coalesce/stacks/vue/layers/api-clients.html#api-callers">API Callers</a>.</p><div class="tip custom-block"><p class="custom-block-title">TIP</p><p>It is highly recommended that all <a href="/Coalesce/stacks/vue/layers/api-clients.html#api-callers">API Callers</a> utilized by your application that don&#39;t have any other kind of error handling should be represented by a <a href="/Coalesce/stacks/vue/coalesce-vue-vuetify/components/c-loader-status.html">c-loader-status</a> so that users can be aware of any errors that occur.</p></div></td></tr><tr><td><p><a href="./components/c-list-range-display.html">c-list-range-display</a></p></td><td><p>Displays pagination information about the current <code>$items</code> of a <a href="/Coalesce/stacks/vue/layers/viewmodels.html">ListViewModel</a> in the format <code>&lt;start index&gt; - &lt;end index&gt; of &lt;total count&gt;</code>.</p></td></tr><tr><td><p><a href="./components/c-table.html">c-table</a></p></td><td><p>A table component for displaying the contents of a <a href="/Coalesce/stacks/vue/layers/viewmodels.html">ListViewModel</a>. Also supports modifying the list&#39;s <a href="/Coalesce/modeling/model-components/data-sources.html#standard-parameters">sort parameters</a> by clicking on column headers. Pairs well with a <a href="/Coalesce/stacks/vue/coalesce-vue-vuetify/components/c-list-pagination.html">c-list-pagination</a>.</p></td></tr></table><h2 id="input-components" tabindex="-1">Input Components <a class="header-anchor" href="#input-components" aria-label="Permalink to &quot;Input Components&quot;">​</a></h2><table><thead><tr><th width="170px">Component</th><th>Description</th></tr></thead><tr><td><p><a href="./components/c-input.html">c-input</a></p></td><td><p>A general-purpose input component for most <a href="/Coalesce/stacks/vue/layers/metadata.html">Values</a>. c-input delegates to other components based on the type of value it is bound to. This includes both other <a href="/Coalesce/stacks/vue/coalesce-vue-vuetify/overview.html">Coalesce Vuetify Components</a> as well as direct usages of some <a href="https://vuetifyjs.com/" target="_blank" rel="noreferrer">Vuetify</a> components.</p></td></tr><tr><td><p><a href="./components/c-select.html">c-select</a></p></td><td><p>A dropdown component that allows for selecting values fetched from the generated <code>/list</code> API endpoints.</p><p>Used for selecting values for foreign key and navigation properties, or for selecting arbitrary objects or primary keys without a parent or owning object.</p></td></tr><tr><td><p><a href="./components/c-datetime-picker.html">c-datetime-picker</a></p></td><td><p>A general, all-purpose date/time input component that can be used either with <a href="/Coalesce/stacks/vue/layers/models.html">models</a> and <a href="/Coalesce/stacks/vue/layers/metadata.html">metadata</a> or as a standalone component using only <code>v-model</code>.</p></td></tr><tr><td><p><a href="./components/c-select-many-to-many.html">c-select-many-to-many</a></p></td><td><p>A multi-select dropdown component that allows for selecting values fetched from the generated <code>/list</code> API endpoints for collection navigation properties that were annotated with <a href="/Coalesce/modeling/model-components/attributes/many-to-many.html">[ManyToMany]</a>.</p></td></tr><tr><td><p><a href="./components/c-select-string-value.html">c-select-string-value</a></p></td><td><p>A dropdown component that will present a list of suggested string values from a custom API endpoint. Allows users to input values that aren&#39;t provided by the endpoint.</p><p>Effectively, this is a server-driven autocomplete list.</p></td></tr><tr><td><p><a href="./components/c-select-values.html">c-select-values</a></p></td><td><p>A multi-select input component for collections of non-object values (primarily strings and numbers).</p></td></tr><tr><td><p><a href="./components/c-list-filters.html">c-list-filters</a></p></td><td><p>A component that provides an interface for modifying the <code>filters</code> prop of a <a href="/Coalesce/stacks/vue/layers/viewmodels.html">ListViewModel</a>&#39;s <a href="/Coalesce/modeling/model-components/data-sources.html#standard-parameters">parameters</a>.</p></td></tr><tr><td><p><a href="./components/c-list-pagination.html">c-list-pagination</a></p></td><td><p>A component that provides an interface for modifying the pagination <a href="/Coalesce/modeling/model-components/data-sources.html#standard-parameters">parameters</a> of a <a href="/Coalesce/stacks/vue/layers/viewmodels.html">ListViewModel</a>.</p><p>This is a composite of <a href="/Coalesce/stacks/vue/coalesce-vue-vuetify/components/c-list-page-size.html">c-list-page-size</a>, <a href="/Coalesce/stacks/vue/coalesce-vue-vuetify/components/c-list-range-display.html">c-list-range-display</a>, and <a href="/Coalesce/stacks/vue/coalesce-vue-vuetify/components/c-list-page.html">c-list-page</a>, arranged horizontally. It is designed to be used above or below a table (e.g. <a href="/Coalesce/stacks/vue/coalesce-vue-vuetify/components/c-table.html">c-table</a>).</p></td></tr><tr><td><p><a href="./components/c-list-page-size.html">c-list-page-size</a></p></td><td><p>A component that provides an dropdown for modifying the <code>pageSize</code> <a href="/Coalesce/modeling/model-components/data-sources.html#standard-parameters">parameter</a> prop of a <a href="/Coalesce/stacks/vue/layers/viewmodels.html">ListViewModel</a>.</p></td></tr><tr><td><p><a href="./components/c-list-page.html">c-list-page</a></p></td><td><p>A component that provides previous/next buttons and a text field for modifying the <code>page</code> <a href="/Coalesce/modeling/model-components/data-sources.html#standard-parameters">parameter</a> prop of a <a href="/Coalesce/stacks/vue/layers/viewmodels.html">ListViewModel</a>.</p></td></tr></table><h2 id="admin-components" tabindex="-1">Admin Components <a class="header-anchor" href="#admin-components" aria-label="Permalink to &quot;Admin Components&quot;">​</a></h2><table><thead><tr><th width="170px">Component</th><th>Description</th></tr></thead><tr><td><p><a href="./components/c-admin-method.html">c-admin-method</a></p></td><td><p>Provides an interface for invoking a <a href="/Coalesce/modeling/model-components/methods.html">method</a> and rendering its result, designed to be use in an admin page.</p></td></tr><tr><td><p><a href="./components/c-admin-methods.html">c-admin-methods</a></p></td><td><p>Renders in a <a href="https://vuetifyjs.com/" target="_blank" rel="noreferrer">Vuetify</a> <a href="https://vuetifyjs.com/en/components/expansion-panels/" target="_blank" rel="noreferrer">v-expansion-panels</a> a <a href="/Coalesce/stacks/vue/coalesce-vue-vuetify/components/c-admin-method.html">c-admin-method</a> for each method on a <a href="/Coalesce/stacks/vue/layers/viewmodels.html">ViewModel</a> or <a href="/Coalesce/stacks/vue/layers/viewmodels.html">ListViewModel</a>.</p></td></tr><tr><td><p><a href="./components/c-admin-display.html">c-admin-display</a></p></td><td><p>Behaves the same as <a href="/Coalesce/stacks/vue/coalesce-vue-vuetify/components/c-display.html">c-display</a>, except any collection navigation properties will be rendered as links to an admin list page, and any models will be rendered as a link to an admin item page.</p></td></tr><tr><td><p><a href="./components/c-admin-editor.html">c-admin-editor</a></p></td><td><p>An editor for a single <a href="/Coalesce/stacks/vue/layers/viewmodels.html">ViewModel</a> instance. Provides a <a href="/Coalesce/stacks/vue/coalesce-vue-vuetify/components/c-input.html">c-input</a> for each property of the model.</p></td></tr><tr><td><p><a href="./components/c-admin-editor-page.html">c-admin-editor-page</a></p></td><td><p>A page for a creating/editing single <a href="/Coalesce/stacks/vue/layers/viewmodels.html">ViewModel</a> instance. Provides a <a href="/Coalesce/stacks/vue/coalesce-vue-vuetify/components/c-admin-editor.html">c-admin-editor</a> and a <a href="/Coalesce/stacks/vue/coalesce-vue-vuetify/components/c-admin-methods.html">c-admin-methods</a> for the instance. Designed to be routed to directly with <a href="https://router.vuejs.org/" target="_blank" rel="noreferrer">vue-router</a>.</p></td></tr><tr><td><p><a href="./components/c-admin-table.html">c-admin-table</a></p></td><td><p>An full-featured table for a <a href="/Coalesce/stacks/vue/layers/viewmodels.html">ListViewModel</a>, including a <a href="/Coalesce/stacks/vue/coalesce-vue-vuetify/components/c-admin-table-toolbar.html">c-admin-table-toolbar</a>, <a href="/Coalesce/stacks/vue/coalesce-vue-vuetify/components/c-table.html">c-table</a>, and <a href="/Coalesce/stacks/vue/coalesce-vue-vuetify/components/c-list-pagination.html">c-list-pagination</a>.</p></td></tr><tr><td><p><a href="./components/c-admin-table-toolbar.html">c-admin-table-toolbar</a></p></td><td><p>A full-featured toolbar for a <a href="/Coalesce/stacks/vue/layers/viewmodels.html">ListViewModel</a> designed to be used on an admin page, including &quot;Create&quot; and &quot;Reload&quot; buttons, a <a href="/Coalesce/stacks/vue/coalesce-vue-vuetify/components/c-list-range-display.html">c-list-range-display</a>, a <a href="/Coalesce/stacks/vue/coalesce-vue-vuetify/components/c-list-page.html">c-list-page</a>, a search field, <a href="/Coalesce/stacks/vue/coalesce-vue-vuetify/components/c-list-filters.html">c-list-filters</a>, and a <a href="/Coalesce/stacks/vue/coalesce-vue-vuetify/components/c-list-page-size.html">c-list-page-size</a>.</p></td></tr><tr><td><p><a href="./components/c-admin-table-page.html">c-admin-table-page</a></p></td><td><p>A full-featured page for interacting with a <a href="/Coalesce/stacks/vue/layers/viewmodels.html">ListViewModel</a>. Provides a <a href="/Coalesce/stacks/vue/coalesce-vue-vuetify/components/c-admin-table.html">c-admin-table</a> and a <a href="/Coalesce/stacks/vue/coalesce-vue-vuetify/components/c-admin-methods.html">c-admin-methods</a> for the list. Designed to be routed to directly with <a href="https://router.vuejs.org/" target="_blank" rel="noreferrer">vue-router</a>.</p></td></tr><tr><td><p><a href="./components/c-admin-audit-log-page.html">c-admin-audit-log-page</a></p></td><td><p>A full-featured page for interacting with Coalesce&#39;s <a href="/Coalesce/topics/audit-logging.html">Audit Logging</a>. Presents a view similar to <a href="/Coalesce/stacks/vue/coalesce-vue-vuetify/components/c-admin-table-page.html">c-admin-table-page</a> with content optimized for viewing audit log records. Designed to be routed to directly with <a href="https://router.vuejs.org/" target="_blank" rel="noreferrer">vue-router</a>.</p></td></tr></table>',12),n=[l];function r(c,i,d,p,m,h){return a(),t("div",null,n)}const v=e(s,[["render",r]]);export{f as __pageData,v as default};
