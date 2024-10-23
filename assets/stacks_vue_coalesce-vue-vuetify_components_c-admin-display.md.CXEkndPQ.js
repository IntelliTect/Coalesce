import{_ as e,c as a,o as s,a7 as o}from"./chunks/framework.BkavzUpE.js";const D=JSON.parse('{"title":"c-admin-display","description":"","frontmatter":{},"headers":[],"relativePath":"stacks/vue/coalesce-vue-vuetify/components/c-admin-display.md","filePath":"stacks/vue/coalesce-vue-vuetify/components/c-admin-display.md"}'),t={name:"stacks/vue/coalesce-vue-vuetify/components/c-admin-display.md"},n=o(`<h1 id="c-admin-display" tabindex="-1">c-admin-display <a class="header-anchor" href="#c-admin-display" aria-label="Permalink to &quot;c-admin-display&quot;">​</a></h1><p>Behaves the same as <a href="/Coalesce/stacks/vue/coalesce-vue-vuetify/components/c-display.html">c-display</a>, except any collection navigation properties will be rendered as links to an admin list page, and any models will be rendered as a link to an admin item page.</p><p>Links for collections are resolved from <a href="https://router.vuejs.org/" target="_blank" rel="noreferrer">vue-router</a> with a route name of <code>coalesce-admin-list</code>, a <code>type</code> route param containing the name of the collection&#39;s type, and a query parameter <code>filter.&lt;foreign key name&gt;</code> with a value of the primary key of the owner of the collection. This route is expected to resolve to a <a href="/Coalesce/stacks/vue/coalesce-vue-vuetify/components/c-admin-table-page.html">c-admin-table-page</a>, which is setup by default by the template outlined in <a href="/Coalesce/stacks/vue/getting-started.html">Getting Started with Vue</a>.</p><p>Links for single models are resolved from <a href="https://router.vuejs.org/" target="_blank" rel="noreferrer">vue-router</a> with a route name of <code>coalesce-admin-item</code>, a <code>type</code> route param containing the name of the model&#39;s type, and a <code>id</code> route param containing the object&#39;s primary key. This route is expected to resolve to a <a href="/Coalesce/stacks/vue/coalesce-vue-vuetify/components/c-admin-editor-page.html">c-admin-editor-page</a>, which is setup by default by the template outlined in <a href="/Coalesce/stacks/vue/getting-started.html">Getting Started with Vue</a>.</p><h2 id="examples" tabindex="-1">Examples <a class="header-anchor" href="#examples" aria-label="Permalink to &quot;Examples&quot;">​</a></h2><div class="language-template"><button title="Copy Code" class="copy"></button><span class="lang">template</span><pre class="shiki dark-plus vp-code"><code><span class="line"><span style="color:#6A9955;">&lt;!-- Renders regularly as text: --&gt;</span></span>
<span class="line"><span style="color:#808080;">&lt;</span><span style="color:#569CD6;">c-admin-display</span><span style="color:#D4D4D4;"> :</span><span style="color:#9CDCFE;">model</span><span style="color:#D4D4D4;">=</span><span style="color:#D4D4D4;">&quot;</span><span style="color:#9CDCFE;">person</span><span style="color:#D4D4D4;">&quot;</span><span style="color:#9CDCFE;"> for</span><span style="color:#D4D4D4;">=</span><span style="color:#CE9178;">&quot;firstName&quot;</span><span style="color:#D4D4D4;"> /</span><span style="color:#808080;">&gt;</span></span>
<span class="line"></span>
<span class="line"><span style="color:#6A9955;">&lt;!-- Renders as a link to an item: --&gt;</span></span>
<span class="line"><span style="color:#808080;">&lt;</span><span style="color:#569CD6;">c-admin-display</span><span style="color:#D4D4D4;"> :</span><span style="color:#9CDCFE;">model</span><span style="color:#D4D4D4;">=</span><span style="color:#D4D4D4;">&quot;</span><span style="color:#9CDCFE;">person</span><span style="color:#D4D4D4;">&quot;</span><span style="color:#9CDCFE;"> for</span><span style="color:#D4D4D4;">=</span><span style="color:#CE9178;">&quot;company&quot;</span><span style="color:#D4D4D4;"> /</span><span style="color:#808080;">&gt;</span></span>
<span class="line"></span>
<span class="line"><span style="color:#6A9955;">&lt;!-- Renders as a link to a list: --&gt;</span></span>
<span class="line"><span style="color:#808080;">&lt;</span><span style="color:#569CD6;">c-admin-display</span><span style="color:#D4D4D4;"> :</span><span style="color:#9CDCFE;">model</span><span style="color:#D4D4D4;">=</span><span style="color:#D4D4D4;">&quot;</span><span style="color:#9CDCFE;">person</span><span style="color:#D4D4D4;">&quot;</span><span style="color:#9CDCFE;"> for</span><span style="color:#D4D4D4;">=</span><span style="color:#CE9178;">&quot;casesAssigned&quot;</span><span style="color:#D4D4D4;"> /</span><span style="color:#808080;">&gt;</span></span></code></pre></div><h2 id="props" tabindex="-1">Props <a class="header-anchor" href="#props" aria-label="Permalink to &quot;Props&quot;">​</a></h2><p>Same as <a href="/Coalesce/stacks/vue/coalesce-vue-vuetify/components/c-display.html">c-display</a>.</p><h2 id="slots" tabindex="-1">Slots <a class="header-anchor" href="#slots" aria-label="Permalink to &quot;Slots&quot;">​</a></h2><p>Same as <a href="/Coalesce/stacks/vue/coalesce-vue-vuetify/components/c-display.html">c-display</a>.</p>`,10),l=[n];function p(c,r,i,d,y,m){return s(),a("div",null,l)}const h=e(t,[["render",p]]);export{D as __pageData,h as default};
