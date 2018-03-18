﻿using IntelliTect.Coalesce.CodeGeneration.Generation;
using IntelliTect.Coalesce.TypeDefinition;
using System;
using System.Collections.Generic;
using System.Text;
using IntelliTect.Coalesce.CodeGeneration.Knockout.BaseGenerators;
using IntelliTect.Coalesce.Utilities;
using IntelliTect.Coalesce.Knockout.TypeDefinition;

namespace IntelliTect.Coalesce.CodeGeneration.Knockout.Generators
{
    public class KoExternalType : KnockoutViewModelGenerator
    {
        public KoExternalType(GeneratorServices services) : base(services) { }
        
        public override void BuildOutput(TypeScriptCodeBuilder b)
        {
            using (b.Block($"module {ViewModelModuleName}"))
            {
                b.DocComment($"External Type {Model.Name}");
                WriteViewModelClass(b);
            }
        }

        private void WriteViewModelClass(TypeScriptCodeBuilder b)
        {
            using (b.Block($"export class {Model.ViewModelGeneratedClassName}"))
            {
                if (Model.PrimaryKey != null)
                {
                    // ID of the object.
                    b.Line("public myId: any = 0;");
                }

                b.Line();
                b.Line("// Observables");
                foreach (PropertyViewModel prop in Model.ClientProperties)
                {
                    b.Line($"public {prop.JsVariable}: {prop.Type.TsKnockoutType(true)} = {prop.Type.ObservableConstructorCall()};");
                    if (prop.PureType.IsEnum)
                    {
                        b.Line($"public {prop.JsTextPropertyName} = {prop.Type.ObservableConstructorCall()};");
                    }
                }

                b.Line("public parent: any;");
                b.Line("public parentCollection: any;");

                b.Line();
                WriteMethod_LoadFromDto(b);

                b.Line();
                WriteMethod_SaveToDto(b);

                b.Line();
                using (b.Block("constructor(newItem?: any, parent?: any)"))
                {
                    b.Line("this.parent = parent;");
                    b.Line();
                    b.Line("if (newItem) {");
                    b.Line("    this.loadFromDto(newItem);");
                    b.Line("}");
                }
            }
        }

        private void WriteMethod_LoadFromDto(TypeScriptCodeBuilder b)
        {
            b.DocComment(new[]{
                "Load the object from the DTO.",
                "@param data: The incoming data object to load.",
            });
            using (b.Block("public loadFromDto = (data: any) =>", ';'))
            {
                b.Line("if (!data) return;");

                if (Model.PrimaryKey != null)
                {
                    b.Line("// Set the ID");
                    b.Line($"this.myId = data.{Model.PrimaryKey.JsonName};");
                }

                b.Line();
                b.Line("// Load the properties.");
                foreach (PropertyViewModel prop in Model.ClientProperties)
                {
                    if (prop.Type.IsCollection && prop.Type.ClassViewModel != null)
                    {
                        b.Line($"if (data.{prop.JsonName} != null) {{");
                        b.Line("// Merge the incoming array");
                        if (prop.Type.PureType.ClassViewModel.PrimaryKey != null)
                        {
                            b.Indented($"Coalesce.KnockoutUtilities.RebuildArray(this.{prop.JsVariable}, data.{prop.JsonName}, \'{prop.Type.PureType.ClassViewModel.PrimaryKey.JsVariable}\', ViewModels.{prop.Type.PureType.ClassViewModel.Name}, self, true);");
                        }
                        else
                        {
                            b.Indented($"Coalesce.KnockoutUtilities.RebuildArray(this.{prop.JsVariable}, data.{prop.JsonName}, null, ViewModels.{prop.Type.PureType.ClassViewModel.Name}, this, true);");
                        }
                        b.Line("}");
                    }
                    else if (prop.Type.IsDate)
                    {
                        b.Line($"if (data.{prop.JsVariable} == null) this.{prop.JsVariable}(null);");
                        b.Line($"else if (this.{prop.JsVariable}() == null || !this.{prop.JsVariable}()!.isSame(moment(data.{prop.JsVariable}))) {{");
                        b.Indented($"this.{prop.JsVariable}(moment(data.{prop.JsVariable}));");
                        b.Line("}");
                    }
                    else if (prop.IsPOCO)
                    {
                        b.Line($"if (!this.{prop.JsVariable}()){{");
                        b.Indented($"this.{prop.JsVariable}(new {prop.Object.ViewModelClassName}(data.{prop.JsonName}, this));");
                        b.Line("} else {");
                        b.Indented($"this.{prop.JsVariable}()!.loadFromDto(data.{prop.JsonName});");
                        b.Line("}");
                    }
                    else
                    {
                        b.Line($"this.{prop.JsVariable}(data.{prop.JsVariable});");
                    }
                }
                b.Line();
            }
        }
    }
}
