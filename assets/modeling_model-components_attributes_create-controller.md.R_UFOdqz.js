import{_ as a,D as o,o as l,c as n,I as s,R as t}from"./chunks/framework.g9eZ-ZSs.js";const m=JSON.parse('{"title":"[CreateController]","description":"","frontmatter":{},"headers":[],"relativePath":"modeling/model-components/attributes/create-controller.md","filePath":"modeling/model-components/attributes/create-controller.md"}'),r={name:"modeling/model-components/attributes/create-controller.md"},p=t(`<h1 id="createcontroller" tabindex="-1">[CreateController] <a class="header-anchor" href="#createcontroller" aria-label="Permalink to &quot;[CreateController]&quot;">​</a></h1><p>By default an API and View controller are both created. This allows for suppressing the creation of either or both of these.</p><h2 id="example-usage" tabindex="-1">Example Usage <a class="header-anchor" href="#example-usage" aria-label="Permalink to &quot;Example Usage&quot;">​</a></h2><div class="language-c#"><button title="Copy Code" class="copy"></button><span class="lang">c#</span><pre class="shiki dark-plus vp-code"><code><span class="line"><span style="color:#D4D4D4;">[</span><span style="color:#4EC9B0;">CreateController</span><span style="color:#D4D4D4;">(</span><span style="color:#9CDCFE;">view</span><span style="color:#D4D4D4;">: </span><span style="color:#569CD6;">false</span><span style="color:#D4D4D4;">, </span><span style="color:#9CDCFE;">api</span><span style="color:#D4D4D4;">: </span><span style="color:#569CD6;">true</span><span style="color:#D4D4D4;">)]</span></span>
<span class="line"><span style="color:#569CD6;">public</span><span style="color:#569CD6;"> class</span><span style="color:#4EC9B0;"> Person</span></span>
<span class="line"><span style="color:#D4D4D4;">{</span></span>
<span class="line"><span style="color:#569CD6;">    public</span><span style="color:#569CD6;"> int</span><span style="color:#9CDCFE;"> PersonId</span><span style="color:#D4D4D4;"> { </span><span style="color:#569CD6;">get</span><span style="color:#D4D4D4;">; </span><span style="color:#569CD6;">set</span><span style="color:#D4D4D4;">; }</span></span>
<span class="line"><span style="color:#D4D4D4;">    </span></span>
<span class="line"><span style="color:#D4D4D4;">    ...</span></span>
<span class="line"><span style="color:#D4D4D4;">}</span></span></code></pre></div><h2 id="properties" tabindex="-1">Properties <a class="header-anchor" href="#properties" aria-label="Permalink to &quot;Properties&quot;">​</a></h2>`,5);function c(i,D,d,C,u,y){const e=o("Prop");return l(),n("div",null,[p,s(e,{def:"public bool WillCreateView { get; set; } = true",ctor:"1"}),s(e,{def:"public bool WillCreateApi { get; set; } = true",ctor:"2"})])}const _=a(r,[["render",c]]);export{m as __pageData,_ as default};