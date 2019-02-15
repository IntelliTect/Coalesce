using IntelliTect.Coalesce.CodeGeneration.Generation;
using IntelliTect.Coalesce.TypeDefinition;
using System;
using System.Collections.Generic;
using System.Text;
using IntelliTect.Coalesce.CodeGeneration.Knockout.BaseGenerators;
using IntelliTect.Coalesce.Utilities;
using IntelliTect.Coalesce.Knockout.TypeDefinition;
using System.Linq;
using IntelliTect.Coalesce.TypeDefinition.Enums;

namespace IntelliTect.Coalesce.CodeGeneration.Knockout.Generators
{
    public class KoViewModel : KnockoutViewModelGenerator
    {
        public KoViewModel(GeneratorServices services) : base(services) { }

        public override void BuildOutput(TypeScriptCodeBuilder b)
        {
            using (b.Block($"module {ViewModelModuleName}"))
            {
                b.Line();
                WriteViewModelClass(b);

                b.Line();
                WriteEnumNamespace(b);
            }
        }

        private void WriteViewModelClass(TypeScriptCodeBuilder b)
        {
            using (b.Block($"export class {Model.ViewModelGeneratedClassName} extends Coalesce.BaseViewModel"))
            {
                b.Line($"public readonly modelName = \"{Model.ClientTypeName}\";");
                b.Line($"public readonly primaryKeyName = \"{Model.PrimaryKey.JsVariable}\";");
                b.Line($"public readonly modelDisplayName = \"{Model.DisplayName}\";");
                b.Line($"public readonly apiController = \"/{Model.ApiRouteControllerPart}\";");
                b.Line($"public readonly viewController = \"/{Model.ControllerName}\";");

                b.DocComment($"Configuration for all instances of {Model.ViewModelClassName}. Can be overidden on each instance via instance.coalesceConfig.");
                b.Line($"public static coalesceConfig: Coalesce.ViewModelConfiguration<{Model.ViewModelClassName}>");
                b.Indented($"= new Coalesce.ViewModelConfiguration<{Model.ViewModelClassName}>(Coalesce.GlobalConfiguration.viewModel);");

                b.DocComment($"Configuration for the current {Model.ViewModelClassName} instance.");
                b.Line("public coalesceConfig: Coalesce.ViewModelConfiguration<this>");
                b.Indented($"= new Coalesce.ViewModelConfiguration<{Model.ViewModelGeneratedClassName}>({Model.ViewModelClassName}.coalesceConfig);");

                b.DocComment("The namespace containing all possible values of this.dataSource.");
                b.Line($"public dataSources: typeof ListViewModels.{Model.ClientTypeName}DataSources = ListViewModels.{Model.ClientTypeName}DataSources;");

                b.Line();
                b.Line();
                foreach (PropertyViewModel prop in Model.ClientProperties)
                {
                    b.DocComment(prop.Comment);
                    b.Line($"public {prop.JsVariable}: {prop.Type.TsKnockoutType(true)} = {prop.Type.ObservableConstructorCall()};");
                    if (prop.Type.IsEnum)
                    {
                        b.DocComment($"Text value for enumeration {prop.Name}");
                        b.Line($"public {prop.JsTextPropertyName}: KnockoutComputed<string | null> = ko.pureComputed(() => {{");
                        b.Line($"    for(var i = 0; i < this.{prop.JsVariable}Values.length; i++){{");
                        b.Line($"        if (this.{prop.JsVariable}Values[i].id == this.{prop.JsVariable}()){{");
                        b.Line($"            return this.{prop.JsVariable}Values[i].value;");
                        b.Line("        }");
                        b.Line("    }");
                        b.Line("    return null;");
                        b.Line("});");
                    }
                    if (prop.IsManytoManyCollection)
                    {
                        if (prop.Comment.Length > 0)
                        {
                            b.DocComment($"Collection of related objects for many-to-many relationship {prop.ManyToManyCollectionName} via {prop.Name}");
                        }
                        b.Line($"public {prop.ManyToManyCollectionName.ToCamelCase()}: KnockoutObservableArray<ViewModels.{prop.ManyToManyCollectionProperty.Object.ViewModelClassName}> = ko.observableArray([]);");
                    }
                }

                b.Line();
                foreach (PropertyViewModel prop in Model.ClientProperties.Where(f => f.IsPOCO))
                {
                    b.DocComment($"Display text for {prop.Name}");
                    b.Line($"public {prop.JsTextPropertyName}: KnockoutComputed<string>;");
                }

                b.Line();
                foreach (PropertyViewModel prop in Model.FileProperties)
                {
                    b.DocComment($"File properties for {prop.Name}");
                    b.Line($"public {prop.JsVariableUrl}: KnockoutComputed<string> = ko.pureComputed(() => {{");
                    var fileUrl = $"this.coalesceConfig.baseApiUrl() + this.apiController + '/{prop.FileControllerMethodName}";
                    fileUrl += $"?id=' + this.{prop.Parent.PrimaryKey.JsVariable}()";
                    fileUrl += " + '&' + this.dataSource.getQueryString()";
                    if (!string.IsNullOrWhiteSpace(prop.FileHashProperty)) {
                        fileUrl += $" + '&hash=' + this.{prop.FileHashProperty.ToCamelCase()}()";
                    }
                    b.Line($"    return {fileUrl};");
                    b.Line("});");                
                }

                b.Line();
                foreach (PropertyViewModel prop in Model.ClientProperties.Where(f => f.Role == PropertyRole.CollectionNavigation && !f.IsManytoManyCollection))
                {
                    b.DocComment($"Add object to {prop.JsVariable}");
                    using (b.Block($"public addTo{prop.Name} = (autoSave?: boolean | null): {prop.Object.ViewModelClassName} =>", ';'))
                    {
                        b.Line($"var newItem = new {prop.Object.ViewModelClassName}();");
                        b.Line($"if (typeof(autoSave) == 'boolean'){{");
                        b.Line($"    newItem.coalesceConfig.autoSaveEnabled(autoSave);");
                        b.Line($"}}");
                        b.Line($"newItem.parent = this;");
                        b.Line($"newItem.parentCollection = this.{prop.JsVariable};");
                        b.Line($"newItem.isExpanded(true);");
                        if (prop.InverseIdProperty != null)
                        {
                            b.Line($"newItem.{prop.InverseIdProperty.JsVariable}(this.{Model.PrimaryKey.JsVariable}());");
                        }
                        else if (prop.Object.PropertyByName(Model.PrimaryKey.JsVariable) != null)
                        {
                            // TODO: why do we do this? Causes weird behavior simply because key names line up.
                            // If all primary keys are just named "Id", this will copy the PK of the parent to the PK of the child,
                            // which doesn't make sense.
                            b.Line($"newItem.{Model.PrimaryKey.JsVariable}(this.{Model.PrimaryKey.JsVariable}());");
                        }
                        b.Line($"this.{prop.JsVariable}.push(newItem);");
                        b.Line($"return newItem;");
                    }

                    b.DocComment($"ListViewModel for {prop.Name}. Allows for loading subsets of data.");
                    b.Line($"public {prop.JsVariable}List: (loadImmediate?: boolean) => {ListViewModelModuleName}.{prop.Object.ListViewModelClassName};");
                }

                b.Line();
                foreach (PropertyViewModel prop in Model.ClientProperties.Where(f => f.Role == PropertyRole.CollectionNavigation))
                {
                    b.DocComment($"Url for a table view of all members of collection {prop.Name} for the current object.");
                    b.Line($"public {prop.ListEditorUrlName()}: KnockoutComputed<string> = ko.computed(");
                    if (prop.ListEditorUrl() == null)
                    {
                        b.Indented($"() => \"Inverse property not set on {Model.ClientTypeName} for property {prop.Name}\",");
                    }
                    else
                    {
                        b.Indented($"() => this.coalesceConfig.baseViewUrl() + '/{prop.ListEditorUrl()}' + this.{Model.PrimaryKey.JsVariable}(),");
                    }
                    b.Indented("null, { deferEvaluation: true }");
                    b.Line(");");
                }

                b.Line();
                foreach (PropertyViewModel prop in Model.ClientProperties.Where(f => f.Role == PropertyRole.ReferenceNavigation))
                {
                    b.DocComment($"Pops up a stock editor for object {prop.JsVariable}");
                    b.Line($"public show{prop.Name}Editor: (callback?: any) => void;");
                }

                b.Line();
                foreach (PropertyViewModel prop in Model.ClientProperties.Where(f => f.Type.IsEnum))
                {
                    b.DocComment($"Array of all possible names & values of enum {prop.JsVariable}");
                    b.Line($"public {prop.JsVariable}Values: Coalesce.EnumValue[] = [ ");
                    foreach (var kvp in prop.Type.EnumValues)
                    {
                        b.Indented($"{{ id: {kvp.Key}, value: '{kvp.Value.ToProperCase()}' }},");
                    }
                    b.Line("];");
                }

                b.Line();
                foreach (var prop in Model.ClientProperties.Where(f => f.IsFile))
                {
                    b.DocComment($"Upload method for {prop.Name}");
                    using (b.Block($"public {prop.JsVariable}Upload = (data: any, e: any): void =>"))
                    {
                        b.Line("let file = e.target.files[0];");
                        b.Line("let formData = new FormData();");
                        b.Line("formData.append(\"file\", file);");
                        using (b.Block("$.ajax(")){
                            b.Line($"type: \"POST\",");
                            b.Line($"url: this.apiController +'/{prop.UploadUrl()}',");
                            b.Line($"contentType: false,");
                            b.Line($"processData: false,");
                            b.Line($"data: formData,");
                            b.Line($"success: (result) => {{}},");
                            b.Line($"error: (result) => {{}}");
                        }
                    }
                    b.Line("");
                }



                b.Line();
                foreach (var method in Model.ClientMethods.Where(m => !m.IsStatic || m.ResultType.EqualsType(Model.Type)))
                {
                    WriteClientMethodDeclaration(b, method, Model.ViewModelGeneratedClassName, true, true);
                }

                WriteMethod_LoadFromDto(b);

                WriteMethod_SaveToDto(b);

                b.DocComment(new[]
                {
                    "Loads any child objects that have an ID set, but not the full object.",
                    "This is useful when creating an object that has a parent object and the ID is set on the new child."
                });
                using (b.Block("public loadChildren = (callback?: () => void): void =>", ';'))
                {
                    b.Line("var loadingCount = 0;");
                    // AES 4/14/18 - unsure of the reason for the !IsReadOnly check here. Perhaps just redundant?
                    foreach (PropertyViewModel prop in Model.ClientProperties.Where(f => f.Role == PropertyRole.ReferenceNavigation && !f.IsReadOnly))
                    {
                        b.Line($"// See if this.{prop.JsVariable} needs to be loaded.");
                        using (b.Block($"if (this.{prop.JsVariable}() == null && this.{prop.ForeignKeyProperty.JsVariable}() != null)"))
                        {
                            b.Line( "loadingCount++;");
                            b.Line($"var {prop.JsVariable}Obj = new {prop.Object.ViewModelClassName}();");
                            b.Line($"{prop.JsVariable}Obj.load(this.{prop.ForeignKeyProperty.JsVariable}(), () => {{");
                            b.Indented( "loadingCount--;");
                            b.Indented($"this.{prop.JsVariable}({prop.JsVariable}Obj);");
                            b.Indented( "if (loadingCount == 0 && typeof(callback) == \"function\") { callback(); }");
                            b.Line( "});");
                        }
                    }
                    b.Line( "if (loadingCount == 0 && typeof(callback) == \"function\") { callback(); }");
                }


                b.Line();
                using (b.Block("public setupValidation(): void"))
                {
                    b.Line("if (this.errors !== null) return;");

                    var validatableProps = Model.ClientProperties.Where(p => !string.IsNullOrWhiteSpace(p.ClientValidationKnockoutJs()));

                    b.Line("this.errors = ko.validation.group([");
                    foreach (PropertyViewModel prop in validatableProps.Where(p => !p.ClientValidationAllowSave))
                    {
                        b.Indented($"this.{prop.JsVariable}.extend({{ {prop.ClientValidationKnockoutJs()} }}),");
                    }
                    b.Line("]);");

                    b.Line("this.warnings = ko.validation.group([");
                    foreach (PropertyViewModel prop in validatableProps.Where(p => p.ClientValidationAllowSave))
                    {
                        b.Indented($"this.{prop.JsVariable}.extend({{ {prop.ClientValidationKnockoutJs()} }}),");
                    }
                    b.Line("]);");
                }
                


                b.Line();
                using (b.Block($"constructor(newItem?: object, parent?: Coalesce.BaseViewModel | {ListViewModelModuleName}.{Model.ListViewModelClassName})"))
                {
                    b.Line("super(parent);");
                    b.Line("this.baseInitialize();");
                    b.Line("const self = this;");

                    // Create computeds for display for objects
                    b.Line();
                    foreach (PropertyViewModel prop in Model.ClientProperties.Where(f => f.IsPOCO))
                    {
                        // If the object exists, use the text value. Otherwise show 'None'
                        using (b.Block($"this.{prop.JsTextPropertyName} = ko.pureComputed(function()", ");"))
                        {
                            b.Line($"if (self.{prop.JsVariable}() && self.{prop.JsVariable}()!.{prop.Object.ListTextProperty.JsVariable}()) {{");
                            b.Indented($"return self.{prop.JsVariable}()!.{prop.Object.ListTextProperty.JsVariable}()!.toString();");
                            b.Line("} else {");
                            b.Indented("return \"None\";");
                            b.Line("}");
                        }
                    }

                    b.Line();
                    b.Line();
                    foreach (PropertyViewModel prop in Model.ClientProperties.Where(f => f.Role == PropertyRole.CollectionNavigation && !f.IsManytoManyCollection))
                    {
                        b.Line($"// List Object model for {prop.Name}. Allows for loading subsets of data.");
                        var childListVar = $"_{prop.JsVariable}List";
                        b.Line($"var {childListVar}: {ListViewModelModuleName}.{prop.Object.ListViewModelClassName};");

                        using (b.Block($"this.{prop.JsVariable}List = function(loadImmediate = true)"))
                        {
                            using (b.Block($"if (!{childListVar})"))
                            {
                                b.Line($"{childListVar} = new {ListViewModelModuleName}.{prop.Object.ListViewModelClassName}();");
                                b.Line($"if (loadImmediate) load{prop.Name}List();");
                                b.Line($"self.{prop.Parent.PrimaryKey.JsVariable}.subscribe(load{prop.Name}List)");
                            }
                            b.Line($"return {childListVar};");
                        }

                        using (b.Block($"function load{prop.Name}List()"))
                        {
                            using (b.Block($"if (self.{prop.Parent.PrimaryKey.JsVariable}())"))
                            {
                                if (prop.InverseIdProperty != null)
                                {
                                    b.Line($"{childListVar}.queryString = \"filter.{prop.InverseIdProperty.Name}=\" + self.{prop.Parent.PrimaryKey.JsVariable}();");
                                }
                                else
                                {
                                    b.Line($"{childListVar}.queryString = \"filter.{Model.PrimaryKey.Name}=\" + self.{prop.Parent.PrimaryKey.JsVariable}();");
                                }
                                b.Line($"{childListVar}.load();");
                            }
                        }
                    }

                    b.Line();
                    b.Line();
                    foreach (PropertyViewModel prop in Model.ClientProperties.Where(f => f.Role == PropertyRole.ReferenceNavigation))
                    {
                        using (b.Block($"this.show{prop.Name}Editor = function(callback: any)", ';'))
                        {
                            b.Line($"if (!self.{prop.JsVariable}()) {{");
                            b.Indented($"self.{prop.JsVariable}(new {prop.Object.ViewModelClassName}());");
                            b.Line("}");
                            b.Line($"self.{prop.JsVariable}()!.showEditor(callback)");
                        }
                    }

                    // Register autosave subscriptions on all autosavable properties.
                    // Must be done after everything else in the ctor.
                    b.Line();
                    foreach (PropertyViewModel prop in Model.ClientProperties.Where(p => p.IsClientWritable && !p.Type.IsCollection))
                    {
                        b.Line($"self.{prop.JsVariable}.subscribe(self.autoSave);");
                    }

                    foreach (PropertyViewModel prop in Model.ClientProperties.Where(p => p.IsClientWritable && p.IsManytoManyCollection))
                    {
                        b.Line();
                        b.Line($"self.{prop.ManyToManyCollectionName.ToCamelCase()}.subscribe<KnockoutArrayChange<{prop.ManyToManyCollectionProperty.Object.ViewModelClassName}>[]>(changes => {{");
                        using (b.Indented())
                        {
                            using (b.Block("for (var i in changes)"))
                            {
                                b.Line( "var change = changes[i];");
                                b.Line( "self.autoSaveCollection(");
                                b.Indented( "change.status, ");
                                b.Indented($"this.{prop.JsVariable}, ");
                                b.Indented($"{prop.Object.ViewModelClassName}, ");
                                b.Indented($"'{prop.Object.ClientProperties.First(f => f.Type.EqualsType(Model.Type)).ForeignKeyProperty.JsVariable}',");
                                b.Indented($"'{prop.ManyToManyCollectionProperty.ForeignKeyProperty.JsVariable}',");
                                b.Indented($"change.value.{prop.ManyToManyCollectionProperty.Object.PrimaryKey.JsVariable}()");
                                b.Line( ");");
                            }
                        }
                        b.Line("}, null, \"arrayChange\");");
                    }

                    b.Line();
                    b.Line("if (newItem) {");
                    b.Indented("self.loadFromDto(newItem, true);");
                    b.Line("}");
                }
            }
        }

