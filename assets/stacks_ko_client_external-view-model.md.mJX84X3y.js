import{_ as a,D as l,o as n,c as s,I as r,R as i,k as e,a as o}from"./chunks/framework.g9eZ-ZSs.js";const T=JSON.parse('{"title":"TypeScript External ViewModels","description":"","frontmatter":{},"headers":[],"relativePath":"stacks/ko/client/external-view-model.md","filePath":"stacks/ko/client/external-view-model.md"}'),c={name:"stacks/ko/client/external-view-model.md"},p=i('<h1 id="typescript-external-viewmodels" tabindex="-1">TypeScript External ViewModels <a class="header-anchor" href="#typescript-external-viewmodels" aria-label="Permalink to &quot;TypeScript External ViewModels&quot;">​</a></h1><p>For all <a href="/Coalesce/modeling/model-types/external-types.html">External Types</a> in your model, Coalesce will generate a TypeScript class that provides a bare-bones representation of that type&#39;s properties.</p><p>These ViewModels are dependent on <a href="https://knockoutjs.com/" target="_blank" rel="noreferrer">Knockout</a>, and are designed to be used directly from Knockout bindings in your HTML. All data properties on the generated model are Knockout observables.</p><h2 id="base-members" tabindex="-1">Base Members <a class="header-anchor" href="#base-members" aria-label="Permalink to &quot;Base Members&quot;">​</a></h2><p>The TypeScript ViewModels for external types do not have a common base class, and do not have any of the behaviors or convenience properties that the regular <a href="/Coalesce/stacks/ko/client/view-model.html">TypeScript ViewModels</a> for database-mapped classes have.</p><h2 id="model-specific-members" tabindex="-1">Model-Specific Members <a class="header-anchor" href="#model-specific-members" aria-label="Permalink to &quot;Model-Specific Members&quot;">​</a></h2><h3 id="data-properties" tabindex="-1">Data Properties <a class="header-anchor" href="#data-properties" aria-label="Permalink to &quot;Data Properties&quot;">​</a></h3>',7),d=e("p",null,[o("For each exposed property on the underlying EF POCO, a "),e("code",null,"KnockoutObservable<T>"),o(" property will exist on the TypeScript model. For navigation properties, these will be typed with the corresponding TypeScript ViewModel for the other end of the relationship. For collections (including collection navigation properties), these properties will be "),e("code",null,"KnockoutObservableArray<T>"),o(" objects.")],-1),b=e("h3",{id:"enum-members",tabindex:"-1"},[o("Enum Members "),e("a",{class:"header-anchor",href:"#enum-members","aria-label":'Permalink to "Enum Members"'},"​")],-1),m=e("p",null,[o("For each "),e("code",null,"enum"),o(" property on your POCO, the following will be created:")],-1),u=e("p",null,[o("A "),e("code",null,"KnockoutComputed<string>"),o(" property that will provide the text to display for that property.")],-1);function h(k,_,v,y,f,x){const t=l("Prop");return n(),s("div",null,[p,r(t,{def:`
public personId: KnockoutObservable<number | null> = ko.observable(null);
public fullName: KnockoutObservable<string | null> = ko.observable(null);
public gender: KnockoutObservable<number | null> = ko.observable(null);
public companyId: KnockoutObservable<number | null> = ko.observable(null);
public company: KnockoutObservable<ViewModels.Company | null> = ko.observable(null);
public addresses: KnockoutObservableArray<ViewModels.Address> = ko.observableArray([]);
public birthDate: KnockoutObservable<moment.Moment | null> = ko.observable(moment());`,lang:"ts",id:"data-property-members"}),d,b,m,r(t,{def:"public genderText: KnockoutComputed<string | null>",lang:"ts"}),u])}const g=a(c,[["render",h]]);export{T as __pageData,g as default};