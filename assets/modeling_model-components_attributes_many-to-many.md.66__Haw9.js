import{_ as n,D as o,o as t,c as l,I as a,R as p,k as e}from"./chunks/framework.g9eZ-ZSs.js";const g=JSON.parse('{"title":"[ManyToMany]","description":"","frontmatter":{},"headers":[],"relativePath":"modeling/model-components/attributes/many-to-many.md","filePath":"modeling/model-components/attributes/many-to-many.md"}'),r={name:"modeling/model-components/attributes/many-to-many.md"},c=p(`<h1 id="manytomany" tabindex="-1">[ManyToMany] <a class="header-anchor" href="#manytomany" aria-label="Permalink to &quot;[ManyToMany]&quot;">​</a></h1><p>Used to specify a Many to Many relationship. Because EF core does not support automatic intermediate mapping tables, this field is used to allow for direct reference of the many-to-many collections from the ViewModel.</p><p>The named specified in the attribute will be used as the name of a collection of the objects on the other side of the relationship in the generated <a href="/Coalesce/stacks/disambiguation/view-model.html">TypeScript ViewModels</a>.</p><h2 id="example-usage" tabindex="-1">Example Usage <a class="header-anchor" href="#example-usage" aria-label="Permalink to &quot;Example Usage&quot;">​</a></h2><div class="language-c#"><button title="Copy Code" class="copy"></button><span class="lang">c#</span><pre class="shiki dark-plus vp-code"><code><span class="line"><span style="color:#569CD6;">public</span><span style="color:#569CD6;"> class</span><span style="color:#4EC9B0;"> Person</span></span>
<span class="line"><span style="color:#D4D4D4;">{</span></span>
<span class="line"><span style="color:#569CD6;">    public</span><span style="color:#569CD6;"> int</span><span style="color:#9CDCFE;"> PersonId</span><span style="color:#D4D4D4;"> { </span><span style="color:#569CD6;">get</span><span style="color:#D4D4D4;">; </span><span style="color:#569CD6;">set</span><span style="color:#D4D4D4;">; }</span></span>
<span class="line"><span style="color:#569CD6;">    public</span><span style="color:#569CD6;"> string</span><span style="color:#9CDCFE;"> FirstName</span><span style="color:#D4D4D4;"> { </span><span style="color:#569CD6;">get</span><span style="color:#D4D4D4;">; </span><span style="color:#569CD6;">set</span><span style="color:#D4D4D4;">; }</span></span>
<span class="line"><span style="color:#569CD6;">    public</span><span style="color:#569CD6;"> string</span><span style="color:#9CDCFE;"> LastName</span><span style="color:#D4D4D4;"> { </span><span style="color:#569CD6;">get</span><span style="color:#D4D4D4;">; </span><span style="color:#569CD6;">set</span><span style="color:#D4D4D4;">; }</span></span>
<span class="line"></span>
<span class="line"><span style="color:#D4D4D4;">    [</span><span style="color:#4EC9B0;">ManyToMany</span><span style="color:#D4D4D4;">(</span><span style="color:#CE9178;">&quot;Appointments&quot;</span><span style="color:#D4D4D4;">)]</span></span>
<span class="line"><span style="color:#569CD6;">    public</span><span style="color:#4EC9B0;"> ICollection</span><span style="color:#D4D4D4;">&lt;</span><span style="color:#4EC9B0;">PersonAppointment</span><span style="color:#D4D4D4;">&gt; </span><span style="color:#9CDCFE;">PersonAppointments</span><span style="color:#D4D4D4;"> { </span><span style="color:#569CD6;">get</span><span style="color:#D4D4D4;">; </span><span style="color:#569CD6;">set</span><span style="color:#D4D4D4;">; }</span></span>
<span class="line"><span style="color:#D4D4D4;">}</span></span></code></pre></div><h2 id="properties" tabindex="-1">Properties <a class="header-anchor" href="#properties" aria-label="Permalink to &quot;Properties&quot;">​</a></h2>`,6),i=e("p",null,"The name of the collection that will contain the set of objects on the other side of the many-to-many relationship.",-1),y=e("p",null,"The name of the navigation property on the middle entity that points at the far side of the many-to-many relationship. Use this to resolve ambiguities when the middle table of the many-to-many relationship has more than two reference navigation properties on it.",-1);function D(m,d,h,u,C,f){const s=o("Prop");return t(),l("div",null,[c,a(s,{def:"public string CollectionName { get; }",ctor:"1"}),i,a(s,{def:"public string FarNavigationProperty { get; set; }"}),y])}const b=n(r,[["render",D]]);export{g as __pageData,b as default};