import{_ as s,o as a,c as n,R as e}from"./chunks/framework.g9eZ-ZSs.js";const m=JSON.parse('{"title":"[Inject]","description":"","frontmatter":{},"headers":[],"relativePath":"modeling/model-components/attributes/inject.md","filePath":"modeling/model-components/attributes/inject.md"}'),o={name:"modeling/model-components/attributes/inject.md"},l=e(`<h1 id="inject" tabindex="-1">[Inject] <a class="header-anchor" href="#inject" aria-label="Permalink to &quot;[Inject]&quot;">​</a></h1><p>Used to mark a <a href="/Coalesce/modeling/model-components/methods.html">Method</a> parameter for dependency injection from the application&#39;s <code>IServiceProvider</code>.</p><p>See <a href="/Coalesce/modeling/model-components/methods.html">Methods</a> for more.</p><p>This gets translated to a <code>Microsoft.AspNetCore.Mvc.FromServicesAttribute</code> in the generated API controller&#39;s action.</p><h2 id="example-usage" tabindex="-1">Example Usage <a class="header-anchor" href="#example-usage" aria-label="Permalink to &quot;Example Usage&quot;">​</a></h2><div class="language-c#"><button title="Copy Code" class="copy"></button><span class="lang">c#</span><pre class="shiki dark-plus vp-code"><code><span class="line"><span style="color:#569CD6;">public</span><span style="color:#569CD6;"> class</span><span style="color:#4EC9B0;"> Person</span></span>
<span class="line"><span style="color:#D4D4D4;">{</span></span>
<span class="line"><span style="color:#569CD6;">    public</span><span style="color:#569CD6;"> int</span><span style="color:#9CDCFE;"> PersonId</span><span style="color:#D4D4D4;"> { </span><span style="color:#569CD6;">get</span><span style="color:#D4D4D4;">; </span><span style="color:#569CD6;">set</span><span style="color:#D4D4D4;">; }</span></span>
<span class="line"><span style="color:#569CD6;">    public</span><span style="color:#569CD6;"> string</span><span style="color:#9CDCFE;"> FirstName</span><span style="color:#D4D4D4;"> { </span><span style="color:#569CD6;">get</span><span style="color:#D4D4D4;">; </span><span style="color:#569CD6;">set</span><span style="color:#D4D4D4;">; }</span></span>
<span class="line"><span style="color:#569CD6;">    public</span><span style="color:#569CD6;"> string</span><span style="color:#9CDCFE;"> LastName</span><span style="color:#D4D4D4;"> { </span><span style="color:#569CD6;">get</span><span style="color:#D4D4D4;">; </span><span style="color:#569CD6;">set</span><span style="color:#D4D4D4;">; }</span></span>
<span class="line"></span>
<span class="line"><span style="color:#569CD6;">    public</span><span style="color:#569CD6;"> string</span><span style="color:#DCDCAA;"> GetFullName</span><span style="color:#D4D4D4;">([</span><span style="color:#4EC9B0;">Inject</span><span style="color:#D4D4D4;">] </span><span style="color:#4EC9B0;">ILogger</span><span style="color:#D4D4D4;">&lt;</span><span style="color:#4EC9B0;">Person</span><span style="color:#D4D4D4;">&gt; </span><span style="color:#9CDCFE;">logger</span><span style="color:#D4D4D4;">)</span></span>
<span class="line"><span style="color:#D4D4D4;">    {</span></span>
<span class="line"><span style="color:#9CDCFE;">        logger</span><span style="color:#D4D4D4;">.</span><span style="color:#DCDCAA;">LogInformation</span><span style="color:#D4D4D4;">(</span><span style="color:#CE9178;">&quot;Person &quot;</span><span style="color:#D4D4D4;"> + </span><span style="color:#9CDCFE;">PersonId</span><span style="color:#D4D4D4;"> + </span><span style="color:#CE9178;">&quot;&#39;s full name was requested&quot;</span><span style="color:#D4D4D4;">);</span></span>
<span class="line"><span style="color:#C586C0;">        return</span><span style="color:#9CDCFE;"> FirstName</span><span style="color:#D4D4D4;"> + </span><span style="color:#CE9178;">&quot; &quot;</span><span style="color:#D4D4D4;"> + </span><span style="color:#9CDCFE;">LastName</span><span style="color:#CE9178;">&quot;</span><span style="color:#F44747;">;</span></span>
<span class="line"><span style="color:#D4D4D4;">    }</span></span>
<span class="line"><span style="color:#D4D4D4;">}</span></span></code></pre></div>`,6),p=[l];function t(c,r,D,i,y,d){return a(),n("div",null,p)}const u=s(o,[["render",t]]);export{m as __pageData,u as default};