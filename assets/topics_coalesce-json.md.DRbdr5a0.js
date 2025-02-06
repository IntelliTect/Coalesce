import{_ as s,c as n,o as e,a7 as a}from"./chunks/framework.BkavzUpE.js";const D=JSON.parse('{"title":"Config: Code Gen","description":"","frontmatter":{"title":"Config: Code Gen"},"headers":[],"relativePath":"topics/coalesce-json.md","filePath":"topics/coalesce-json.md"}'),o={name:"topics/coalesce-json.md"},l=a(`<h1 id="code-generation-configuration" tabindex="-1">Code Generation Configuration <a class="header-anchor" href="#code-generation-configuration" aria-label="Permalink to &quot;Code Generation Configuration&quot;">​</a></h1><p>In Coalesce, all configuration of the code generation is done in a JSON file. This file is typically named <code>coalesce.json</code> and is typically placed in the solution root.</p><h2 id="file-resolution" tabindex="-1">File Resolution <a class="header-anchor" href="#file-resolution" aria-label="Permalink to &quot;File Resolution&quot;">​</a></h2><p>When the code generation is run by invoking <code>dotnet coalesce</code>, Coalesce will try to find a configuration file via the following means:</p><ol><li>If an argument is specified on the command line, it will be used as the location of the file. E.g. <code>dotnet coalesce C:/Projects/MyProject/config.json</code></li><li>If no argument is given, Coalesce will try to use a file in the working directory named <code>coalesce.json</code></li><li>If no file is found in the working directory, Coalesce will crawl up the directory tree from the working directory until a file named <code>coalesce.json</code> is found. If such a file is never found, an error will be thrown.</li></ol><h2 id="contents" tabindex="-1">Contents <a class="header-anchor" href="#contents" aria-label="Permalink to &quot;Contents&quot;">​</a></h2><p>A full example of a <code>coalesce.json</code> file, along with an explanation of each property, is as follows:</p><div class="language-js"><button title="Copy Code" class="copy"></button><span class="lang">js</span><pre class="shiki dark-plus vp-code"><code><span class="line"><span style="color:#D4D4D4;">{</span></span>
<span class="line"><span style="color:#CE9178;">    &quot;webProject&quot;</span><span style="color:#D4D4D4;">: {</span></span>
<span class="line"><span style="color:#6A9955;">        // Required: Path to the csproj of the web project. Path is relative to location of this coalesce.json file.</span></span>
<span class="line"><span style="color:#CE9178;">        &quot;projectFile&quot;</span><span style="color:#9CDCFE;">:</span><span style="color:#CE9178;"> &quot;src/Coalesce.Web/Coalesce.Web.csproj&quot;</span><span style="color:#D4D4D4;">,</span></span>
<span class="line"></span>
<span class="line"><span style="color:#6A9955;">        // Optional: Framework to use when evaluating &amp; building dependencies.</span></span>
<span class="line"><span style="color:#6A9955;">        // Not needed if your project only specifies a single framework - only required for multi-targeting projects.</span></span>
<span class="line"><span style="color:#CE9178;">        &quot;framework&quot;</span><span style="color:#9CDCFE;">:</span><span style="color:#CE9178;"> &quot;netcoreapp2.0&quot;</span><span style="color:#D4D4D4;">,</span></span>
<span class="line"></span>
<span class="line"><span style="color:#6A9955;">        // Optional: Build configuration to use when evaluating &amp; building dependencies.</span></span>
<span class="line"><span style="color:#6A9955;">        // Defaults to &quot;Debug&quot;.</span></span>
<span class="line"><span style="color:#CE9178;">        &quot;configuration&quot;</span><span style="color:#9CDCFE;">:</span><span style="color:#CE9178;"> &quot;Debug&quot;</span><span style="color:#D4D4D4;">,</span></span>
<span class="line"></span>
<span class="line"><span style="color:#6A9955;">        // Optional: Override the namespace prefix for generated C# code.</span></span>
<span class="line"><span style="color:#6A9955;">        // Defaults to MSBuild&#39;s \`$(RootNamespace)\` for the project.</span></span>
<span class="line"><span style="color:#CE9178;">        &quot;rootNamespace&quot;</span><span style="color:#9CDCFE;">:</span><span style="color:#CE9178;"> &quot;MyCompany.Coalesce.Web&quot;</span><span style="color:#D4D4D4;">,</span></span>
<span class="line"><span style="color:#D4D4D4;">    },</span></span>
<span class="line"></span>
<span class="line"><span style="color:#CE9178;">    &quot;dataProject&quot;</span><span style="color:#D4D4D4;">: {</span></span>
<span class="line"><span style="color:#6A9955;">        // Required: Path to the csproj of the data project. Path is relative to location of this coalesce.json file.</span></span>
<span class="line"><span style="color:#CE9178;">        &quot;projectFile&quot;</span><span style="color:#9CDCFE;">:</span><span style="color:#CE9178;"> &quot;src/Coalesce.Domain/Coalesce.Domain.csproj&quot;</span><span style="color:#D4D4D4;">,</span></span>
<span class="line"></span>
<span class="line"><span style="color:#6A9955;">        // Optional: Framework to use when evaluating &amp; building dependencies.</span></span>
<span class="line"><span style="color:#6A9955;">        // Not needed if your project only specifies a single framework - only required for multi-targeting projects.</span></span>
<span class="line"><span style="color:#CE9178;">        &quot;framework&quot;</span><span style="color:#9CDCFE;">:</span><span style="color:#CE9178;"> &quot;netstandard2.0&quot;</span><span style="color:#D4D4D4;">,</span></span>
<span class="line"></span>
<span class="line"><span style="color:#6A9955;">        // Optional: Build configuration to use when evaluating &amp; building dependencies.</span></span>
<span class="line"><span style="color:#6A9955;">        // Defaults to &quot;Release&quot;.</span></span>
<span class="line"><span style="color:#CE9178;">        &quot;configuration&quot;</span><span style="color:#9CDCFE;">:</span><span style="color:#CE9178;"> &quot;Debug&quot;</span><span style="color:#D4D4D4;">,</span></span>
<span class="line"><span style="color:#D4D4D4;">    },</span></span>
<span class="line"></span>
<span class="line"><span style="color:#6A9955;">    // The name of the root generator to use.</span></span>
<span class="line"><span style="color:#6A9955;">    // The only current available value is &quot;Vue&quot; (default).</span></span>
<span class="line"><span style="color:#CE9178;">    &quot;rootGenerator&quot;</span><span style="color:#D4D4D4;">: </span><span style="color:#CE9178;">&quot;Vue&quot;</span><span style="color:#D4D4D4;">,</span></span>
<span class="line"><span style="color:#D4D4D4;">            </span></span>
<span class="line"><span style="color:#6A9955;">    // If set, specifies a list of whitelisted root type names that will restrict</span></span>
<span class="line"><span style="color:#6A9955;">    // which types Coalesce will use for code generation. </span></span>
<span class="line"><span style="color:#6A9955;">    // Root types are those that must be annotated with [Coalesce].</span></span>
<span class="line"><span style="color:#6A9955;">    // Useful if want to segment a single data project into multiple web projects, </span></span>
<span class="line"><span style="color:#6A9955;">    // or into different areas/directories within a single web project.</span></span>
<span class="line"><span style="color:#CE9178;">    &quot;rootTypesWhitelist&quot;</span><span style="color:#D4D4D4;">: [</span></span>
<span class="line"><span style="color:#CE9178;">        &quot;MyDbContext&quot;</span><span style="color:#D4D4D4;">, </span><span style="color:#CE9178;">&quot;MyCustomDto&quot;</span></span>
<span class="line"><span style="color:#D4D4D4;">    ],</span></span>
<span class="line"></span>
<span class="line"><span style="color:#CE9178;">    &quot;generatorConfig&quot;</span><span style="color:#D4D4D4;">: {</span></span>
<span class="line"><span style="color:#6A9955;">        // A set of objects keyed by generator name.</span></span>
<span class="line"><span style="color:#6A9955;">        // Generator names may optionally be qualified by their full namespace.</span></span>
<span class="line"><span style="color:#6A9955;">        // All generators are listed when running &#39;dotnet coalesce&#39; with &#39;--verbosity debug&#39;.</span></span>
<span class="line"><span style="color:#6A9955;">        // For example, &quot;Controllers&quot; or &quot;IntelliTect.Coalesce.CodeGeneration.Vue.Generators.Controllers&quot;.</span></span>
<span class="line"><span style="color:#CE9178;">        &quot;GeneratorName&quot;</span><span style="color:#9CDCFE;">:</span><span style="color:#D4D4D4;"> {</span></span>
<span class="line"><span style="color:#6A9955;">            // Optional: true if the generator should be disabled.</span></span>
<span class="line"><span style="color:#CE9178;">            &quot;disabled&quot;</span><span style="color:#9CDCFE;">:</span><span style="color:#569CD6;"> true</span><span style="color:#D4D4D4;">,</span></span>
<span class="line"><span style="color:#6A9955;">            // Optional: Configures a path relative to the default output path for the generator</span></span>
<span class="line"><span style="color:#6A9955;">            // where that generator&#39;s output should be placed instead.</span></span>
<span class="line"><span style="color:#CE9178;">            &quot;targetDirectory&quot;</span><span style="color:#9CDCFE;">:</span><span style="color:#CE9178;"> &quot;../DifferentFolder&quot;</span></span>
<span class="line"><span style="color:#6A9955;">            // Optional: Indentation size</span></span>
<span class="line"><span style="color:#CE9178;">            &quot;indentationSize&quot;</span><span style="color:#D4D4D4;">: </span><span style="color:#B5CEA8;">2</span><span style="color:#D4D4D4;"> </span></span>
<span class="line"><span style="color:#D4D4D4;">        },</span></span>
<span class="line"><span style="color:#D4D4D4;">    }</span></span>
<span class="line"><span style="color:#D4D4D4;">}</span></span></code></pre></div><h2 id="additional-cli-options" tabindex="-1">Additional CLI Options <a class="header-anchor" href="#additional-cli-options" aria-label="Permalink to &quot;Additional CLI Options&quot;">​</a></h2><p>There are a couple of extra options which are only available as CLI parameters to <code>dotnet coalesce</code>. These options do not affect the behavior of the code generation - only the behavior of the CLI itself.</p><p><code>--debug</code></p><p>When this flag is specified when running <code>dotnet coalesce</code>, Coalesce will wait up to 60 seconds for a debugger to be attached to its process before starting code generation.</p><p><code>-v|--verbosity &lt;level&gt;</code></p><p>Set the verbosity of the output. Options are <code>trace</code>, <code>debug</code>, <code>information</code>, <code>warning</code>, <code>error</code>, <code>critical</code>, and <code>none</code>.</p>`,14),t=[l];function p(c,i,r,u,d,y){return e(),n("div",null,t)}const h=s(o,[["render",p]]);export{D as __pageData,h as default};
