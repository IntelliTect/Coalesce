import{_ as n,D as o,o as t,c as l,I as a,R as p,k as e}from"./chunks/framework.g9eZ-ZSs.js";const g=JSON.parse('{"title":"[ManyToMany]","description":"","frontmatter":{},"headers":[],"relativePath":"modeling/model-components/attributes/many-to-many.md","filePath":"modeling/model-components/attributes/many-to-many.md"}'),r={name:"modeling/model-components/attributes/many-to-many.md"},c=p("",6),i=e("p",null,"The name of the collection that will contain the set of objects on the other side of the many-to-many relationship.",-1),y=e("p",null,"The name of the navigation property on the middle entity that points at the far side of the many-to-many relationship. Use this to resolve ambiguities when the middle table of the many-to-many relationship has more than two reference navigation properties on it.",-1);function D(m,d,h,u,C,f){const s=o("Prop");return t(),l("div",null,[c,a(s,{def:"public string CollectionName { get; }",ctor:"1"}),i,a(s,{def:"public string FarNavigationProperty { get; set; }"}),y])}const b=n(r,[["render",D]]);export{g as __pageData,b as default};