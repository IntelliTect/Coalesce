import{_ as l,c as o,I as t,a7 as n,l as e,a as s,D as c,o as p}from"./chunks/framework.BkavzUpE.js";const k=JSON.parse('{"title":"c-select-values","description":"","frontmatter":{},"headers":[],"relativePath":"stacks/vue/coalesce-vue-vuetify/components/c-select-values.md","filePath":"stacks/vue/coalesce-vue-vuetify/components/c-select-values.md"}'),r={name:"stacks/vue/coalesce-vue-vuetify/components/c-select-values.md"},i=n(`<h1 id="c-select-values" tabindex="-1">c-select-values <a class="header-anchor" href="#c-select-values" aria-label="Permalink to &quot;c-select-values&quot;">​</a></h1><p>A multi-select input component for collections of non-object values (primarily strings and numbers).</p><div class="tip custom-block"><p class="custom-block-title">TIP</p><p>It is unlikely that you&#39;ll ever need to use this component directly - it is highly recommended that you use <a href="/Coalesce/stacks/vue/coalesce-vue-vuetify/components/c-input.html">c-input</a> instead and let it delegate to <a href="/Coalesce/stacks/vue/coalesce-vue-vuetify/components/c-select-values.html">c-select-values</a> for you.</p></div><h2 id="examples" tabindex="-1">Examples <a class="header-anchor" href="#examples" aria-label="Permalink to &quot;Examples&quot;">​</a></h2><div class="language-template"><button title="Copy Code" class="copy"></button><span class="lang">template</span><pre class="shiki dark-plus vp-code"><code><span class="line"><span style="color:#808080;">&lt;</span><span style="color:#569CD6;">c-select-values</span><span style="color:#D4D4D4;"> </span></span>
<span class="line"><span style="color:#D4D4D4;">    :</span><span style="color:#9CDCFE;">model</span><span style="color:#D4D4D4;">=</span><span style="color:#D4D4D4;">&quot;</span><span style="color:#9CDCFE;">post</span><span style="color:#D4D4D4;">.</span><span style="color:#9CDCFE;">setTags</span><span style="color:#D4D4D4;">.</span><span style="color:#9CDCFE;">args</span><span style="color:#D4D4D4;">&quot;</span><span style="color:#D4D4D4;"> </span></span>
<span class="line"><span style="color:#9CDCFE;">    for</span><span style="color:#D4D4D4;">=</span><span style="color:#CE9178;">&quot;Post.methods.setTags.params.tagNames&quot;</span><span style="color:#D4D4D4;"> </span></span>
<span class="line"><span style="color:#D4D4D4;">/</span><span style="color:#808080;">&gt;</span></span></code></pre></div><h2 id="props" tabindex="-1">Props <a class="header-anchor" href="#props" aria-label="Permalink to &quot;Props&quot;">​</a></h2>`,6),u=e("p",null,"A metadata specifier for the value being bound. One of:",-1),d=e("ul",null,[e("li",null,[s("A string with the name of the value belonging to "),e("code",null,"model"),s(".")]),e("li",null,"A direct reference to a metadata object."),e("li",null,"A string in dot-notation that starts with a type name.")],-1),m=e("p",null,[s("An object owning the value that was specified by the "),e("code",null,"for"),s(" prop.")],-1),h=e("p",null,[s("If binding the component with "),e("code",null,"v-model"),s(", accepts the "),e("code",null,"value"),s(" part of "),e("code",null,"v-model"),s(".")],-1);function v(D,y,f,_,g,C){const a=c("Prop");return p(),o("div",null,[i,t(a,{def:"for: string | CollectionProperty | CollectionValue",lang:"ts"}),u,d,t(a,{def:"model?: Model",lang:"ts"}),m,t(a,{def:`value?: any // Vue 2
modelValue?: any // Vue 3`,lang:"ts"}),h])}const P=l(r,[["render",v]]);export{k as __pageData,P as default};
