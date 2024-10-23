import{_ as e,c as a,o as s,a7 as t}from"./chunks/framework.BkavzUpE.js";const y=JSON.parse('{"title":"Immutability","description":"","frontmatter":{},"headers":[],"relativePath":"topics/immutability.md","filePath":"topics/immutability.md"}'),n={name:"topics/immutability.md"},o=t(`<h1 id="immutability" tabindex="-1">Immutability <a class="header-anchor" href="#immutability" aria-label="Permalink to &quot;Immutability&quot;">​</a></h1><p>Immutability of data is an essential consideration of almost any system - it is usually important there is confidence in the correctness of historical data.</p><p>For example, a record of an invoice must not change when the prices for the line items are updated as the future progresses, and if a purchased item is later refunded, none of the data about the original purchase should be changed. Only new information describing the refund should be added to the database, including details about the refund and some indication of &quot;refunded&quot; on the original item (like a foreign key referencing the refund transaction)</p><p>It is ultimately up to each individual use case when deciding when data needs to be immutable, but at the very least, any data involving financial transactions and any data where auditing is a major concern should be immutable to at least some extent.</p><p>This page explores some techniques to achieve immutability in a Coalesce application.</p><h2 id="configuration-data-vs-transactional-data" tabindex="-1">Configuration data vs Transactional data <a class="header-anchor" href="#configuration-data-vs-transactional-data" aria-label="Permalink to &quot;Configuration data vs Transactional data&quot;">​</a></h2><h3 id="transactional-data" tabindex="-1">Transactional Data <a class="header-anchor" href="#transactional-data" aria-label="Permalink to &quot;Transactional Data&quot;">​</a></h3><p>In an application, transaction data is any data that represents an event or action that occurred at a particular time. A purchase, an invoice or account statement, a message or email, an audit or error log, a calendar event - these are all examples of transactional data.</p><p>For purposes of this exercise, we&#39;re also lumping non-configuration master data under the umbrella of transactional data. For example, a master Customer record, or a User record.</p><h3 id="configuration-data" tabindex="-1">Configuration Data <a class="header-anchor" href="#configuration-data" aria-label="Permalink to &quot;Configuration Data&quot;">​</a></h3><p>Configuration data (sometimes categorized under the umbrella of master data) is the data in a system that informs how transactions occur. The current price of an item, any kind of template, and even singleton configuration like a site&#39;s theme and colors.</p><p>If you have configuration data that is linked to transactional data and affects how that transactional data is interpreted, that configuration data becomes a strong candidate for immutability. For example, an <code>InvoiceLine</code> record that references a <code>Product</code> by foreign key instead of having columns on <code>InvoiceLine</code> to hold the price and description of the product - any future updates to the product should not affect past invoices.</p><h2 id="general-techniques" tabindex="-1">General Techniques <a class="header-anchor" href="#general-techniques" aria-label="Permalink to &quot;General Techniques&quot;">​</a></h2><p>The following immutability techniques are applicable to both transactional and configuration data.</p><h3 id="disable-edits" tabindex="-1">Disable edits <a class="header-anchor" href="#disable-edits" aria-label="Permalink to &quot;Disable edits&quot;">​</a></h3><p>One of the simplest ways to enforce immutability of data is to simply prevent it from being edited (and deleted, if applicable) at all.</p><p>Instead of using incremental saves (e.g. through <a href="/Coalesce/stacks/vue/layers/viewmodels.html#auto-save">auto-saves</a>), only create completed records or sets of records with an explicit <a href="/Coalesce/stacks/vue/layers/viewmodels.html#saving-and-deleting">save</a> or <a href="/Coalesce/stacks/vue/layers/viewmodels.html#bulk-saves">bulk save</a> operation, or a <a href="/Coalesce/modeling/model-components/methods.html">custom method</a>. Disable edits entirely to the immutable entities with <a href="/Coalesce/modeling/model-components/attributes/security-attribute.html#edit">security attributes</a>.</p><p>Unfortunately, this approach is usually not feasible for anything but the simplest of data models. There are usually additional considerations to be had, including:</p><ul><li>Completely immutable hierarchical data models can be unreasonably difficult to work with, requiring a full clone of the hierarchy to make the smallest of change (although this <em>could</em> be a desirable characteristic, depending on the use case).</li><li>There&#39;s no ability to save in-progress or &quot;draft&quot; state. All data must be created all at once.</li></ul><h3 id="editable-until-finalized" tabindex="-1">Editable until finalized <a class="header-anchor" href="#editable-until-finalized" aria-label="Permalink to &quot;Editable until finalized&quot;">​</a></h3><p>A more nuanced approach to immutability is to only disable editing once a record has reached a &quot;finalized&quot; state. For example, an order that has transitioned from a &quot;shopping cart&quot; to a submitted, paid-for order. Or, a set of configuration that has transitioned from a &quot;draft&quot; state to a &quot;published&quot; state.</p><p>Using <a href="/Coalesce/modeling/model-components/behaviors.html">Behaviors</a> on all entities in a hierarchy, prohibit any undesirable edits by overriding the <code>BeforeSave</code> or <code>BeforeSaveAsync</code> method and checking the state of the record in the database to determine if edits are permissible.</p><p>This technique has the advantage of being infinitely customizable, allowing for scenarios like administrative overrides of records that would otherwise be uneditable by an unprivileged user.</p><h3 id="soft-deletes" tabindex="-1">Soft deletes <a class="header-anchor" href="#soft-deletes" aria-label="Permalink to &quot;Soft deletes&quot;">​</a></h3><p>While not a immutability strategy on its own, implementing immutability usually requires the prevention hard deletes of existing records. However, the ability to retire or archive old records using soft deletes is still valuable and doesn&#39;t violate the principals of immutability. Doing so is fairly straightforward in Coalesce:</p><ul><li>Add a property to the type to indicate soft delete status (usually a <code>DateTimeOffset? DeletedDate { get; set; }</code>)</li><li>Choose how soft deletes will occur: <ul><li>To soft-delete items using the built-in <code>/delete</code> endpoint and <code>$delete</code> API on <code>ViewModel</code> instances, override <code>ExecuteDeleteAsync</code> on the type&#39;s <a href="/Coalesce/modeling/model-components/behaviors.html">Behaviors</a> to set the <code>DeletedDate</code> and call <code>db.SaveChangesAsync()</code>. Do not call the base ExecuteDeleteAsync method (which will perform a hard delete). This approach also makes the Delete button in <a href="/Coalesce/stacks/vue/coalesce-vue-vuetify/overview.html#admin-components">admin pages</a> perform a soft delete.</li><li>Otherwise, set the soft delete flag using regular saves, just as you would change any other property. Don&#39;t forget to implement security restrictions around who can delete and un-delete records if that&#39;s important to your application.</li></ul></li><li>Filter out soft-deleted values from selection in your custom UI pages. There are a few options here: <ul><li><p>The simplest way is to use Coalesce&#39;s built-in <a href="/Coalesce/modeling/model-components/data-sources.html#member-applylistpropertyfilter">filtering</a> to exclude soft-deleted items. This can be done from a <a href="/Coalesce/stacks/vue/layers/viewmodels.html#member-item-_params">ListViewModel&#39;s $params.filter</a>:</p><div class="language-ts"><button title="Copy Code" class="copy"></button><span class="lang">ts</span><pre class="shiki dark-plus vp-code"><code><span class="line"><span style="color:#569CD6;">const</span><span style="color:#4FC1FF;"> list</span><span style="color:#D4D4D4;"> = </span><span style="color:#569CD6;">new</span><span style="color:#DCDCAA;"> ItemTypeListViewModel</span><span style="color:#D4D4D4;">();</span></span>
<span class="line"><span style="color:#9CDCFE;">list</span><span style="color:#D4D4D4;">.</span><span style="color:#9CDCFE;">$params</span><span style="color:#D4D4D4;">.</span><span style="color:#9CDCFE;">filter</span><span style="color:#D4D4D4;">.</span><span style="color:#9CDCFE;">deletedDate</span><span style="color:#D4D4D4;"> = </span><span style="color:#569CD6;">null</span><span style="color:#D4D4D4;">;</span></span></code></pre></div><p>...or be passed directly to a <a href="/Coalesce/stacks/vue/coalesce-vue-vuetify/components/c-select.html#member-params"><code>c-input</code> or <code>c-select</code></a>:</p><div class="language-template"><button title="Copy Code" class="copy"></button><span class="lang">template</span><pre class="shiki dark-plus vp-code"><code><span class="line"><span style="color:#808080;">&lt;</span><span style="color:#569CD6;">c-select</span><span style="color:#D4D4D4;"> </span></span>
<span class="line"><span style="color:#D4D4D4;">  :</span><span style="color:#9CDCFE;">model</span><span style="color:#D4D4D4;">=</span><span style="color:#D4D4D4;">&quot;</span><span style="color:#9CDCFE;">item</span><span style="color:#D4D4D4;">&quot;</span><span style="color:#D4D4D4;"> </span></span>
<span class="line"><span style="color:#9CDCFE;">  for</span><span style="color:#D4D4D4;">=</span><span style="color:#CE9178;">&quot;itemType&quot;</span><span style="color:#D4D4D4;"> </span></span>
<span class="line"><span style="color:#D4D4D4;">  :</span><span style="color:#9CDCFE;">params</span><span style="color:#D4D4D4;">=</span><span style="color:#D4D4D4;">&quot;</span><span style="color:#D4D4D4;">{ </span><span style="color:#9CDCFE;">filter:</span><span style="color:#D4D4D4;"> { </span><span style="color:#9CDCFE;">deletedDate:</span><span style="color:#569CD6;"> null</span><span style="color:#D4D4D4;"> } }</span><span style="color:#D4D4D4;">&quot;</span><span style="color:#D4D4D4;"> </span></span>
<span class="line"><span style="color:#D4D4D4;">/</span><span style="color:#808080;">&gt;</span></span></code></pre></div></li><li><p>You can also use a <a href="/Coalesce/modeling/model-components/data-sources.html">custom data source</a> if your needs around excluding soft-deleted items are more complex. For example, if there are certain classes of users in your application who should not be allowed to read soft-deleted items, enforce that in the <a href="/Coalesce/modeling/model-components/data-sources.html#defining-data-sources">default data source</a> for the type.</p></li></ul></li></ul><h2 id="configuration-immutability" tabindex="-1">Configuration Immutability <a class="header-anchor" href="#configuration-immutability" aria-label="Permalink to &quot;Configuration Immutability&quot;">​</a></h2><p>In addition to the general techniques above, the following are approaches are specifically relevant to handling configuration data:</p><h3 id="enforce-nothing-document-consequences" tabindex="-1">Enforce nothing, document consequences <a class="header-anchor" href="#enforce-nothing-document-consequences" aria-label="Permalink to &quot;Enforce nothing, document consequences&quot;">​</a></h3><p>The simplest and riskiest approach is to enforce nothing in the application, but ensure that configuration administrators are aware of the consequences of changing configuration that could have unintended consequences.</p><p>For example, in a scenario with a transactional &quot;Item&quot; record and a configuration &quot;ItemType&quot; record, changing the name of the ItemType would affect the apparent type of all existing Item records that use that type. This can be OK if modifications are performed with this understanding as to not alter the meaning of existing data, but can have undesirable consequences if an existing ItemType is renamed to something completely unrelated.</p><h3 id="disable-edits-1" tabindex="-1">Disable edits <a class="header-anchor" href="#disable-edits-1" aria-label="Permalink to &quot;Disable edits&quot;">​</a></h3><p>The next simplest approach is to make configuration records fully immutable by disabling edits and hard deletes using <a href="/Coalesce/modeling/model-components/attributes/security-attribute.html#edit">security attributes</a>. This is largely foolproof, but comes with the same drawbacks <a href="#disable-edits">as described above</a>.</p><p>For simple cases like a table having not much more than a <code>Name</code> column that provides values in a dropdown, the burden on configuration administrators is usually small. However, for more complex configuration - especially hierarchical configuration - the burden imparted by pure immutability is often unreasonably high. For these scenarios, continue reading the next sections.</p><h3 id="editable-until-used" tabindex="-1">Editable until used <a class="header-anchor" href="#editable-until-used" aria-label="Permalink to &quot;Editable until used&quot;">​</a></h3><p>As an extension of the <a href="#editable-until-finalized">Editable until finalized</a> technique described above, configuration data could be left editable as long as it has not yet been referenced by any transactional data.</p><p>This can be useful for scenarios where a formal &quot;publish&quot; state for the configuration is excessive. Pick-lists for selection in a dropdown, for example, can benefit from this approach by allowing values to be created and worked on as long as changes would not affect the meaning of any existing transactional data that references those values.</p><p>To enforce this, use <a href="/Coalesce/modeling/model-components/behaviors.html">Behaviors</a> to block edits to in-use configuration by looking in the database for uses of the configuration record being edited.</p><h3 id="copy-onto-transactional-records" tabindex="-1">Copy onto transactional records <a class="header-anchor" href="#copy-onto-transactional-records" aria-label="Permalink to &quot;Copy onto transactional records&quot;">​</a></h3><p>Another strategy for dealing with configuration changes is to leave configuration records mutable, but copy the important configuration values onto each transactional record as transactions occur.</p><p>This can work great for financial records especially - when a purchase is finalized and paid for, copy the fields like price and description onto each line item in the purchase. This way, future updates to products do not affect past purchases of that item.</p><p>Ensure that the properties in your transactional records that will hold the snapshotted configuration <a href="/Coalesce/modeling/model-components/attributes/security-attribute.html#read">are immutable</a>, then populate these properties from configuration in your backend code (custom methods, services, or behaviors) when transactional records are created.</p><h3 id="versioned-configuration" tabindex="-1">Versioned configuration <a class="header-anchor" href="#versioned-configuration" aria-label="Permalink to &quot;Versioned configuration&quot;">​</a></h3><p>A more advanced but more powerful system of configuration is to use versioned configuration.</p><p>In this approach, there are two tables: A primary configuration table that is freely mutable, and a second table that is versioned and immutable. The primary table keeps track of active version of configuration as well as any configuration that does not need to be versioned or kept immutable, while the records in the versioned table are what get associated to transaction data that relies on the configuration.</p><p>For example:</p><div class="language-c#"><button title="Copy Code" class="copy"></button><span class="lang">c#</span><pre class="shiki dark-plus vp-code"><code><span class="line"></span>
<span class="line"><span style="color:#D4D4D4;">[</span><span style="color:#4EC9B0;">Delete</span><span style="color:#D4D4D4;">(</span><span style="color:#9CDCFE;">DenyAll</span><span style="color:#D4D4D4;">)]</span></span>
<span class="line"><span style="color:#569CD6;">public</span><span style="color:#569CD6;"> class</span><span style="color:#4EC9B0;"> ProductConfiguration</span><span style="color:#D4D4D4;"> </span></span>
<span class="line"><span style="color:#D4D4D4;">{</span></span>
<span class="line"><span style="color:#569CD6;">    public</span><span style="color:#569CD6;"> int</span><span style="color:#9CDCFE;"> Id</span><span style="color:#D4D4D4;"> { </span><span style="color:#569CD6;">get</span><span style="color:#D4D4D4;">; </span><span style="color:#569CD6;">set</span><span style="color:#D4D4D4;">; }</span></span>
<span class="line"></span>
<span class="line"><span style="color:#569CD6;">    public</span><span style="color:#569CD6;"> string</span><span style="color:#9CDCFE;"> Sku</span><span style="color:#D4D4D4;"> { </span><span style="color:#569CD6;">get</span><span style="color:#D4D4D4;">; </span><span style="color:#569CD6;">set</span><span style="color:#D4D4D4;">; }</span></span>
<span class="line"><span style="color:#569CD6;">    public</span><span style="color:#569CD6;"> string</span><span style="color:#9CDCFE;"> MarketingDescription</span><span style="color:#D4D4D4;"> { </span><span style="color:#569CD6;">get</span><span style="color:#D4D4D4;">; </span><span style="color:#569CD6;">set</span><span style="color:#D4D4D4;">; }</span></span>
<span class="line"><span style="color:#569CD6;">    public</span><span style="color:#569CD6;"> string</span><span style="color:#9CDCFE;"> InternalNotes</span><span style="color:#D4D4D4;"> { </span><span style="color:#569CD6;">get</span><span style="color:#D4D4D4;">; </span><span style="color:#569CD6;">set</span><span style="color:#D4D4D4;">; }</span></span>
<span class="line"></span>
<span class="line"><span style="color:#569CD6;">    public</span><span style="color:#569CD6;"> int</span><span style="color:#D4D4D4;">? </span><span style="color:#9CDCFE;">CurrentVersionId</span><span style="color:#D4D4D4;"> { </span><span style="color:#569CD6;">get</span><span style="color:#D4D4D4;">; </span><span style="color:#569CD6;">set</span><span style="color:#D4D4D4;">; }</span></span>
<span class="line"><span style="color:#569CD6;">    public</span><span style="color:#4EC9B0;"> ProductConfigurationVersion</span><span style="color:#9CDCFE;"> CurrentVersion</span><span style="color:#D4D4D4;"> { </span><span style="color:#569CD6;">get</span><span style="color:#D4D4D4;">; </span><span style="color:#569CD6;">set</span><span style="color:#D4D4D4;">; }</span></span>
<span class="line"></span>
<span class="line"><span style="color:#569CD6;">    public</span><span style="color:#4EC9B0;"> DateTimeOffset</span><span style="color:#D4D4D4;">? </span><span style="color:#9CDCFE;">DeletedDate</span><span style="color:#D4D4D4;"> { </span><span style="color:#569CD6;">get</span><span style="color:#D4D4D4;">; </span><span style="color:#569CD6;">set</span><span style="color:#D4D4D4;">; }</span></span>
<span class="line"><span style="color:#D4D4D4;">}</span></span>
<span class="line"></span>
<span class="line"><span style="color:#D4D4D4;">[</span><span style="color:#4EC9B0;">Edit</span><span style="color:#D4D4D4;">(</span><span style="color:#9CDCFE;">DenyAll</span><span style="color:#D4D4D4;">)]</span></span>
<span class="line"><span style="color:#D4D4D4;">[</span><span style="color:#4EC9B0;">Delete</span><span style="color:#D4D4D4;">(</span><span style="color:#9CDCFE;">DenyAll</span><span style="color:#D4D4D4;">)]</span></span>
<span class="line"><span style="color:#569CD6;">public</span><span style="color:#569CD6;"> class</span><span style="color:#4EC9B0;"> ProductConfigurationVersion</span></span>
<span class="line"><span style="color:#D4D4D4;">{</span></span>
<span class="line"><span style="color:#569CD6;">    public</span><span style="color:#569CD6;"> int</span><span style="color:#9CDCFE;"> Id</span><span style="color:#D4D4D4;"> { </span><span style="color:#569CD6;">get</span><span style="color:#D4D4D4;">; </span><span style="color:#569CD6;">set</span><span style="color:#D4D4D4;">; }</span></span>
<span class="line"></span>
<span class="line"><span style="color:#569CD6;">    public</span><span style="color:#569CD6;"> int</span><span style="color:#9CDCFE;"> ConfigurationId</span><span style="color:#D4D4D4;"> { </span><span style="color:#569CD6;">get</span><span style="color:#D4D4D4;">; </span><span style="color:#569CD6;">set</span><span style="color:#D4D4D4;">; }</span></span>
<span class="line"><span style="color:#569CD6;">    public</span><span style="color:#4EC9B0;"> ProductConfiguration</span><span style="color:#9CDCFE;"> Configuration</span><span style="color:#D4D4D4;"> { </span><span style="color:#569CD6;">get</span><span style="color:#D4D4D4;">; </span><span style="color:#569CD6;">set</span><span style="color:#D4D4D4;">; }</span></span>
<span class="line"></span>
<span class="line"><span style="color:#569CD6;">    public</span><span style="color:#569CD6;"> string</span><span style="color:#9CDCFE;"> Name</span><span style="color:#D4D4D4;"> { </span><span style="color:#569CD6;">get</span><span style="color:#D4D4D4;">; </span><span style="color:#569CD6;">set</span><span style="color:#D4D4D4;">; }</span></span>
<span class="line"><span style="color:#569CD6;">    public</span><span style="color:#569CD6;"> decimal</span><span style="color:#9CDCFE;"> Price</span><span style="color:#D4D4D4;"> { </span><span style="color:#569CD6;">get</span><span style="color:#D4D4D4;">; </span><span style="color:#569CD6;">set</span><span style="color:#D4D4D4;">; }</span></span>
<span class="line"></span>
<span class="line"><span style="color:#569CD6;">    public</span><span style="color:#4EC9B0;"> DateTimeOffset</span><span style="color:#9CDCFE;"> CreatedOn</span><span style="color:#D4D4D4;"> { </span><span style="color:#569CD6;">get</span><span style="color:#D4D4D4;">; </span><span style="color:#569CD6;">set</span><span style="color:#D4D4D4;">; }</span></span>
<span class="line"><span style="color:#D4D4D4;">}</span></span></code></pre></div><p>In this example, other configuration records that need to reference the product can reference the <code>ProductConfiguration</code> record and not need to worry about performing updates to foreign keys every time a new version of the product configuration is created.</p><p>Transactional records, on the other hand, should have foreign keys that reference the <code>ProductConfigurationVersion</code> record so that the exact active version at the time of purchase is known. The principal configuration record can be reached through the <code>Configuration</code> navigation property.</p>`,49),l=[o];function i(r,c,p,d,u,h){return s(),a("div",null,l)}const m=e(n,[["render",i]]);export{y as __pageData,m as default};
