---
# https://vitepress.dev/reference/default-theme-home-page
layout: home

hero:
  name: "Coalesce"
  text: The full-stack meta-framework for ASP.NET Core and Vue.
  tagline: Build what matters. Generate the rest.
  image:
    src: /coalesce-icon-color.svg
    alt: Coalesce
  actions:
    - theme: brand
      text: Introduction
      link: /introduction
    - theme: alt
      text: Get Started
      link: /stacks/vue/getting-started
---

<div class="landing-sections">

<section class="value-props">

## Why Coalesce?

<div class="props-grid">
<div class="prop-item prop-item-detailed">
<div class="prop-stat">Skip the Plumbing</div>
<p class="prop-description">You build Entity Framework data models, domain services, and rich user interfaces. Coalesce generates API controllers, DTOs, TypeScript models, API clients, and more. Focus on delivering value - we'll handle the trivia.</p>
</div>
<div class="prop-item prop-item-detailed">
<div class="prop-stat">Effortless UX</div>
<p class="prop-description">Generated ViewModels include auto-save, bulk saves, dirty tracking, loading states, and validation — all flowing from your C# model. Ready-made Vue components give you data-driven dropdowns, date pickers, and more, ready to use in your UIs.</p>
</div>
<div class="prop-item prop-item-detailed">
<div class="prop-stat">Security Included</div>
<p class="prop-description">Apply role-based, row-level, and property-level security with simple declarative attributes or completely custom code — whichever fits the scenario. Security trimming is enforced automatically on every API response.</p>
</div>
</div>
</section>


<section class="how-it-works">

## The Coalesce Workflow

<div class="steps">
<div class="step">
<div class="step-number">1</div>
<div class="step-content">

### Define Your Models

Write C# classes with Entity Framework. Use attributes to configure behavior, security, and validation. Write service classes for anything that doesn't fit into simple CRUD.

```cs
[Create(Roles = "HR")]
[Edit(Roles = "HR,Admin")]
[Delete(DenyAll)]
public class Employee
{
    public int EmployeeId { get; set; }

    [Search]
    public required string Name { get; set; }

    [Search, EmailAddress]
    public string Email { get; set; }

    public DateOnly HireDate { get; init; }
}
```

</div>
</div>

<div class="step">
<div class="step-number">2</div>
<div class="step-content">

### Run Code Generation

A single `dotnet coalesce` command generates everything between your domain models and your UI.

```
dotnet coalesce
```

</div>
</div>

<div class="step">
<div class="step-number">3</div>
<div class="step-content">

### Build Your Experience

Use the generated APIs and TypeScript with the provided Vue components to build custom pages with full type-safety. Or, start using the full-featured admin pages right away to view and edit data.

```vue
<template>
  <c-loader-status :loaders="[employee.$load, employee.$save]">
    <c-input :model="employee" for="name" />
    <c-input :model="employee" for="email" />
    <c-input :model="employee" for="hireDate" />
    <v-btn @click="employee.$save()">Save</v-btn>
  </c-loader-status>
</template>
<script setup lang="ts">
  import { EmployeeViewModel } from "@/viewmodels.g";
  const employee = new EmployeeViewModel();
  employee.$load(1);
</script>
```

</div>
</div>
</div>
</section>


<section class="what-gets-generated">

## One Model, Full Stack

<p class="section-blurb">From a single C# class, Coalesce generates a complete vertical application slice:</p>

<div class="gen-fanout">
<div class="gen-input">
<div class="gen-input-label">You write:</div>

```cs
[Edit(Roles = "HR,Admin")]
[Delete(DenyAll)]
public class Employee
{
    public int EmployeeId { get; set; }

    [Search, Required]
    public string Name { get; set; }

    [Search, EmailAddress]
    public string Email { get; set; }

    public DateOnly HireDate { get; init; }

    [Coalesce]
    public async Task SendWelcomeEmail(
        [Inject] IEmailService emailService)
    {
        await emailService.Send(Email, "Welcome!");
    }
}
```

