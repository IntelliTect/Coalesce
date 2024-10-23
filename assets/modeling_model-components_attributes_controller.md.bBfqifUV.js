import{_ as s,c as a,I as t,a7 as n,l as e,a as l,D as r,o as c}from"./chunks/framework.BkavzUpE.js";const v=JSON.parse('{"title":"[Controller]","description":"","frontmatter":{"deprecated":true},"headers":[],"relativePath":"modeling/model-components/attributes/controller.md","filePath":"modeling/model-components/attributes/controller.md"}'),p={name:"modeling/model-components/attributes/controller.md"},i=n(`<h1 id="controller" tabindex="-1">[Controller] <a class="header-anchor" href="#controller" aria-label="Permalink to &quot;[Controller]&quot;">​</a></h1><p><code>IntelliTect.Coalesce.DataAnnotations.ControllerAttribute</code></p><p>Allows for control over the generated MVC Controllers.</p><p>Currently only controls over the API controllers are present, but additional properties may be added in the future.</p><p>This attribute may be placed on any type from which an API controller is generated, including <a href="/Coalesce/modeling/model-types/entities.html">Entity Models</a>, <a href="/Coalesce/modeling/model-types/dtos.html">Custom DTOs</a>, and <a href="/Coalesce/modeling/model-types/services.html">Services</a>.</p><h2 id="example-usage" tabindex="-1">Example Usage <a class="header-anchor" href="#example-usage" aria-label="Permalink to &quot;Example Usage&quot;">​</a></h2><div class="language-c#"><button title="Copy Code" class="copy"></button><span class="lang">c#</span><pre class="shiki dark-plus vp-code"><code><span class="line"><span style="color:#D4D4D4;">[</span><span style="color:#4EC9B0;">Controller</span><span style="color:#D4D4D4;">(</span><span style="color:#9CDCFE;">ApiRouted</span><span style="color:#D4D4D4;"> = </span><span style="color:#569CD6;">false</span><span style="color:#D4D4D4;">, </span><span style="color:#9CDCFE;">ApiControllerSuffix</span><span style="color:#D4D4D4;"> = </span><span style="color:#CE9178;">&quot;Gen&quot;</span><span style="color:#D4D4D4;">, </span><span style="color:#9CDCFE;">ApiActionsProtected</span><span style="color:#D4D4D4;"> = </span><span style="color:#569CD6;">true</span><span style="color:#D4D4D4;">)]</span></span>
<span class="line"><span style="color:#569CD6;">public</span><span style="color:#569CD6;"> class</span><span style="color:#4EC9B0;"> Person</span></span>
<span class="line"><span style="color:#D4D4D4;">{</span></span>
<span class="line"><span style="color:#569CD6;">    public</span><span style="color:#569CD6;"> int</span><span style="color:#9CDCFE;"> PersonId</span><span style="color:#D4D4D4;"> { </span><span style="color:#569CD6;">get</span><span style="color:#D4D4D4;">; </span><span style="color:#569CD6;">set</span><span style="color:#D4D4D4;">; }</span></span>
<span class="line"><span style="color:#D4D4D4;">    </span></span>
<span class="line"><span style="color:#D4D4D4;">    ...</span></span>
<span class="line"><span style="color:#D4D4D4;">}</span></span></code></pre></div><h2 id="properties" tabindex="-1">Properties <a class="header-anchor" href="#properties" aria-label="Permalink to &quot;Properties&quot;">​</a></h2>`,8),d=n("<p>Determines whether or not a <code>[Route]</code> annotation will be placed on the generated API controller. Set to <code>false</code> to prevent emission of the <code>[Route]</code> attribute.</p><p>Use cases include:</p><ul><li>Defining your routes through IRouteBuilder in Startup.cs instead</li><li>Preventing API controllers from being exposed by default.</li><li>Routing to your own custom controller that inherits from the generated API controller in order to implement more granular or complex authorization logic.</li></ul>",3),u=e("p",null,"If set, will determine the name of the generated API controller.",-1),h=e("p",null,[l("Takes precedence over the value of "),e("code",null,"ApiControllerSuffix"),l(".")],-1),m=e("p",null,"If set, will be appended to the default name of the API controller generated for this model.",-1),D=e("p",null,[l("Will be overridden by the value of "),e("code",null,"ApiControllerName"),l(" if it is set.")],-1),_=n('<p>If true, actions on the generated API controller will have an access modifier of <code>protected</code> instead of <code>public</code>.</p><p>In order to consume the generated API controller, you must inherit from the generated controller and override each desired generated action method via hiding (i.e. use <code>public new ...</code>, not <code>public override ...</code>).</p><div class="tip custom-block"><p class="custom-block-title">Note</p><p>If you inherit from the generated API controllers and override their methods without setting <code>ApiActionsProtected = true</code>, all non-overridden actions from the generated controller will still be exposed as normal.</p></div>',3);function f(y,C,g,b,A,P){const o=r("Prop");return c(),a("div",null,[i,t(o,{def:"public bool ApiRouted { get; set; } = true;"}),d,t(o,{def:"public string ApiControllerName { get; set; } = null;"}),u,h,t(o,{def:"public string ApiControllerSuffix { get; set; } = null;"}),m,D,t(o,{def:"public bool ApiActionsProtected { get; set; } = false;"}),_])}const T=s(p,[["render",f]]);export{v as __pageData,T as default};
