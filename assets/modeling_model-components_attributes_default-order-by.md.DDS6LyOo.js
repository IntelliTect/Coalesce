import{_ as l,c as o,I as n,a as e,a7 as p,l as s,D as t,o as r}from"./chunks/framework.BkavzUpE.js";const O=JSON.parse('{"title":"[DefaultOrderBy]","description":"","frontmatter":{},"headers":[],"relativePath":"modeling/model-components/attributes/default-order-by.md","filePath":"modeling/model-components/attributes/default-order-by.md"}'),c={name:"modeling/model-components/attributes/default-order-by.md"},D=p(`<h1 id="defaultorderby" tabindex="-1">[DefaultOrderBy] <a class="header-anchor" href="#defaultorderby" aria-label="Permalink to &quot;[DefaultOrderBy]&quot;">​</a></h1><p><code>IntelliTect.Coalesce.DataAnnotations.DefaultOrderByAttribute</code></p><p>Allows setting of the default manner in which the data returned to the client will be sorted. Multiple fields can be used to sort an object by specifying an index.</p><p>This affects the sort order both when requesting a list of the model itself, as well as when the model appears as a child collection off of a navigation property of another object.</p><p>In the first case (a list of the model itself), this can be overridden by setting the <code>orderBy</code> or <code>orderByDescending</code> property on the <a href="/Coalesce/stacks/vue/layers/viewmodels.html#member-list-_params">TypeScript <code>ListViewModel</code>&#39;s <code>$params</code></a>.</p><h2 id="example-usage" tabindex="-1">Example Usage <a class="header-anchor" href="#example-usage" aria-label="Permalink to &quot;Example Usage&quot;">​</a></h2><div class="language-c#"><button title="Copy Code" class="copy"></button><span class="lang">c#</span><pre class="shiki dark-plus vp-code"><code><span class="line"><span style="color:#569CD6;">public</span><span style="color:#569CD6;"> class</span><span style="color:#4EC9B0;"> Person</span></span>
<span class="line"><span style="color:#D4D4D4;">{</span></span>
<span class="line"><span style="color:#569CD6;">    public</span><span style="color:#569CD6;"> int</span><span style="color:#9CDCFE;"> PersonId</span><span style="color:#D4D4D4;"> { </span><span style="color:#569CD6;">get</span><span style="color:#D4D4D4;">; </span><span style="color:#569CD6;">set</span><span style="color:#D4D4D4;">; }</span></span>
<span class="line"><span style="color:#D4D4D4;">    </span></span>
<span class="line"><span style="color:#569CD6;">    public</span><span style="color:#569CD6;"> int</span><span style="color:#9CDCFE;"> DepartmentId</span><span style="color:#D4D4D4;"> { </span><span style="color:#569CD6;">get</span><span style="color:#D4D4D4;">; </span><span style="color:#569CD6;">set</span><span style="color:#D4D4D4;">; }</span></span>
<span class="line"></span>
<span class="line"><span style="color:#D4D4D4;">    [</span><span style="color:#4EC9B0;">DefaultOrderBy</span><span style="color:#D4D4D4;">(</span><span style="color:#9CDCFE;">FieldOrder</span><span style="color:#D4D4D4;"> = </span><span style="color:#B5CEA8;">0</span><span style="color:#D4D4D4;">, </span><span style="color:#9CDCFE;">FieldName</span><span style="color:#D4D4D4;"> = </span><span style="color:#569CD6;">nameof</span><span style="color:#D4D4D4;">(</span><span style="color:#9CDCFE;">Department</span><span style="color:#D4D4D4;">.</span><span style="color:#9CDCFE;">Order</span><span style="color:#D4D4D4;">))]</span></span>
<span class="line"><span style="color:#569CD6;">    public</span><span style="color:#4EC9B0;"> Department</span><span style="color:#9CDCFE;"> Department</span><span style="color:#D4D4D4;"> { </span><span style="color:#569CD6;">get</span><span style="color:#D4D4D4;">; </span><span style="color:#569CD6;">set</span><span style="color:#D4D4D4;">; }</span></span>
<span class="line"><span style="color:#D4D4D4;">    </span></span>
<span class="line"><span style="color:#D4D4D4;">    [</span><span style="color:#4EC9B0;">DefaultOrderBy</span><span style="color:#D4D4D4;">(</span><span style="color:#9CDCFE;">FieldOrder</span><span style="color:#D4D4D4;"> = </span><span style="color:#B5CEA8;">1</span><span style="color:#D4D4D4;">)]</span></span>
<span class="line"><span style="color:#569CD6;">    public</span><span style="color:#569CD6;"> string</span><span style="color:#9CDCFE;"> LastName</span><span style="color:#D4D4D4;"> { </span><span style="color:#569CD6;">get</span><span style="color:#D4D4D4;">; </span><span style="color:#569CD6;">set</span><span style="color:#D4D4D4;">; }</span></span>
<span class="line"><span style="color:#D4D4D4;">}</span></span></code></pre></div><div class="language-c#"><button title="Copy Code" class="copy"></button><span class="lang">c#</span><pre class="shiki dark-plus vp-code"><code><span class="line"><span style="color:#569CD6;">public</span><span style="color:#569CD6;"> class</span><span style="color:#4EC9B0;"> Person</span></span>
<span class="line"><span style="color:#D4D4D4;">{</span></span>
<span class="line"><span style="color:#569CD6;">    public</span><span style="color:#569CD6;"> int</span><span style="color:#9CDCFE;"> PersonId</span><span style="color:#D4D4D4;"> { </span><span style="color:#569CD6;">get</span><span style="color:#D4D4D4;">; </span><span style="color:#569CD6;">set</span><span style="color:#D4D4D4;">; }</span></span>
<span class="line"><span style="color:#D4D4D4;">    </span></span>
<span class="line"><span style="color:#569CD6;">    public</span><span style="color:#569CD6;"> int</span><span style="color:#9CDCFE;"> DepartmentId</span><span style="color:#D4D4D4;"> { </span><span style="color:#569CD6;">get</span><span style="color:#D4D4D4;">; </span><span style="color:#569CD6;">set</span><span style="color:#D4D4D4;">; }</span></span>
<span class="line"></span>
<span class="line"><span style="color:#D4D4D4;">    [</span><span style="color:#4EC9B0;">DefaultOrderBy</span><span style="color:#D4D4D4;">(</span><span style="color:#9CDCFE;">FieldOrder</span><span style="color:#D4D4D4;"> = </span><span style="color:#B5CEA8;">0</span><span style="color:#D4D4D4;">, </span><span style="color:#9CDCFE;">FieldName</span><span style="color:#D4D4D4;"> = </span><span style="color:#569CD6;">nameof</span><span style="color:#D4D4D4;">(</span><span style="color:#9CDCFE;">Department</span><span style="color:#D4D4D4;">.</span><span style="color:#9CDCFE;">Order</span><span style="color:#D4D4D4;">))]</span></span>
<span class="line"><span style="color:#569CD6;">    public</span><span style="color:#4EC9B0;"> Department</span><span style="color:#9CDCFE;"> Department</span><span style="color:#D4D4D4;"> { </span><span style="color:#569CD6;">get</span><span style="color:#D4D4D4;">; </span><span style="color:#569CD6;">set</span><span style="color:#D4D4D4;">; }</span></span>
<span class="line"><span style="color:#D4D4D4;">    </span></span>
<span class="line"><span style="color:#D4D4D4;">    [</span><span style="color:#4EC9B0;">DefaultOrderBy</span><span style="color:#D4D4D4;">(</span><span style="color:#9CDCFE;">FieldOrder</span><span style="color:#D4D4D4;"> = </span><span style="color:#B5CEA8;">1</span><span style="color:#D4D4D4;">)]</span></span>
<span class="line"><span style="color:#569CD6;">    public</span><span style="color:#569CD6;"> string</span><span style="color:#9CDCFE;"> LastName</span><span style="color:#D4D4D4;"> { </span><span style="color:#569CD6;">get</span><span style="color:#D4D4D4;">; </span><span style="color:#569CD6;">set</span><span style="color:#D4D4D4;">; }</span></span>
<span class="line"><span style="color:#D4D4D4;">}</span></span></code></pre></div><h2 id="properties" tabindex="-1">Properties <a class="header-anchor" href="#properties" aria-label="Permalink to &quot;Properties&quot;">​</a></h2>`,9),i=s("p",null,"Specify the index of this field when sorting by multiple fields.",-1),y=s("p",null,[e("Lower-valued properties will be used first; higher-valued properties will be used as a tiebreaker (i.e. "),s("code",null,".ThenBy(...)"),e(").")],-1),d=s("p",null,"Specify the direction of the ordering for the property.",-1),C=s("p",null,"Enum values are:",-1),u=s("ul",null,[s("li",null,[s("code",null,"DefaultOrderByAttribute.OrderByDirections.Ascending")]),s("li",null,[s("code",null,"DefaultOrderByAttribute.OrderByDirections.Descending")])],-1),h=s("p",null,[e("When using the "),s("code",null,"DefaultOrderByAttribute"),e(" on an object property, specifies the field on the object to use for sorting. See the first example above.")],-1);function f(m,b,g,_,B,E){const a=t("Prop");return r(),o("div",null,[D,n(a,{def:"public int FieldOrder { get; set; } = 0; ",ctor:"1"}),e(),i,y,n(a,{def:"public OrderByDirections OrderByDirection { get; set; } = OrderByDirections.Ascending;",ctor:"2"}),d,C,u,n(a,{def:"public string FieldName { get; set; }"}),h])}const v=l(c,[["render",f]]);export{O as __pageData,v as default};