</div>
<div class="gen-arrow">→</div>
<div class="gen-output">
<div class="gen-output-label">Coalesce provides:</div>
<div class="gen-file-tree">
<div class="gen-group">
<div class="gen-group-label">C# — Server</div>
<div class="gen-file"><span class="gen-file-icon">⚙️</span> EmployeeController.g.cs <span class="gen-file-desc">Full CRUD + custom methods API</span></div>
<div class="gen-file"><span class="gen-file-icon">📦</span> EmployeeDto.g.cs <span class="gen-file-desc">Request/response DTOs with security trimming</span></div>
</div>
<div class="gen-group">
<div class="gen-group-label">TypeScript — Client</div>
<div class="gen-file"><span class="gen-file-icon">📋</span> metadata.g.ts <span class="gen-file-desc">Type metadata, validation rules, security info</span></div>
<div class="gen-file"><span class="gen-file-icon">🔷</span> models.g.ts <span class="gen-file-desc">Interfaces and enums</span></div>
<div class="gen-file"><span class="gen-file-icon">🧩</span> api-clients.g.ts <span class="gen-file-desc">Typed HTTP clients for every endpoint</span></div>
<div class="gen-file"><span class="gen-file-icon">⚡</span> viewmodels.g.ts <span class="gen-file-desc">Reactive ViewModels with auto-save, loading states, validation</span></div>
</div>
<div class="gen-group">
<div class="gen-group-label">Vue — UI</div>
<div class="gen-file"><span class="gen-file-icon">📄</span> Admin list &amp; editor pages <span class="gen-file-desc">Full CRUD UI out of the box</span></div>
<div class="gen-file"><span class="gen-file-icon">🧩</span> Input components <span class="gen-file-desc">Dropdowns, date pickers, selects — model-driven</span></div>
<div class="gen-file"><span class="gen-file-icon">🧩</span> Display components <span class="gen-file-desc">Tables, detail views, lists</span></div>
</div>
</div>
</div>
</div>

</section>


<section class="comparison">

## Coalesce vs. Alternatives

<p class="section-blurb">Unlike low-code platforms, you're never boxed or locked in. Unlike one-time scaffolding, Coalesce regenerates as your models evolve. </p>

<table class="comparison-table">
<thead>
<tr>
<th></th>
<th><strong>Coalesce</strong></th>
<th><strong>Low-Code Platforms</strong><br><span class="th-example">Power Apps, OutSystems</span></th>
<th><strong>API Client Generators</strong><br><span class="th-example">NSwag, OpenAPI codegen</span></th>
<th><strong>Build from Scratch</strong><br><span class="th-example">Controllers, AutoMapper</span></th>
</tr>
</thead>
<tbody>
<tr>
<td><strong>Time to working CRUD</strong></td>
<td>✅ Minutes</td>
<td>✅ Minutes</td>
<td>❌ Backend not included</td>
<td>❌ Days to weeks</td>
</tr>
<tr>
<td><strong>Adaptability to change</strong></td>
<td>✅ Re-run codegen</td>
<td>⚠️ Varies by vendor</td>
<td>✅ Re-run client gen</td>
<td>❌ Manual updates everywhere</td>
</tr>
<tr>
<td><strong>Admin UI included</strong></td>
<td>✅ Full-featured</td>
<td>✅ Included</td>
<td>❌ Not included</td>
<td>❌ Not included</td>
</tr>
<tr>
<td><strong>Code Generation</strong></td>
<td>✅ Server &amp; Client</td>
<td>❌ No source code</td>
<td>⚠️ Client only</td>
<td>❌ Neither</td>
</tr>
<tr>
<td><strong>Fully customizable</strong></td>
<td>✅ Override anything</td>
<td>⚠️ Limited</td>
<td>✅ Full control</td>
<td>✅ Full control</td>
</tr>
<tr>
<td><strong>Security model built-in</strong></td>
<td>✅ Row & property level</td>
<td>⚠️ Vendor-specific</td>
<td>❌ Not included</td>
<td>❌ Build from scratch</td>
</tr>
<tr>
<td><strong>Deployment</strong></td>
<td>✅ Any host</td>
<td>❌ Vendor lock-in</td>
<td>✅ Any host</td>
<td>✅ Any host</td>
</tr>
</tbody>
</table>

