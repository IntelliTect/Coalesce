import{_ as s,c as n,I as o,a,a7 as l,l as e,D as p,o as r}from"./chunks/framework.BkavzUpE.js";const g=JSON.parse('{"title":"[DateType]","description":"","frontmatter":{"deprecated":true},"headers":[],"relativePath":"modeling/model-components/attributes/date-type.md","filePath":"modeling/model-components/attributes/date-type.md"}'),c={name:"modeling/model-components/attributes/date-type.md"},i=l(`<h1 id="datetype" tabindex="-1">[DateType] <a class="header-anchor" href="#datetype" aria-label="Permalink to &quot;[DateType]&quot;">​</a></h1><p><code>IntelliTect.Coalesce.DataAnnotations.DateTypeAttribute</code></p><div class="warning custom-block"><p class="custom-block-title">WARNING</p><p>This attribute is deprecated and not recommended for use in new development. Instead, use the native .NET types <a href="https://learn.microsoft.com/en-us/dotnet/api/system.dateonly" target="_blank" rel="noreferrer"><code>System.DateOnly</code></a> and <a href="https://learn.microsoft.com/en-us/dotnet/api/system.timeonly" target="_blank" rel="noreferrer"><code>System.TimeOnly</code></a>.</p></div><p>Specifies whether a DateTime type will have a date and a time, or only a date.</p><h2 id="example-usage" tabindex="-1">Example Usage <a class="header-anchor" href="#example-usage" aria-label="Permalink to &quot;Example Usage&quot;">​</a></h2><div class="language-c#"><button title="Copy Code" class="copy"></button><span class="lang">c#</span><pre class="shiki dark-plus vp-code"><code><span class="line"><span style="color:#569CD6;">public</span><span style="color:#569CD6;"> class</span><span style="color:#4EC9B0;"> Person</span></span>
<span class="line"><span style="color:#D4D4D4;">{</span></span>
<span class="line"><span style="color:#569CD6;">    public</span><span style="color:#569CD6;"> int</span><span style="color:#9CDCFE;"> PersonId</span><span style="color:#D4D4D4;"> { </span><span style="color:#569CD6;">get</span><span style="color:#D4D4D4;">; </span><span style="color:#569CD6;">set</span><span style="color:#D4D4D4;">; }</span></span>
<span class="line"></span>
<span class="line"><span style="color:#D4D4D4;">    [</span><span style="color:#4EC9B0;">DateType</span><span style="color:#D4D4D4;">(</span><span style="color:#9CDCFE;">DateTypeAttribute</span><span style="color:#D4D4D4;">.</span><span style="color:#9CDCFE;">DateTypes</span><span style="color:#D4D4D4;">.</span><span style="color:#9CDCFE;">DateOnly</span><span style="color:#D4D4D4;">)]</span></span>
<span class="line"><span style="color:#569CD6;">    public</span><span style="color:#4EC9B0;"> DateTimeOffset</span><span style="color:#D4D4D4;">? </span><span style="color:#9CDCFE;">BirthDate</span><span style="color:#D4D4D4;"> { </span><span style="color:#569CD6;">get</span><span style="color:#D4D4D4;">; </span><span style="color:#569CD6;">set</span><span style="color:#D4D4D4;">; }</span></span>
<span class="line"><span style="color:#D4D4D4;">}</span></span></code></pre></div><h2 id="properties" tabindex="-1">Properties <a class="header-anchor" href="#properties" aria-label="Permalink to &quot;Properties&quot;">​</a></h2>`,7),D=e("p",null,"The type of date the property represents.",-1),d=e("p",null,"Enum values are:",-1),y=e("ul",null,[e("li",null,[e("code",null,"DateTypeAttribute.DateTypes.DateTime"),a(" Subject is both a date and time.")]),e("li",null,[e("code",null,"DateTypeAttribute.DateTypes.DateOnly"),a(" Subject is only a date with no significant time component.")]),e("li",null,[e("code",null,"DateTypeAttribute.DateTypes.TimeOnly"),a(" Subject is only a time with no significant date component.")])],-1);function m(u,h,_,T,b,C){const t=p("Prop");return r(),n("div",null,[i,o(t,{def:"public DateTypes DateType { get; set; } = DateTypes.DateTime; ",ctor:"1"}),a(),D,d,y])}const E=s(c,[["render",m]]);export{g as __pageData,E as default};