        private void WriteEnumNamespace(TypeScriptCodeBuilder b)
        {
            using (b.Block($"export namespace {Model.ViewModelGeneratedClassName}"))
            {
                foreach (PropertyViewModel prop in Model.ClientProperties.Where(f => f.Type.IsEnum))
                {
                    using (b.Block($"export enum {prop.Name}Enum"))
                    {
                        foreach (var kvp in prop.Type.EnumValues)
                        {
                            b.Line($"{kvp.Value} = {kvp.Key},");
                        }
                    }
                }
            }
        }

        private void WriteMethod_LoadFromDto(TypeScriptCodeBuilder b)
        {
            b.DocComment(new[]{
                "Load the ViewModel object from the DTO.",
                "@param data: The incoming data object to load.",
                "@param force: Will override the check against isLoading that is done to prevent recursion. False is default.",
                "@param allowCollectionDeletes: Set true when entire collections are loaded. True is the default. ",
                "In some cases only a partial collection is returned, set to false to only add/update collections.",
            });
            using (b.Block("public loadFromDto = (data: any, force: boolean = false, allowCollectionDeletes: boolean = true): void =>", ';'))
            {
                b.Line("if (!data || (!force && this.isLoading())) return;");
                b.Line("this.isLoading(true);");

                b.Line("// Set the ID ");
                b.Line($"this.myId = data.{Model.PrimaryKey.JsonName};");
                b.Line($"this.{Model.PrimaryKey.JsVariable}(data.{Model.PrimaryKey.JsonName});");

                b.Line("// Load the lists of other objects");
                foreach (PropertyViewModel prop in Model.ClientProperties.Where(p => p.Type.IsCollection))
                {
                    using (b.Block($"if (data.{prop.JsonName} != null)"))
                    {
                        if (prop.Object?.PrimaryKey != null)
                        {
                            b.Line("// Merge the incoming array");
                            b.Line($"Coalesce.KnockoutUtilities.RebuildArray(this.{prop.JsVariable}, data.{prop.JsonName}, '{prop.Object.PrimaryKey.JsonName}', {prop.Object.ViewModelClassName}, this, allowCollectionDeletes);");
                            if (prop.IsManytoManyCollection)
                            {
                                b.Line("// Add many-to-many collection");
                                b.Line("let objs: any[] = [];");
                                using (b.Block($"$.each(data.{prop.JsonName}, (index, item) =>", ");"))
                                {
                                    b.Line($"if (item.{prop.ManyToManyCollectionProperty.JsonName}) {{");
                                    b.Indented($"objs.push(item.{prop.ManyToManyCollectionProperty.JsonName});");
                                    b.Line($"}}");
                                }
                                b.Line($"Coalesce.KnockoutUtilities.RebuildArray(this.{prop.ManyToManyCollectionName.ToCamelCase()}, objs, '{prop.ManyToManyCollectionProperty.ForeignKeyProperty.JsVariable}', {prop.ManyToManyCollectionProperty.Object.ViewModelClassName}, this, allowCollectionDeletes);");
                            }
                        }
                        else if (prop.PureType.IsPrimitive)
                        {
                            b.Line($"this.{prop.JsVariable}(data.{prop.JsVariable});");
                        }
                        else
                        {
                            b.Line($"Coalesce.KnockoutUtilities.RebuildArray(this.{prop.JsVariable}, data.{prop.JsonName}, null, {prop.Object.ViewModelClassName}, this, allowCollectionDeletes);");
                        }
                    }
                }

                // Objects are loaded first so that they are available when the IDs get loaded.
                // This handles the issue with populating select lists with correct data because we now have the object.
                foreach (PropertyViewModel prop in Model.ClientProperties.Where(p => p.IsPOCO))
                {
                    b.Line($"if (!data.{prop.JsonName}) {{ ");
                    using (b.Indented())
                    {
                        if (prop.ForeignKeyProperty != null)
                        {
                            // Prop is a reference navigation prop. The incoming foreign key doesn't match our existing foreign key, so clear out the property. 
                            // If the incoming key and existing key DOES match, we assume that the data just wasn't loaded, but is still valid, so we do nothing.
                            b.Line($"if (data.{prop.ForeignKeyProperty.JsonName} != this.{prop.ForeignKeyProperty.JsVariable}()) {{");
                            b.Indented($"this.{prop.JsVariable}(null);");
                            b.Line("}");
                        }
                        else
                        {
                            b.Line($"this.{prop.JsVariable}(null);");
                        }
                    }
                    b.Line("} else {");
                    using (b.Indented())
                    {
                        b.Line($"if (!this.{prop.JsVariable}()){{");
                        b.Indented($"this.{prop.JsVariable}(new {prop.Object.ViewModelClassName}(data.{prop.JsonName}, this));");
                        b.Line("} else {");
                        b.Indented($"this.{prop.JsVariable}()!.loadFromDto(data.{prop.JsonName});");
                        b.Line("}");
                        if (prop.Object.IsDbMappedType)
                        {
                            b.Line($"if (this.parent instanceof {prop.Object.ViewModelClassName} && this.parent !== this.{prop.JsVariable}() && this.parent.{prop.Object.PrimaryKey.JsVariable}() == this.{prop.JsVariable}()!.{prop.Object.PrimaryKey.JsVariable}())");
                            b.Line("{");
                            b.Indented($"this.parent.loadFromDto(data.{prop.JsonName}, undefined, false);");
                            b.Line("}");
                        }
                    }
                    b.Line("}");
                }

                b.Line();
                b.Line("// The rest of the objects are loaded now.");
                foreach (PropertyViewModel prop in Model.ClientProperties.Where(p => p.Object == null && !p.IsPrimaryKey))
                {
                    if (prop.Type.IsDate)
                    {   // Using valueOf/getTime here is a 20x performance increase over moment.isSame(). moment(new Date(...)) is also a 10x perf increase.
                        b.Line($"if (data.{prop.JsonName} == null) this.{prop.JsVariable}(null);");
                        b.Line($"else if (this.{prop.JsVariable}() == null || this.{prop.JsVariable}()!.valueOf() != new Date(data.{prop.JsonName}).getTime()){{");
                        b.Indented($"this.{prop.JsVariable}(moment(new Date(data.{prop.JsonName})));");
                        b.Line("}");
                    }
                    else
                    {
                        b.Line($"this.{prop.JsVariable}(data.{prop.JsonName});");
                    }
                }

                b.Line("if (this.coalesceConfig.onLoadFromDto()){");
                b.Indented("this.coalesceConfig.onLoadFromDto()(this as any);");
                b.Line("}");
                b.Line("this.isLoading(false);");
                b.Line("this.isDirty(false);");
                b.Line("if (this.coalesceConfig.validateOnLoadFromDto()) this.validate();");
            }
        }
    }
}
