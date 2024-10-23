import{_ as p,c as r,I as o,w as a,a as e,l as s,a7 as l,D as n,o as i}from"./chunks/framework.BkavzUpE.js";const I=JSON.parse('{"title":"Includes String","description":"","frontmatter":{},"headers":[],"relativePath":"concepts/includes.md","filePath":"concepts/includes.md"}'),d={name:"concepts/includes.md"},D=s("h1",{id:"includes-string",tabindex:"-1"},[e("Includes String "),s("a",{class:"header-anchor",href:"#includes-string","aria-label":'Permalink to "Includes String"'},"​")],-1),u=s("p",null,'Coalesce provides a number of extension points for loading & serialization which make use of a concept called an "includes string" (also referred to as "include string" or just "includes").',-1),y=s("h2",{id:"includes-string-1",tabindex:"-1"},[e("Includes String "),s("a",{class:"header-anchor",href:"#includes-string-1","aria-label":'Permalink to "Includes String"'},"​")],-1),h=s("p",null,"The includes string is simply a string which can be set to any arbitrary value. It is passed from the client to the server in order to customize data loading and serialization. It can be set on both the TypeScript ViewModels and the ListViewModels.",-1),C=s("div",{class:"language-ts"},[s("button",{title:"Copy Code",class:"copy"}),s("span",{class:"lang"},"ts"),s("pre",{class:"shiki dark-plus vp-code"},[s("code",null,[s("span",{class:"line"},[s("span",{style:{color:"#C586C0"}},"import"),s("span",{style:{color:"#D4D4D4"}}," { "),s("span",{style:{color:"#9CDCFE"}},"PersonViewModel"),s("span",{style:{color:"#D4D4D4"}},", "),s("span",{style:{color:"#9CDCFE"}},"PersonListViewModel"),s("span",{style:{color:"#D4D4D4"}}," } "),s("span",{style:{color:"#C586C0"}},"from"),s("span",{style:{color:"#CE9178"}}," '@/viewmodels.g'")]),e(`
`),s("span",{class:"line"}),e(`
`),s("span",{class:"line"},[s("span",{style:{color:"#569CD6"}},"var"),s("span",{style:{color:"#9CDCFE"}}," person"),s("span",{style:{color:"#D4D4D4"}}," = "),s("span",{style:{color:"#569CD6"}},"new"),s("span",{style:{color:"#DCDCAA"}}," PersonViewModel"),s("span",{style:{color:"#D4D4D4"}},"();")]),e(`
`),s("span",{class:"line"},[s("span",{style:{color:"#9CDCFE"}},"person"),s("span",{style:{color:"#D4D4D4"}},"."),s("span",{style:{color:"#9CDCFE"}},"$includes"),s("span",{style:{color:"#D4D4D4"}}," = "),s("span",{style:{color:"#CE9178"}},'"details"'),s("span",{style:{color:"#D4D4D4"}},";")]),e(`
`),s("span",{class:"line"}),e(`
`),s("span",{class:"line"},[s("span",{style:{color:"#569CD6"}},"var"),s("span",{style:{color:"#9CDCFE"}}," personList"),s("span",{style:{color:"#D4D4D4"}}," = "),s("span",{style:{color:"#569CD6"}},"new"),s("span",{style:{color:"#DCDCAA"}}," PersonListViewModel"),s("span",{style:{color:"#D4D4D4"}},"();")]),e(`
`),s("span",{class:"line"},[s("span",{style:{color:"#9CDCFE"}},"personList"),s("span",{style:{color:"#D4D4D4"}},"."),s("span",{style:{color:"#9CDCFE"}},"$includes"),s("span",{style:{color:"#D4D4D4"}}," = "),s("span",{style:{color:"#CE9178"}},'"details"'),s("span",{style:{color:"#D4D4D4"}},";")])])])],-1),m=l("",15),b=s("div",{class:"language-ts"},[s("button",{title:"Copy Code",class:"copy"}),s("span",{class:"lang"},"ts"),s("pre",{class:"shiki dark-plus vp-code"},[s("code",null,[s("span",{class:"line"},[s("span",{style:{color:"#C586C0"}},"import"),s("span",{style:{color:"#D4D4D4"}}," { "),s("span",{style:{color:"#9CDCFE"}},"PersonListViewModel"),s("span",{style:{color:"#D4D4D4"}}," } "),s("span",{style:{color:"#C586C0"}},"from"),s("span",{style:{color:"#CE9178"}}," '@/viewmodels.g'")]),e(`
`),s("span",{class:"line"}),e(`
`),s("span",{class:"line"},[s("span",{style:{color:"#569CD6"}},"const"),s("span",{style:{color:"#4FC1FF"}}," personList"),s("span",{style:{color:"#D4D4D4"}}," = "),s("span",{style:{color:"#569CD6"}},"new"),s("span",{style:{color:"#DCDCAA"}}," PersonListViewModel"),s("span",{style:{color:"#D4D4D4"}},"();")]),e(`
`),s("span",{class:"line"},[s("span",{style:{color:"#9CDCFE"}},"personList"),s("span",{style:{color:"#D4D4D4"}},"."),s("span",{style:{color:"#9CDCFE"}},"$includes"),s("span",{style:{color:"#D4D4D4"}}," = "),s("span",{style:{color:"#CE9178"}},'"Editor"'),s("span",{style:{color:"#D4D4D4"}},";")]),e(`
`),s("span",{class:"line"},[s("span",{style:{color:"#C586C0"}},"await"),s("span",{style:{color:"#9CDCFE"}}," personList"),s("span",{style:{color:"#D4D4D4"}},"."),s("span",{style:{color:"#DCDCAA"}},"$load"),s("span",{style:{color:"#D4D4D4"}},"();")]),e(`
`),s("span",{class:"line"},[s("span",{style:{color:"#6A9955"}},"// Objects in personList.$items will not contain CreatedBy nor Department objects.")]),e(`
`),s("span",{class:"line"}),e(`
`),s("span",{class:"line"},[s("span",{style:{color:"#569CD6"}},"const"),s("span",{style:{color:"#4FC1FF"}}," personList2"),s("span",{style:{color:"#D4D4D4"}}," = "),s("span",{style:{color:"#569CD6"}},"new"),s("span",{style:{color:"#DCDCAA"}}," PersonListViewModel"),s("span",{style:{color:"#D4D4D4"}},"();")]),e(`
`),s("span",{class:"line"},[s("span",{style:{color:"#9CDCFE"}},"personList2"),s("span",{style:{color:"#D4D4D4"}},"."),s("span",{style:{color:"#9CDCFE"}},"$includes"),s("span",{style:{color:"#D4D4D4"}}," = "),s("span",{style:{color:"#CE9178"}},'"details"'),s("span",{style:{color:"#D4D4D4"}},";")]),e(`
`),s("span",{class:"line"},[s("span",{style:{color:"#C586C0"}},"await"),s("span",{style:{color:"#9CDCFE"}}," personList"),s("span",{style:{color:"#D4D4D4"}},"."),s("span",{style:{color:"#DCDCAA"}},"$load"),s("span",{style:{color:"#D4D4D4"}},"();")]),e(`
`),s("span",{class:"line"},[s("span",{style:{color:"#6A9955"}},"// Objects in personList2.items will be allowed to contain both CreatedBy and Department objects. ")]),e(`
`),s("span",{class:"line"},[s("span",{style:{color:"#6A9955"}},"// Department will be allowed to include its other Person objects.")])])])],-1),g=s("h3",{id:"properties",tabindex:"-1"},[e("Properties "),s("a",{class:"header-anchor",href:"#properties","aria-label":'Permalink to "Properties"'},"​")],-1),w=l("",3);function f(_,E,v,P,A,F){const t=n("CodeTabs"),c=n("Prop");return i(),r("div",null,[D,u,y,h,o(t,null,{vue:a(()=>[C]),_:1}),m,o(t,null,{vue:a(()=>[b]),_:1}),g,o(c,{def:"public string ContentViews { get; set; }",ctor:"1"}),e(),w])}const T=p(d,[["render",f]]);export{I as __pageData,T as default};
