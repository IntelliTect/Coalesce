import{_ as a,D as s,o as t,c as o,I as n,R as l,k as p}from"./chunks/framework.g9eZ-ZSs.js";const _=JSON.parse('{"title":"[TypeScriptPartial]","description":"","frontmatter":{},"headers":[],"relativePath":"modeling/model-components/attributes/typescript-partial.md","filePath":"modeling/model-components/attributes/typescript-partial.md"}'),r={name:"modeling/model-components/attributes/typescript-partial.md"},i=l(`<h1 id="typescriptpartial" tabindex="-1">[TypeScriptPartial] <a class="header-anchor" href="#typescriptpartial" aria-label="Permalink to &quot;[TypeScriptPartial]&quot;">​</a></h1><div class="tip custom-block"><p class="custom-block-title">Note</p><p>This attribute only applies to the Knockout front-end stack. It is not applicable to the Vue stack.</p></div><p>If defined on a model, a typescript file will be generated in ./Scripts/Partials if one does not already exist. This &#39;Partial&#39; TypeScript file contains a class which inherits from the generated TypeScript ViewModel. The partial class has the same name as the generated ViewModel would normally have, and the generated ViewModel is renamed to <code>&quot;&lt;ClassName&gt;Partial&quot;</code>.</p><p>This behavior allows you to extend the behavior of the generated TypeScript view models with your own properties and methods for defining more advanced behavior on the client. One of the most common use cases of this is to define additional Knockout <code>ComputedObservable</code> properties for information that is only useful in the browser - for example, computing a css class based on data in the object.</p><h2 id="example-usage" tabindex="-1">Example Usage <a class="header-anchor" href="#example-usage" aria-label="Permalink to &quot;Example Usage&quot;">​</a></h2><div class="language-c#"><button title="Copy Code" class="copy"></button><span class="lang">c#</span><pre class="shiki dark-plus vp-code"><code><span class="line"><span style="color:#D4D4D4;">[</span><span style="color:#4EC9B0;">TypeScriptPartial</span><span style="color:#D4D4D4;">]</span></span>
<span class="line"><span style="color:#569CD6;">public</span><span style="color:#569CD6;"> class</span><span style="color:#4EC9B0;"> Employee</span></span>
<span class="line"><span style="color:#D4D4D4;">{</span></span>
<span class="line"><span style="color:#569CD6;">    public</span><span style="color:#569CD6;"> int</span><span style="color:#9CDCFE;"> EmployeeId</span><span style="color:#D4D4D4;"> { </span><span style="color:#569CD6;">get</span><span style="color:#D4D4D4;">; </span><span style="color:#569CD6;">set</span><span style="color:#D4D4D4;">; }</span></span>
<span class="line"></span>
<span class="line"><span style="color:#D4D4D4;">    ...</span></span>
<span class="line"><span style="color:#D4D4D4;">}</span></span></code></pre></div><h2 id="properties" tabindex="-1">Properties <a class="header-anchor" href="#properties" aria-label="Permalink to &quot;Properties&quot;">​</a></h2>`,7),c=p("p",null,"If set, overrides the name of the generated ViewModel which becomes the base class for the generated 'Partial' TypeScript file.",-1);function d(h,m,y,u,D,f){const e=s("Prop");return t(),o("div",null,[i,n(e,{def:"public string BaseClassName { get; set; }"}),c])}const g=a(r,[["render",d]]);export{_ as __pageData,g as default};