</section>


<section class="use-cases">

## The Right Fit

<p class="section-blurb">Any application where CRUD is the foundation and you'd rather spend time on business logic and user experience than wiring up APIs and forms is a perfect fit for Coalesce.</p>

<div class="use-case-list">
<span class="use-case">Internal Tools</span>
<span class="use-case">Admin Panels</span>
<span class="use-case">Line-of-Business Apps</span>
<span class="use-case">MVPs & Prototypes</span>
<span class="use-case">Data-Driven Applications</span>
<span class="use-case">CRUD-Heavy Backends</span>
</div>


</section>


<section class="cta">

## Ready to Build Faster?

<p class="section-blurb">Set up a new project in under five minutes. The getting started guide walks you through creating your first model, running codegen, and seeing the admin UI — no configuration required.</p>

<div class="cta-buttons">
<a class="cta-btn cta-btn-alt" href="/introduction.html">Read the Introduction</a>
<a class="cta-btn cta-btn-brand" href="/stacks/vue/getting-started.html">Get Started</a>
</div>

</section>

</div>

<SiteFooter />

<style>
.VPHero .text {
  font-size: 42px;
  line-height: 1.1;
  padding: 16px 0;
  color: var(--logo-text-color);
}

.VPHero .VPImage {
  width: 100%
}

.landing-sections {
  max-width: 1152px;
  margin: 0 auto;
  /* padding: 0 24px; */
}

.landing-sections .header-anchor {
  display: none;
}

.landing-sections section {
  padding: 64px 0;
  border-top: 1px solid var(--vp-c-divider);
}

.landing-sections h2 {
  font-size: 32px;
  font-weight: 700;
  text-align: center;
  margin-top: 0px;
  margin-bottom: 36px;
  letter-spacing: -0.02em;
  border: none;
  padding: 0;
}

/* How It Works */
.steps {
  display: flex;
  flex-direction: column;
  gap: 32px;
}

.step {
  display: flex;
  gap: 24px;
  align-items: flex-start;
}

.step-number {
  flex-shrink: 0;
  width: 48px;
  height: 48px;
  border-radius: 50%;
  background: var(--vp-c-brand-3);
  color: white;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 20px;
  font-weight: 700;
}

.step-content {
  flex: 1;
  min-width: 0;
}

.step-content h3 {
  margin: 0 0 8px 0;
  border: none;
  padding: 0;
  font-size: 20px;
}

.step-content p {
  margin: 0 0 12px 0;
  color: var(--vp-c-text-2);
}

/* What Gets Generated */
.gen-fanout {
  display: grid;
  grid-template-columns: auto auto 1fr;
  gap: 24px;
  align-items: center;
}

.gen-input {
  max-width: 480px;
}

.gen-input-label,
.gen-output-label {
  font-size: 13px;
  font-weight: 600;
  text-transform: uppercase;
  letter-spacing: 0.05em;
  color: var(--vp-c-text-3);
  margin-bottom: 12px;
}

.gen-input .vp-code-group,
.gen-input div[class*="language-"] {
  margin: 0 !important;
}

.gen-arrow {
  font-size: 36px;
  color: var(--vp-c-brand-1);
  font-weight: 700;
}

.gen-output {
  min-width: 0;
}

.gen-file-tree {
  display: flex;
  flex-direction: column;
  gap: 16px;
}

.gen-group {
  padding: 16px;
  border-radius: 10px;
  background: var(--vp-c-bg-soft);
  border: 1px solid var(--vp-c-divider);
}

.gen-group-label {
  font-size: 12px;
  font-weight: 700;
  text-transform: uppercase;
  letter-spacing: 0.05em;
  color: var(--vp-c-brand-1);
  margin-bottom: 10px;
}

