import{_ as l,D as p,o as t,c,I as r,w as a,R as n,k as s,a as e}from"./chunks/framework.g9eZ-ZSs.js";const I=JSON.parse('{"title":"Include Tree","description":"","frontmatter":{},"headers":[],"relativePath":"concepts/include-tree.md","filePath":"concepts/include-tree.md"}'),D={name:"concepts/include-tree.md"},y=n(`<h1 id="include-tree" tabindex="-1">Include Tree <a class="header-anchor" href="#include-tree" aria-label="Permalink to &quot;Include Tree&quot;">​</a></h1><p>When Coalesce maps from the your POCO objects that are returned from EF Core queries, it will follow a structure called an <code>IncludeTree</code> to determine what relationships to follow and how deep to go in re-creating that structure in the mapped DTOs.</p><h2 id="purpose" tabindex="-1">Purpose <a class="header-anchor" href="#purpose" aria-label="Permalink to &quot;Purpose&quot;">​</a></h2><p>Without an <code>IncludeTree</code> present, Coalesce will map the entire object graph that is reachable from the root object. This can often spiral out of control if there aren&#39;t any rules defining how far to go while turning this graph into a tree.</p><p>For example, suppose you had the following model with a many-to-many relationship (key properties omitted for brevity):</p><div class="language-c#"><button title="Copy Code" class="copy"></button><span class="lang">c#</span><pre class="shiki dark-plus vp-code"><code><span class="line"><span style="color:#569CD6;">public</span><span style="color:#569CD6;"> class</span><span style="color:#4EC9B0;"> Employee</span></span>
<span class="line"><span style="color:#D4D4D4;">{</span></span>
<span class="line"><span style="color:#D4D4D4;">    [</span><span style="color:#4EC9B0;">ManyToMany</span><span style="color:#D4D4D4;">(</span><span style="color:#CE9178;">&quot;Projects&quot;</span><span style="color:#D4D4D4;">)]</span></span>
<span class="line"><span style="color:#569CD6;">    public</span><span style="color:#4EC9B0;"> ICollection</span><span style="color:#D4D4D4;">&lt;</span><span style="color:#4EC9B0;">EmployeeProject</span><span style="color:#D4D4D4;">&gt; </span><span style="color:#9CDCFE;">EmployeeProjects</span><span style="color:#D4D4D4;"> { </span><span style="color:#569CD6;">get</span><span style="color:#D4D4D4;">; </span><span style="color:#569CD6;">set</span><span style="color:#D4D4D4;">; }</span></span>
<span class="line"><span style="color:#D4D4D4;">            </span></span>
<span class="line"><span style="color:#569CD6;">    public</span><span style="color:#569CD6;"> static</span><span style="color:#4EC9B0;"> IQueryable</span><span style="color:#D4D4D4;">&lt;</span><span style="color:#4EC9B0;">Employee</span><span style="color:#D4D4D4;">&gt; </span><span style="color:#DCDCAA;">WithProjectsAndMembers</span><span style="color:#D4D4D4;">(</span><span style="color:#4EC9B0;">AppDbContext</span><span style="color:#9CDCFE;"> db</span><span style="color:#D4D4D4;">, </span><span style="color:#4EC9B0;">ClaimsPrincipal</span><span style="color:#9CDCFE;"> user</span><span style="color:#D4D4D4;">)</span></span>
<span class="line"><span style="color:#D4D4D4;">    {</span></span>
<span class="line"><span style="color:#6A9955;">        // Load all projects of an employee, as well as all members of those projects.</span></span>
<span class="line"><span style="color:#C586C0;">        return</span><span style="color:#9CDCFE;"> db</span><span style="color:#D4D4D4;">.</span><span style="color:#9CDCFE;">Employees</span></span>
<span class="line"><span style="color:#D4D4D4;">            .</span><span style="color:#DCDCAA;">Include</span><span style="color:#D4D4D4;">(</span><span style="color:#9CDCFE;">e</span><span style="color:#D4D4D4;"> =&gt; </span><span style="color:#9CDCFE;">e</span><span style="color:#D4D4D4;">.</span><span style="color:#9CDCFE;">EmployeeProjects</span><span style="color:#D4D4D4;">)</span></span>
<span class="line"><span style="color:#D4D4D4;">                .</span><span style="color:#DCDCAA;">ThenInclude</span><span style="color:#D4D4D4;">(</span><span style="color:#9CDCFE;">ep</span><span style="color:#D4D4D4;"> =&gt; </span><span style="color:#9CDCFE;">ep</span><span style="color:#D4D4D4;">.</span><span style="color:#9CDCFE;">Project</span><span style="color:#D4D4D4;">.</span><span style="color:#9CDCFE;">EmployeeProjects</span><span style="color:#D4D4D4;">)</span></span>
<span class="line"><span style="color:#D4D4D4;">                .</span><span style="color:#DCDCAA;">ThenInclude</span><span style="color:#D4D4D4;">(</span><span style="color:#9CDCFE;">ep</span><span style="color:#D4D4D4;"> =&gt; </span><span style="color:#9CDCFE;">ep</span><span style="color:#D4D4D4;">.</span><span style="color:#9CDCFE;">Employee</span><span style="color:#D4D4D4;">);</span></span>
<span class="line"><span style="color:#D4D4D4;">    }</span></span>
<span class="line"><span style="color:#D4D4D4;">}</span></span>
<span class="line"></span>
<span class="line"><span style="color:#569CD6;">public</span><span style="color:#569CD6;"> class</span><span style="color:#4EC9B0;"> Project</span></span>
<span class="line"><span style="color:#D4D4D4;">{</span></span>
<span class="line"><span style="color:#D4D4D4;">    [</span><span style="color:#4EC9B0;">ManyToMany</span><span style="color:#D4D4D4;">(</span><span style="color:#CE9178;">&quot;Employees&quot;</span><span style="color:#D4D4D4;">)]</span></span>
<span class="line"><span style="color:#569CD6;">    public</span><span style="color:#4EC9B0;"> ICollection</span><span style="color:#D4D4D4;">&lt;</span><span style="color:#4EC9B0;">EmployeeProject</span><span style="color:#D4D4D4;">&gt; </span><span style="color:#9CDCFE;">EmployeeProjects</span><span style="color:#D4D4D4;"> { </span><span style="color:#569CD6;">get</span><span style="color:#D4D4D4;">; </span><span style="color:#569CD6;">set</span><span style="color:#D4D4D4;">; }</span></span>
<span class="line"><span style="color:#D4D4D4;">}</span></span>
<span class="line"></span>
<span class="line"><span style="color:#569CD6;">public</span><span style="color:#569CD6;"> class</span><span style="color:#4EC9B0;"> EmployeeProject</span></span>
<span class="line"><span style="color:#D4D4D4;">{</span></span>
<span class="line"><span style="color:#569CD6;">    public</span><span style="color:#4EC9B0;"> Employee</span><span style="color:#9CDCFE;"> Employee</span><span style="color:#D4D4D4;"> { </span><span style="color:#569CD6;">get</span><span style="color:#D4D4D4;">; </span><span style="color:#569CD6;">set</span><span style="color:#D4D4D4;">; }</span></span>
<span class="line"><span style="color:#569CD6;">    public</span><span style="color:#4EC9B0;"> Project</span><span style="color:#9CDCFE;"> Project</span><span style="color:#D4D4D4;"> { </span><span style="color:#569CD6;">get</span><span style="color:#D4D4D4;">; </span><span style="color:#569CD6;">set</span><span style="color:#D4D4D4;">; }</span></span>
<span class="line"><span style="color:#D4D4D4;">}</span></span></code></pre></div><p>Now, imagine that you have five employees and five projects, with every employee being a member of every project (i.e. there are 25 EmployeeProject rows).</p><p>Your client code makes a call to the Coalesce-generated API to load Employee #1 using the custom data source:</p>`,8),i=s("div",{class:"language-ts"},[s("button",{title:"Copy Code",class:"copy"}),s("span",{class:"lang"},"ts"),s("pre",{class:"shiki dark-plus vp-code"},[s("code",null,[s("span",{class:"line"},[s("span",{style:{color:"#C586C0"}},"import"),s("span",{style:{color:"#D4D4D4"}}," { "),s("span",{style:{color:"#9CDCFE"}},"Employee"),s("span",{style:{color:"#D4D4D4"}}," } "),s("span",{style:{color:"#C586C0"}},"from"),s("span",{style:{color:"#CE9178"}}," '@/viewmodels.g'")]),e(`
`),s("span",{class:"line"},[s("span",{style:{color:"#C586C0"}},"import"),s("span",{style:{color:"#D4D4D4"}}," { "),s("span",{style:{color:"#9CDCFE"}},"EmployeeViewModel"),s("span",{style:{color:"#D4D4D4"}}," } "),s("span",{style:{color:"#C586C0"}},"from"),s("span",{style:{color:"#CE9178"}}," '@/viewmodels.g'")]),e(`
`),s("span",{class:"line"}),e(`
`),s("span",{class:"line"},[s("span",{style:{color:"#569CD6"}},"var"),s("span",{style:{color:"#9CDCFE"}}," employee"),s("span",{style:{color:"#D4D4D4"}}," = "),s("span",{style:{color:"#569CD6"}},"new"),s("span",{style:{color:"#DCDCAA"}}," EmployeeViewModel"),s("span",{style:{color:"#D4D4D4"}},"();")]),e(`
`),s("span",{class:"line"},[s("span",{style:{color:"#9CDCFE"}},"employee"),s("span",{style:{color:"#D4D4D4"}},"."),s("span",{style:{color:"#9CDCFE"}},"$dataSource"),s("span",{style:{color:"#D4D4D4"}}," = "),s("span",{style:{color:"#569CD6"}},"new"),s("span",{style:{color:"#9CDCFE"}}," Employee"),s("span",{style:{color:"#D4D4D4"}},"."),s("span",{style:{color:"#9CDCFE"}},"DataSources"),s("span",{style:{color:"#D4D4D4"}},"."),s("span",{style:{color:"#DCDCAA"}},"WithProjectsAndMembers"),s("span",{style:{color:"#D4D4D4"}},"();")]),e(`
`),s("span",{class:"line"},[s("span",{style:{color:"#9CDCFE"}},"employee"),s("span",{style:{color:"#D4D4D4"}},"."),s("span",{style:{color:"#DCDCAA"}},"$load"),s("span",{style:{color:"#D4D4D4"}},"("),s("span",{style:{color:"#B5CEA8"}},"1"),s("span",{style:{color:"#D4D4D4"}},");")])])])],-1),d=s("div",{class:"language-ts"},[s("button",{title:"Copy Code",class:"copy"}),s("span",{class:"lang"},"ts"),s("pre",{class:"shiki dark-plus vp-code"},[s("code",null,[s("span",{class:"line"},[s("span",{style:{color:"#569CD6"}},"var"),s("span",{style:{color:"#9CDCFE"}}," employee"),s("span",{style:{color:"#D4D4D4"}}," = "),s("span",{style:{color:"#569CD6"}},"new"),s("span",{style:{color:"#9CDCFE"}}," ViewModels"),s("span",{style:{color:"#D4D4D4"}},"."),s("span",{style:{color:"#DCDCAA"}},"Employee"),s("span",{style:{color:"#D4D4D4"}},"();")]),e(`
`),s("span",{class:"line"},[s("span",{style:{color:"#9CDCFE"}},"employee"),s("span",{style:{color:"#D4D4D4"}},"."),s("span",{style:{color:"#9CDCFE"}},"dataSource"),s("span",{style:{color:"#D4D4D4"}}," = "),s("span",{style:{color:"#569CD6"}},"new"),s("span",{style:{color:"#9CDCFE"}}," employee"),s("span",{style:{color:"#D4D4D4"}},"."),s("span",{style:{color:"#9CDCFE"}},"dataSources"),s("span",{style:{color:"#D4D4D4"}},"."),s("span",{style:{color:"#DCDCAA"}},"WithProjectsAndMembers"),s("span",{style:{color:"#D4D4D4"}},"();")]),e(`
`),s("span",{class:"line"},[s("span",{style:{color:"#9CDCFE"}},"employee"),s("span",{style:{color:"#D4D4D4"}},"."),s("span",{style:{color:"#DCDCAA"}},"load"),s("span",{style:{color:"#D4D4D4"}},"("),s("span",{style:{color:"#B5CEA8"}},"1"),s("span",{style:{color:"#D4D4D4"}},");")])])])],-1),C=n(`<p>If you&#39;re already familiar with the fact that an <code>IncludeTree</code> is implicitly created in this scenario, then imagine for a moment that this is not the case (if you&#39;re not familiar with this fact, then keep reading!).</p><p>After Coalesce has called your <a href="/Coalesce/modeling/model-components/data-sources.html">Data Sources</a> and evaluated the EF IQueryable returned, there are now 35 objects loaded into the current <code>DbContext</code> being used to handle this request - the 5 employees, 5 projects, and 25 relationships.</p><p>To map these objects to DTOs, we start with the root (employee #1) and expand outward from there until the entire object graph has been faithfully re-created with DTO objects, including all navigation properties.</p><p>The root DTO object (employee #1) then eventually is passed to the JSON serializer by ASP.NET Core to formulate the response to the request. As the object is serialized to JSON, the only objects that are not serialized are those that were already serialized as an ancestor of itself. What this ultimately means is that the structure of the serialized JSON with our example scenario ends up following a pattern like this (the vast majority of items have been omitted):</p><div class="language-"><button title="Copy Code" class="copy"></button><span class="lang"></span><pre class="shiki dark-plus vp-code"><code><span class="line"><span>Employee#1</span></span>
<span class="line"><span>    EmployeeProject#1</span></span>
<span class="line"><span>        Project#1</span></span>
<span class="line"><span>            EmployeeProject#6</span></span>
<span class="line"><span>                Employee#2</span></span>
<span class="line"><span>                    EmployeeProject#7</span></span>
<span class="line"><span>                        Project#2</span></span>
<span class="line"><span>                            ... continues down through all remaining employees and projects.</span></span>
<span class="line"><span>                    ...</span></span>
<span class="line"><span>            EmployeeProject#11</span></span>
<span class="line"><span>                Employee#3</span></span>
<span class="line"><span>            ...</span></span>
<span class="line"><span>    EmployeeProject#2</span></span>
<span class="line"><span>        Project#2</span></span>
<span class="line"><span>    ...</span></span></code></pre></div><p>See how the structure includes the EmployeeProjects of Employee#2? We didn&#39;t write our custom data source calls to <code>.Include</code> in such a way that indicated that we wanted the root employee, their projects, the employees of those projects, and then <strong>the projects of those employees</strong>. But, because the JSON serializer blindly follows the object graph, that&#39;s what gets serialized. It turns out that the depth of the tree increases on the order of <code>O(n^2)</code>, and the total size increases on the order of <code>Ω(n!)</code>.</p><p>This is where <code>IncludeTree</code> comes in. When you use a custom data source like we did above, Coalesce automatically captures the structure of the calls to <code>.Include</code> and <code>.ThenInclude</code>, and uses this to perform trimming during creation of the DTO objects.</p><p>With an <code>IncludeTree</code> in place, our new serialized structure looks like this:</p><div class="language-"><button title="Copy Code" class="copy"></button><span class="lang"></span><pre class="shiki dark-plus vp-code"><code><span class="line"><span>Employee#1</span></span>
<span class="line"><span>    EmployeeProject#1</span></span>
<span class="line"><span>        Project#1</span></span>
<span class="line"><span>            EmployeeProject#6</span></span>
<span class="line"><span>                Employee#2</span></span>
<span class="line"><span>            EmployeeProject#11</span></span>
<span class="line"><span>                Employee#3</span></span>
<span class="line"><span>            ...</span></span>
<span class="line"><span>    EmployeeProject#2</span></span>
<span class="line"><span>        Project#2</span></span>
<span class="line"><span>    ...</span></span></code></pre></div><p>No more extra data trailing off the end of the projects&#39; employees!</p><h2 id="usage" tabindex="-1">Usage <a class="header-anchor" href="#usage" aria-label="Permalink to &quot;Usage&quot;">​</a></h2><h3 id="custom-data-sources" tabindex="-1">Custom Data Sources <a class="header-anchor" href="#custom-data-sources" aria-label="Permalink to &quot;Custom Data Sources&quot;">​</a></h3><p>In most cases, you don&#39;t have to worry about creating an <code>IncludeTree</code>. When using the <a href="/Coalesce/modeling/model-components/data-sources.html#standard-data-source">Standard Data Source</a> (or a derivative), the structure of the <code>.Include</code> and <code>.ThenInclude</code> calls will be captured automatically and be turned into an <code>IncludeTree</code>.</p><p>However, there are sometimes cases where you perform complex loading in these methods that involves loading data into the current <code>DbContext</code> outside of the <code>IQueryable</code> that is returned from the method. The most common situation for this is needing to conditionally load related data - for example, load all children of an object where the child has a certain value of a Status property.</p><p>In these cases, Coalesce provides a pair of extension methods, <code>.IncludedSeparately</code> and <code>.ThenIncluded</code>, that can be used to merge in the structure of the data that was loaded separately from the main <code>IQueryable</code>.</p><p>For example:</p><div class="language-c#"><button title="Copy Code" class="copy"></button><span class="lang">c#</span><pre class="shiki dark-plus vp-code"><code><span class="line"><span style="color:#569CD6;">public</span><span style="color:#569CD6;"> override</span><span style="color:#4EC9B0;"> IQueryable</span><span style="color:#D4D4D4;">&lt;</span><span style="color:#4EC9B0;">Employee</span><span style="color:#D4D4D4;">&gt; </span><span style="color:#DCDCAA;">GetQuery</span><span style="color:#D4D4D4;">()</span></span>
<span class="line"><span style="color:#D4D4D4;">{</span></span>
<span class="line"><span style="color:#6A9955;">    // Load all projects that are complete, and their members, into the db context.</span></span>
<span class="line"><span style="color:#9CDCFE;">    Db</span><span style="color:#D4D4D4;">.</span><span style="color:#9CDCFE;">Projects</span></span>
<span class="line"><span style="color:#D4D4D4;">        .</span><span style="color:#DCDCAA;">Include</span><span style="color:#D4D4D4;">(</span><span style="color:#9CDCFE;">p</span><span style="color:#D4D4D4;"> =&gt; </span><span style="color:#9CDCFE;">p</span><span style="color:#D4D4D4;">.</span><span style="color:#9CDCFE;">EmployeeProjects</span><span style="color:#D4D4D4;">).</span><span style="color:#DCDCAA;">ThenInclude</span><span style="color:#D4D4D4;">(</span><span style="color:#9CDCFE;">ep</span><span style="color:#D4D4D4;"> =&gt; </span><span style="color:#9CDCFE;">ep</span><span style="color:#D4D4D4;">.</span><span style="color:#9CDCFE;">Employee</span><span style="color:#D4D4D4;">)</span></span>
<span class="line"><span style="color:#D4D4D4;">        .</span><span style="color:#DCDCAA;">Where</span><span style="color:#D4D4D4;">(</span><span style="color:#9CDCFE;">p</span><span style="color:#D4D4D4;"> =&gt; </span><span style="color:#9CDCFE;">p</span><span style="color:#D4D4D4;">.</span><span style="color:#9CDCFE;">Status</span><span style="color:#D4D4D4;"> == </span><span style="color:#9CDCFE;">ProjectStatus</span><span style="color:#D4D4D4;">.</span><span style="color:#9CDCFE;">Complete</span><span style="color:#D4D4D4;">)</span></span>
<span class="line"><span style="color:#D4D4D4;">        .</span><span style="color:#DCDCAA;">Load</span><span style="color:#D4D4D4;">();</span></span>
<span class="line"></span>
<span class="line"><span style="color:#6A9955;">    // Return an employee query, and notify Coalesce that we loaded the projects in a different query.</span></span>
<span class="line"><span style="color:#C586C0;">    return</span><span style="color:#9CDCFE;"> Db</span><span style="color:#D4D4D4;">.</span><span style="color:#9CDCFE;">Employees</span></span>
<span class="line"><span style="color:#D4D4D4;">        .</span><span style="color:#DCDCAA;">IncludedSeparately</span><span style="color:#D4D4D4;">(</span><span style="color:#9CDCFE;">e</span><span style="color:#D4D4D4;"> =&gt; </span><span style="color:#9CDCFE;">e</span><span style="color:#D4D4D4;">.</span><span style="color:#9CDCFE;">EmployeeProjects</span><span style="color:#D4D4D4;">)</span></span>
<span class="line"><span style="color:#D4D4D4;">        .</span><span style="color:#DCDCAA;">ThenIncluded</span><span style="color:#D4D4D4;">(</span><span style="color:#9CDCFE;">ep</span><span style="color:#D4D4D4;"> =&gt; </span><span style="color:#9CDCFE;">ep</span><span style="color:#D4D4D4;">.</span><span style="color:#9CDCFE;">Project</span><span style="color:#D4D4D4;">.</span><span style="color:#9CDCFE;">EmployeeProjects</span><span style="color:#D4D4D4;">)</span></span>
<span class="line"><span style="color:#D4D4D4;">        .</span><span style="color:#DCDCAA;">ThenIncluded</span><span style="color:#D4D4D4;">(</span><span style="color:#9CDCFE;">ep</span><span style="color:#D4D4D4;"> =&gt; </span><span style="color:#9CDCFE;">ep</span><span style="color:#D4D4D4;">.</span><span style="color:#9CDCFE;">Employee</span><span style="color:#D4D4D4;">);</span></span>
<span class="line"><span style="color:#D4D4D4;">}</span></span></code></pre></div><p>You can also override the <code>GetIncludeTree</code> method of the <a href="/Coalesce/modeling/model-components/data-sources.html#standard-data-source">Standard Data Source</a> to achieve the same result:</p><div class="language-c#"><button title="Copy Code" class="copy"></button><span class="lang">c#</span><pre class="shiki dark-plus vp-code"><code><span class="line"><span style="color:#569CD6;">public</span><span style="color:#569CD6;"> override</span><span style="color:#4EC9B0;"> IncludeTree</span><span style="color:#DCDCAA;"> GetIncludeTree</span><span style="color:#D4D4D4;">(</span><span style="color:#4EC9B0;">IQueryable</span><span style="color:#D4D4D4;">&lt;</span><span style="color:#4EC9B0;">T</span><span style="color:#D4D4D4;">&gt; </span><span style="color:#9CDCFE;">query</span><span style="color:#D4D4D4;">, </span><span style="color:#4EC9B0;">IDataSourceParameters</span><span style="color:#9CDCFE;"> parameters</span><span style="color:#D4D4D4;">) =&gt; </span><span style="color:#9CDCFE;">Db</span></span>
<span class="line"><span style="color:#D4D4D4;">    .</span><span style="color:#9CDCFE;">Employees</span></span>
<span class="line"><span style="color:#D4D4D4;">    .</span><span style="color:#DCDCAA;">IncludedSeparately</span><span style="color:#D4D4D4;">(</span><span style="color:#9CDCFE;">e</span><span style="color:#D4D4D4;"> =&gt; </span><span style="color:#9CDCFE;">e</span><span style="color:#D4D4D4;">.</span><span style="color:#9CDCFE;">EmployeeProjects</span><span style="color:#D4D4D4;">)</span></span>
<span class="line"><span style="color:#D4D4D4;">    .</span><span style="color:#DCDCAA;">ThenIncluded</span><span style="color:#D4D4D4;">(</span><span style="color:#9CDCFE;">ep</span><span style="color:#D4D4D4;"> =&gt; </span><span style="color:#9CDCFE;">ep</span><span style="color:#D4D4D4;">.</span><span style="color:#9CDCFE;">Project</span><span style="color:#D4D4D4;">.</span><span style="color:#9CDCFE;">EmployeeProjects</span><span style="color:#D4D4D4;">)</span></span>
<span class="line"><span style="color:#D4D4D4;">    .</span><span style="color:#DCDCAA;">ThenIncluded</span><span style="color:#D4D4D4;">(</span><span style="color:#9CDCFE;">ep</span><span style="color:#D4D4D4;"> =&gt; </span><span style="color:#9CDCFE;">ep</span><span style="color:#D4D4D4;">.</span><span style="color:#9CDCFE;">Employee</span><span style="color:#D4D4D4;">)</span></span>
<span class="line"><span style="color:#D4D4D4;">    .</span><span style="color:#DCDCAA;">GetIncludeTree</span><span style="color:#D4D4D4;">();</span></span></code></pre></div><h3 id="model-methods" tabindex="-1">Model Methods <a class="header-anchor" href="#model-methods" aria-label="Permalink to &quot;Model Methods&quot;">​</a></h3><p>If you have <a href="/Coalesce/modeling/model-components/methods.html">custom methods</a> that return object data, you may also want to control the structure of the returned data when it is serialized. Fortunately, you can also use <code>IncludeTree</code> in these situations. Without an <code>IncludeTree</code>, the entire object graph is traversed and serialized without limit.</p><div class="tip custom-block"><p class="custom-block-title">TIP</p><p>An <code>IncludeTree</code> can be obtained from any <code>IQueryable</code> by calling the <code>GetIncludeTree</code> extension method (<code>using IntelliTect.Coalesce.Helpers.IncludeTree</code>).</p><p>In situations where your root object isn&#39;t on your <code>DbContext</code> (see <a href="/Coalesce/modeling/model-types/external-types.html">External Types</a>), you can use <code>Enumerable.Empty&lt;MyNonDbClass&gt;().AsQueryable()</code> to get an <code>IQueryable</code> to start from. When you do this, you <strong>must</strong> use <code>IncludedSeparately</code> - the regular EF <code>Include</code> method won&#39;t work without a <code>DbSet</code>.</p></div><p>See the following two techniques for returning an <code>IncludeTree</code> from a custom method:</p><h4 id="itemresult-includetree" tabindex="-1">ItemResult.IncludeTree <a class="header-anchor" href="#itemresult-includetree" aria-label="Permalink to &quot;ItemResult.IncludeTree&quot;">​</a></h4><p>The easiest and most versatile way to return an <code>IncludeTree</code> from a custom method is to make that method return an <code>ItemResult&lt;T&gt;</code>, and then set the <code>IncludeTree</code> property of the <code>ItemResult</code> object. For example:</p><div class="language-c#"><button title="Copy Code" class="copy"></button><span class="lang">c#</span><pre class="shiki dark-plus vp-code"><code><span class="line"><span style="color:#569CD6;">public</span><span style="color:#569CD6;"> class</span><span style="color:#4EC9B0;"> Employee</span></span>
<span class="line"><span style="color:#D4D4D4;">{</span></span>
<span class="line"><span style="color:#569CD6;">    public</span><span style="color:#569CD6;"> async</span><span style="color:#4EC9B0;"> Task</span><span style="color:#D4D4D4;">&lt;</span><span style="color:#4EC9B0;">ItemResult</span><span style="color:#D4D4D4;">&lt;</span><span style="color:#4EC9B0;">ICollection</span><span style="color:#D4D4D4;">&lt;</span><span style="color:#4EC9B0;">Employee</span><span style="color:#D4D4D4;">&gt;&gt;&gt; </span><span style="color:#DCDCAA;">GetChainOfCommand</span><span style="color:#D4D4D4;">(</span><span style="color:#4EC9B0;">AppDbContext</span><span style="color:#9CDCFE;"> db</span><span style="color:#D4D4D4;">)</span></span>
<span class="line"><span style="color:#D4D4D4;">    {</span></span>
<span class="line"><span style="color:#4EC9B0;">        IQueryable</span><span style="color:#D4D4D4;">&lt;</span><span style="color:#4EC9B0;">Employee</span><span style="color:#D4D4D4;">&gt; </span><span style="color:#9CDCFE;">query</span><span style="color:#D4D4D4;"> = </span><span style="color:#9CDCFE;">db</span><span style="color:#D4D4D4;">.</span><span style="color:#9CDCFE;">Employees</span></span>
<span class="line"><span style="color:#D4D4D4;">            .</span><span style="color:#DCDCAA;">Include</span><span style="color:#D4D4D4;">(</span><span style="color:#9CDCFE;">e</span><span style="color:#D4D4D4;"> =&gt; </span><span style="color:#9CDCFE;">e</span><span style="color:#D4D4D4;">.</span><span style="color:#9CDCFE;">Supervisor</span><span style="color:#D4D4D4;">);</span></span>
<span class="line"></span>
<span class="line"><span style="color:#569CD6;">        var</span><span style="color:#9CDCFE;"> ret</span><span style="color:#D4D4D4;"> = </span><span style="color:#569CD6;">new</span><span style="color:#4EC9B0;"> List</span><span style="color:#D4D4D4;">&lt;</span><span style="color:#4EC9B0;">Employee</span><span style="color:#D4D4D4;">&gt;();</span></span>
<span class="line"><span style="color:#569CD6;">        var</span><span style="color:#9CDCFE;"> current</span><span style="color:#D4D4D4;"> = </span><span style="color:#569CD6;">this</span><span style="color:#D4D4D4;">;</span></span>
<span class="line"><span style="color:#C586C0;">        while</span><span style="color:#D4D4D4;"> (</span><span style="color:#9CDCFE;">current</span><span style="color:#D4D4D4;">.</span><span style="color:#9CDCFE;">Supervisor</span><span style="color:#D4D4D4;"> != </span><span style="color:#569CD6;">null</span><span style="color:#D4D4D4;">)</span></span>
<span class="line"><span style="color:#D4D4D4;">        {</span></span>
<span class="line"><span style="color:#9CDCFE;">            ret</span><span style="color:#D4D4D4;">.</span><span style="color:#DCDCAA;">Push</span><span style="color:#D4D4D4;">(</span><span style="color:#9CDCFE;">current</span><span style="color:#D4D4D4;">);</span></span>
<span class="line"><span style="color:#9CDCFE;">            current</span><span style="color:#D4D4D4;"> = </span><span style="color:#569CD6;">await</span><span style="color:#9CDCFE;"> query</span><span style="color:#D4D4D4;">.</span><span style="color:#DCDCAA;">FirstOrDefaultAsync</span><span style="color:#D4D4D4;">(</span><span style="color:#9CDCFE;">e</span><span style="color:#D4D4D4;"> =&gt; </span><span style="color:#9CDCFE;">e</span><span style="color:#D4D4D4;">.</span><span style="color:#9CDCFE;">EmployeeId</span><span style="color:#D4D4D4;"> == </span><span style="color:#9CDCFE;">current</span><span style="color:#D4D4D4;">.</span><span style="color:#9CDCFE;">SupervisorId</span><span style="color:#D4D4D4;">);</span></span>
<span class="line"><span style="color:#D4D4D4;">        }</span></span>
<span class="line"></span>
<span class="line"><span style="color:#C586C0;">        return</span><span style="color:#569CD6;"> new</span><span style="color:#D4D4D4;">(</span><span style="color:#9CDCFE;">ret</span><span style="color:#D4D4D4;">, </span><span style="color:#9CDCFE;">includeTree</span><span style="color:#D4D4D4;">: </span><span style="color:#9CDCFE;">query</span><span style="color:#D4D4D4;">.</span><span style="color:#DCDCAA;">GetIncludeTree</span><span style="color:#D4D4D4;">());</span></span>
<span class="line"><span style="color:#D4D4D4;">    }</span></span>
<span class="line"><span style="color:#D4D4D4;">}</span></span></code></pre></div><h4 id="out-parameter" tabindex="-1">Out Parameter <a class="header-anchor" href="#out-parameter" aria-label="Permalink to &quot;Out Parameter&quot;">​</a></h4><p>To tell Coalesce about the structure of the data returned from a model method, you can also add <code>out IncludeTree includeTree</code> to the signature of the method. Inside your method, set <code>includeTree</code> to an instance of an <code>IncludeTree</code>. However, this approach cannot be used on <code>async</code> methods, since <code>out</code> parameters are not allowed on async methods in C#. For example:</p><div class="language-c#"><button title="Copy Code" class="copy"></button><span class="lang">c#</span><pre class="shiki dark-plus vp-code"><code><span class="line"><span style="color:#569CD6;">public</span><span style="color:#569CD6;"> class</span><span style="color:#4EC9B0;"> Employee</span></span>
<span class="line"><span style="color:#D4D4D4;">{</span></span>
<span class="line"><span style="color:#569CD6;">    public</span><span style="color:#4EC9B0;"> ICollection</span><span style="color:#D4D4D4;">&lt;</span><span style="color:#4EC9B0;">Employee</span><span style="color:#D4D4D4;">&gt; </span><span style="color:#DCDCAA;">GetChainOfCommand</span><span style="color:#D4D4D4;">(</span><span style="color:#4EC9B0;">AppDbContext</span><span style="color:#9CDCFE;"> db</span><span style="color:#D4D4D4;">, </span><span style="color:#569CD6;">out</span><span style="color:#4EC9B0;"> IncludeTree</span><span style="color:#9CDCFE;"> includeTree</span><span style="color:#D4D4D4;">)</span></span>
<span class="line"><span style="color:#D4D4D4;">    {</span></span>
<span class="line"><span style="color:#4EC9B0;">        IQueryable</span><span style="color:#D4D4D4;">&lt;</span><span style="color:#4EC9B0;">Employee</span><span style="color:#D4D4D4;">&gt; </span><span style="color:#9CDCFE;">query</span><span style="color:#D4D4D4;"> = </span><span style="color:#9CDCFE;">db</span><span style="color:#D4D4D4;">.</span><span style="color:#9CDCFE;">Employees</span></span>
<span class="line"><span style="color:#D4D4D4;">            .</span><span style="color:#DCDCAA;">Include</span><span style="color:#D4D4D4;">(</span><span style="color:#9CDCFE;">e</span><span style="color:#D4D4D4;"> =&gt; </span><span style="color:#9CDCFE;">e</span><span style="color:#D4D4D4;">.</span><span style="color:#9CDCFE;">Supervisor</span><span style="color:#D4D4D4;">);</span></span>
<span class="line"></span>
<span class="line"><span style="color:#569CD6;">        var</span><span style="color:#9CDCFE;"> ret</span><span style="color:#D4D4D4;"> = </span><span style="color:#569CD6;">new</span><span style="color:#4EC9B0;"> List</span><span style="color:#D4D4D4;">&lt;</span><span style="color:#4EC9B0;">Employee</span><span style="color:#D4D4D4;">&gt;();</span></span>
<span class="line"><span style="color:#569CD6;">        var</span><span style="color:#9CDCFE;"> current</span><span style="color:#D4D4D4;"> = </span><span style="color:#569CD6;">this</span><span style="color:#D4D4D4;">;</span></span>
<span class="line"><span style="color:#C586C0;">        while</span><span style="color:#D4D4D4;"> (</span><span style="color:#9CDCFE;">current</span><span style="color:#D4D4D4;">.</span><span style="color:#9CDCFE;">Supervisor</span><span style="color:#D4D4D4;"> != </span><span style="color:#569CD6;">null</span><span style="color:#D4D4D4;">)</span></span>
<span class="line"><span style="color:#D4D4D4;">        {</span></span>
<span class="line"><span style="color:#9CDCFE;">            ret</span><span style="color:#D4D4D4;">.</span><span style="color:#DCDCAA;">Push</span><span style="color:#D4D4D4;">(</span><span style="color:#9CDCFE;">current</span><span style="color:#D4D4D4;">);</span></span>
<span class="line"><span style="color:#9CDCFE;">            current</span><span style="color:#D4D4D4;"> = </span><span style="color:#9CDCFE;">query</span><span style="color:#D4D4D4;">.</span><span style="color:#DCDCAA;">FirstOrDefault</span><span style="color:#D4D4D4;">(</span><span style="color:#9CDCFE;">e</span><span style="color:#D4D4D4;"> =&gt; </span><span style="color:#9CDCFE;">e</span><span style="color:#D4D4D4;">.</span><span style="color:#9CDCFE;">EmployeeId</span><span style="color:#D4D4D4;"> == </span><span style="color:#9CDCFE;">current</span><span style="color:#D4D4D4;">.</span><span style="color:#9CDCFE;">SupervisorId</span><span style="color:#D4D4D4;">);</span></span>
<span class="line"><span style="color:#D4D4D4;">        }</span></span>
<span class="line"></span>
<span class="line"><span style="color:#9CDCFE;">        includeTree</span><span style="color:#D4D4D4;"> = </span><span style="color:#9CDCFE;">query</span><span style="color:#D4D4D4;">.</span><span style="color:#DCDCAA;">GetIncludeTree</span><span style="color:#D4D4D4;">();</span></span>
<span class="line"></span>
<span class="line"><span style="color:#C586C0;">        return</span><span style="color:#9CDCFE;"> ret</span><span style="color:#D4D4D4;">;</span></span>
<span class="line"><span style="color:#D4D4D4;">    }</span></span>
<span class="line"><span style="color:#D4D4D4;">}</span></span></code></pre></div><h3 id="external-type-caveats" tabindex="-1">External Type Caveats <a class="header-anchor" href="#external-type-caveats" aria-label="Permalink to &quot;External Type Caveats&quot;">​</a></h3><p>One important point remains regarding <code>IncludeTree</code> - it is not used to control the serialization of objects which are not mapped to the database, known as <a href="/Coalesce/modeling/model-types/external-types.html">External Types</a>. External Types are always put into the DTOs when encountered (unless otherwise prevented by <a href="/Coalesce/modeling/model-components/attributes/dto-includes-excludes.html">[DtoIncludes] &amp; [DtoExcludes]</a> or <a href="/Coalesce/modeling/model-components/attributes/security-attribute.html">Security Attributes</a>), with the assumption that because these objects are created by you (as opposed to Entity Framework), you are responsible for preventing any undesired circular references.</p><p>By not filtering unmapped properties, you as the developer don&#39;t need to account for them in every place throughout your application where they appear - instead, they &#39;just work&#39; and show up on the client as expected.</p><p>Note also that this statement does not apply to database-mapped objects that hang off of unmapped objects - any time a database-mapped object appears, it will be controlled by your include tree. If no include tree is present (because nothing was specified for the unmapped property), these mapped objects hanging off of unmapped objects will be serialized freely and with all circular references, unless you include some calls to <code>.IncludedSeparately(m =&gt; m.MyUnmappedProperty.MyMappedProperty)</code> to limit those objects down.</p>`,33);function u(h,m,E,b,g,F){const o=p("CodeTabs");return t(),c("div",null,[y,r(o,null,{vue:a(()=>[i]),knockout:a(()=>[d]),_:1}),C])}const A=l(D,[["render",u]]);export{I as __pageData,A as default};