.gen-file {
  font-size: 14px;
  padding: 4px 0;
  font-family: var(--vp-font-family-mono);
  color: var(--vp-c-text-1);
}

.gen-file-icon {
  margin-right: 6px;
}

.gen-file-desc {
  font-family: var(--vp-font-family-base);
  font-size: 13px;
  color: var(--vp-c-text-3);
  margin-left: 8px;
}

@media (max-width: 900px) {
  .gen-fanout {
    grid-template-columns: 1fr;
  }
  .gen-arrow {
    transform: rotate(90deg);
    justify-self: center;
  }
  .gen-input {
    max-width: 100%;
  }
}

/* Value Props */
.props-grid {
  display: grid;
  grid-template-columns: repeat(3, 1fr);
  gap: 48px 32px;
  text-align: center;
}

.prop-stat {
  font-size: 28px;
  font-weight: 700;
  color: var(--vp-c-brand-1);
  margin-bottom: 8px;
}

.prop-item-detailed .prop-stat {
  font-size: 28px;
  margin-bottom: 14px;
  text-align: center;
}

.prop-description {
  font-size: 15px;
  text-align: center !important;
  line-height: 1.7;
  color: var(--vp-c-text-2);
  margin: 0;
  text-align: left;
}

/* Comparison */
.comparison-table {
  display: table !important;
  margin: 0 auto !important;
  font-size: 15px;
  border-collapse: separate;
  border-spacing: 0;
  border: 1px solid var(--vp-c-divider);
  border-radius: 12px;
  overflow: hidden;
}

.comparison-table th,
.comparison-table td {
  padding: 14px 16px;
  text-align: center;
  vertical-align: middle;
  border-bottom: 1px solid var(--vp-c-divider);
}

.comparison-table th {
  font-weight: 600;
  background: var(--vp-c-bg-soft);
}

.comparison-table th:first-child,
.comparison-table td:first-child {
  text-align: left;
  font-weight: 500;
}

.comparison-table tr:last-child td {
  border-bottom: none;
}

.comparison-table th:nth-child(2),
.comparison-table td:nth-child(2) {
  background: var(--vp-c-brand-soft);
}

.th-example {
  font-weight: 400;
  font-size: 13px;
  color: var(--vp-c-text-3);
}

/* Use Cases */
.use-case-list {
  display: flex;
  flex-wrap: wrap;
  gap: 12px;
  justify-content: center;
  margin-bottom: 24px;
}

.use-case {
  padding: 8px 20px;
  border-radius: 20px;
  background: var(--vp-c-brand-soft);
  color: var(--vp-c-brand-1);
  font-weight: 500;
  font-size: 15px;
}

.section-blurb {
  text-align: center;
  color: var(--vp-c-text-2);
  max-width: 610px;
  margin: 0 auto 32px !important;
}

/* CTA */
.cta {
  text-align: center;
}

.cta-buttons {
  display: flex;
  gap: 16px;
  justify-content: center;
  flex-wrap: wrap;
}

.cta-btn {
  display: inline-block;
  padding: 14px 36px;
  border-radius: 24px;
  font-weight: 600;
  font-size: 16px;
  text-decoration: none;
  transition: filter 0.2s;
}

.cta-btn:hover {
  filter: brightness(1.1);
}

.cta-btn-brand {
  background: var(--vp-c-brand-2);
  color: #fff !important;
}

.cta-btn-brand:hover {
  background: var(--vp-c-brand-1);
}

.cta-btn-alt {
  background: var(--vp-c-default-soft);
  color: var(--vp-c-text-1);
  border: 1px solid var(--vp-c-divider);
}

.cta-btn-alt:hover {
  border-color: var(--vp-c-text-2);
}

@media (max-width: 640px) {
  .step {
    flex-direction: column;
    align-items: center;
    text-align: center;
  }
  .gen-grid {
    grid-template-columns: 1fr;
  }
  .props-grid {
    grid-template-columns: 1fr;
  }
}
</style>