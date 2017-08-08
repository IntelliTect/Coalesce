using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.CodeAnalysis;
using Microsoft.Extensions.PlatformAbstractions;
using Microsoft.Extensions.ProjectModel;
using Microsoft.VisualStudio.Web.CodeGeneration;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace IntelliTect.Coalesce.CodeGeneration.Documentation
{
    public class AttributeInfo
    {
        public string Name { get; set; }
        public string Signature { get; set; }
        public string Comment { get; set; }
        public string ValidValues { get; set; }
    }

    public class InterfaceInfo
    {
        public string Name { get; set; }
        public IEnumerable<string> Methods { get; set; }
        public IEnumerable<string> Properties { get; set; }
    }

    public class BaseControllerInfo
    {
        public string Name { get; set; }
        public IEnumerable<string> Methods { get; set; }
        public IEnumerable<string> Properties { get; set; }
    }

    public class ModelInfo
    {
        public string Name { get; set; }
        public IEnumerable<string> Methods { get; set; }
        public IEnumerable<string> Properties { get; set; }
    }

    public class DocumentationViewModel
    {
        public List<AttributeInfo> Attributes { get; set; }
        public List<InterfaceInfo> Interfaces { get; set; }
        public List<BaseControllerInfo> BaseControllers { get; set; }
        public List<ModelInfo> Models { get; set; }
    }

    public class DocumentationGenerator : CommonGeneratorBase
    {
        public const string ThisAssemblyName = "IntelliTect.Coalesce.CodeGeneration";

        public DocumentationGenerator(
            IModelTypesLocator modelTypesLocator,
            ICodeGeneratorActionsService codeGeneratorActionsService,
            IServiceProvider serviceProvider,
            ILogger logger)
            : base(PlatformServices.Default.Application)
        {
            ModelTypesLocator = modelTypesLocator;
            ServiceProvider = serviceProvider;
            CodeGeneratorActionsService = codeGeneratorActionsService;
        }

        public IModelTypesLocator ModelTypesLocator { get; }
        protected IProjectContext ProjectContext { get; }
        protected IServiceProvider ServiceProvider { get; }
        protected ICodeGeneratorActionsService CodeGeneratorActionsService { get; }

        public IEnumerable<string> TemplateFolders => TemplateFoldersUtilities.GetTemplateFolders(
            ThisAssemblyName,
            ApplicationEnvironment.ApplicationBasePath,
            new[] {""},
            ProjectContext);

        internal async Task Generate(CommandLineGeneratorModel model)
        {
            var viewModel = new DocumentationViewModel();

            viewModel.Attributes = new List<AttributeInfo>();
            viewModel.Interfaces = new List<InterfaceInfo>();
            viewModel.BaseControllers = new List<BaseControllerInfo>();
            viewModel.Models = new List<ModelInfo>();

            ParseAttributes(viewModel);
            ParseInterfaces(viewModel);
            ParseBaseControllers(viewModel);
            ParseModels(viewModel);

            string viewOutputPath = Path.Combine(
                ApplicationEnvironment.ApplicationBasePath,
                "Views",
                "Home",
                "Documentation.cshtml");
            await CodeGeneratorActionsService.AddFileFromTemplateAsync(viewOutputPath,
                "StaticDocumentationBuilder.cshtml", TemplateFolders, viewModel);
        }

        private void ParseAttributes(DocumentationViewModel viewModel)
        {
            AttributeInfo attributeInfo;

            foreach (ModelType modelType in ModelTypesLocator.GetAllTypes()
                .Where(t => t.FullName.StartsWith("IntelliTect.Coalesce") && t.TypeSymbol.BaseType != null &&
                            t.TypeSymbol.BaseType.Name == "Attribute"))
            {
                attributeInfo = new AttributeInfo();

                attributeInfo.Name = modelType.Name.EndsWith("Attribute")
                    ? modelType.Name.Substring(0, modelType.Name.Length - "Attribute".Length)
                    : modelType.Name;
                foreach (IMethodSymbol constructor in ((INamedTypeSymbol) modelType.TypeSymbol).Constructors)
                {
                    attributeInfo.Signature = RetrieveMethodSignature(constructor);
                    attributeInfo.ValidValues = RetrieveValidEnumValues(constructor);
                    break;
                }
                attributeInfo.Comment = ExtractXmlComments(modelType.TypeSymbol);

                viewModel.Attributes.Add(attributeInfo);
            }
        }

        private void ParseInterfaces(DocumentationViewModel viewModel)
        {
            InterfaceInfo interfaceInfo;

            foreach (ModelType modelType in ModelTypesLocator.GetAllTypes()
                .Where(t => t.FullName.StartsWith("IntelliTect.Coalesce") &&
                            t.TypeSymbol.TypeKind == TypeKind.Interface))
            {
                // get the interface declaration
                interfaceInfo = new InterfaceInfo();
                interfaceInfo.Name = HtmlEncoder.Default.Encode(GetClassDefinition(modelType));

                interfaceInfo.Properties = RetrieveProperties(modelType.TypeSymbol as INamedTypeSymbol)
                    .Select(p => HtmlEncoder.Default.Encode(p));
                interfaceInfo.Methods = RetrieveMethods(modelType.TypeSymbol as INamedTypeSymbol)
                    .Select(m => HtmlEncoder.Default.Encode(m));

                viewModel.Interfaces.Add(interfaceInfo);
            }
        }

        private void ParseBaseControllers(DocumentationViewModel viewModel)
        {
            BaseControllerInfo baseControllerInfo;

            foreach (ModelType modelType in ModelTypesLocator.GetAllTypes()
                .Where(t => t.FullName.StartsWith("IntelliTect.Coalesce.Controllers")))
            {
                baseControllerInfo = new BaseControllerInfo();
                baseControllerInfo.Name = HtmlEncoder.Default.Encode(GetClassDefinition(modelType));

                baseControllerInfo.Properties = RetrieveProperties(modelType.TypeSymbol as INamedTypeSymbol)
                    .Select(p => HtmlEncoder.Default.Encode(p));
                baseControllerInfo.Methods = RetrieveMethods(modelType.TypeSymbol as INamedTypeSymbol)
                    .Select(m => HtmlEncoder.Default.Encode(m));

                viewModel.BaseControllers.Add(baseControllerInfo);
            }
        }

        private void ParseModels(DocumentationViewModel viewModel)
        {
            var modelsWeCareAbout = new List<string> {"ListResult", "SaveResult", "ValidationIssue"};
            ModelInfo modelInfo;

            foreach (ModelType modelType in ModelTypesLocator.GetAllTypes()
                .Where(t => t.FullName.StartsWith("IntelliTect.Coalesce.Models")))
                if (modelsWeCareAbout.Contains(modelType.Name))
                {
                    modelInfo = new ModelInfo();
                    modelInfo.Name = HtmlEncoder.Default.Encode(GetClassDefinition(modelType));

                    modelInfo.Properties = RetrieveProperties(modelType.TypeSymbol as INamedTypeSymbol)
                        .Select(p => HtmlEncoder.Default.Encode(p));
                    modelInfo.Methods = RetrieveMethods(modelType.TypeSymbol as INamedTypeSymbol)
                        .Select(m => HtmlEncoder.Default.Encode(m));

                    viewModel.Models.Add(modelInfo);
                }
        }

        #region Helpers

        public static string RetrieveValidEnumValues(IMethodSymbol methodSymbol)
        {
            string validValues = null;

            foreach (IParameterSymbol parameter in methodSymbol.Parameters)
                if (parameter.Type.TypeKind == TypeKind.Enum)
                    foreach (string memberName in ((INamedTypeSymbol) parameter.Type).MemberNames)
                    {
                        if (string.IsNullOrWhiteSpace(validValues)) validValues = "";
                        else validValues += ", ";
                        validValues += $"{parameter.Type.MetadataName}.{memberName}";
                    }

            return validValues;
        }

        public static string RetrieveMethodSignature(IMethodSymbol methodSymbol)
        {
            var signature = "(";
            string parameterType;
            foreach (IParameterSymbol parameter in methodSymbol.Parameters)
            {
                if (parameter.Type.TypeKind == TypeKind.Enum)
                    parameterType = parameter.Type.MetadataName;
                else
                    parameterType = parameter.Type.ToString();

                if (signature.Length > 1)
                    signature += ", ";
                if (parameter.HasExplicitDefaultValue)
                    if (parameter.Type.TypeKind == TypeKind.Enum)
                    {
                        ISymbol member = ((INamedTypeSymbol) parameter.Type).GetMembers()
                            .SingleOrDefault(m => m is IFieldSymbol &&
                                                  ((IFieldSymbol) m).ConstantValue == parameter.ExplicitDefaultValue);
                        signature += $"{parameterType} {parameter.Name} = {parameterType}.{member.Name}";
                    }
                    else if (parameter.ExplicitDefaultValue == null)
                    {
                        signature += $"{parameterType} {parameter.Name} = null";
                    }
                    else
                    {
                        signature += $"{parameterType} {parameter.Name} = {parameter.ExplicitDefaultValue}";
                    }
                else
                    signature += $"{parameterType} {parameter.Name}";
            }
            signature += ")";

            return signature;
        }

        public static string ExtractXmlComments(ISymbol symbol)
        {
            var xmlDocumentation = new XmlDocument();
            xmlDocumentation.LoadXml(symbol.GetDocumentationCommentXml());
            return xmlDocumentation.SelectSingleNode("/member/summary").InnerText.Trim();
        }

        public static string GetClassDefinition(ModelType modelType)
        {
            string classDefinition = modelType.Name;
            var namedTypeSymbol = (INamedTypeSymbol) modelType.TypeSymbol;

            if (namedTypeSymbol.IsGenericType)
            {
                classDefinition += "<";
                foreach (ITypeSymbol genericType in namedTypeSymbol.TypeArguments)
                {
                    if (!classDefinition.EndsWith("<"))
                        classDefinition += ", ";
                    classDefinition += genericType.Name;
                }
                classDefinition += ">";
            }

            return classDefinition;
        }

        public static List<string> RetrieveProperties(INamedTypeSymbol namedTypeSymbol)
        {
            var properties = new List<string>();

            foreach (IPropertySymbol propertyInfo in namedTypeSymbol.GetMembers()
                .Where(m => m.Kind == SymbolKind.Property))
                properties.Add($"{propertyInfo.Type.Name} {propertyInfo.Name}");

            return properties;
        }

        public static List<string> RetrieveMethods(INamedTypeSymbol namedTypeSymbol)
        {
            var methods = new List<string>();

            foreach (IMethodSymbol methodInfo in namedTypeSymbol.GetMembers().Where(m => m.Kind == SymbolKind.Method))
                if (methodInfo.MethodKind != MethodKind.PropertyGet && methodInfo.MethodKind != MethodKind.PropertySet)
                    methods.Add($"{methodInfo.ReturnType} {methodInfo.Name}{RetrieveMethodSignature(methodInfo)}");

            return methods;
        }

        #endregion

        #region junkCodeButKeepingItAroundForFutureReference

        //// get the syntax tree
        //var syntaxTree = modelType.TypeSymbol.DeclaringSyntaxReferences[0].SyntaxTree;
        //methodSpans = new List<int>();
        //foreach (var childSymbol in modelType.TypeSymbol.GetMembers().Where(m => m.Kind == Microsoft.CodeAnalysis.SymbolKind.Method))
        //{
        //    if (childSymbol.DeclaringSyntaxReferences.Length > 0)
        //    {
        //        var childSyntaxReference = childSymbol.DeclaringSyntaxReferences[0];
        //        Console.WriteLine(childSyntaxReference.Span.Start);
        //        methodSpans.Add(childSyntaxReference.Span.Start);
        //    }
        //}

        //if (syntaxTree.HasCompilationUnitRoot)
        //{
        //    // get the root element of the syntax tree, in a simple case, it will be the CompilationUnit
        //    CompilationUnitSyntax compilationUnit = syntaxTree.GetRoot() as CompilationUnitSyntax;
        //    // A CompilationUnit will have a NamespaceDeclaration
        //    NamespaceDeclarationSyntax namespaceDeclaration = compilationUnit.Members[0] as NamespaceDeclarationSyntax;
        //    // A NamespaceDeclaration will have 1 or more ClassDeclorations
        //    foreach (ClassDeclarationSyntax classDeclaration in namespaceDeclaration.Members)
        //    {
        //        if (classDeclaration.HasLeadingTrivia)
        //        {
        //            foreach (var leadingTrivia in classDeclaration.GetLeadingTrivia().Where(t => t.Kind() != SyntaxKind.WhitespaceTrivia && t.Kind() != SyntaxKind.EndOfLineTrivia))
        //            {
        //                var leadingTriviaString = leadingTrivia.ToString().Replace("///", "");
        //                XmlDocument document = new XmlDocument();
        //                document.LoadXml(leadingTriviaString);
        //                attributeInfo.Comment = document.InnerText.Trim();
        //            }
        //        }
        //        // A ClassDeclaration can have FieldDeclarations, ConstructorDeclarations, and MethodDeclarations
        //    }
        //    //foreach (int methodSpanStart in methodSpans)
        //    //{
        //    //    var syntaxToken = compilationUnit.FindToken(methodSpanStart);
        //    //    if (syntaxToken.HasLeadingTrivia)
        //    //    {
        //    //        foreach (var trivia in syntaxToken.Parent.GetLeadingTrivia().Where(t => t.Kind() != SyntaxKind.WhitespaceTrivia))
        //    //        {
        //    //            Console.WriteLine(trivia.GetType());
        //    //        }
        //    //        //foreach (var leadingTrivia in syntaxToken.)
        //    //    }
        //    //}
        //}

        //foreach (var trivia in modelType.TypeSymbol.DeclaringSyntaxReferences[0].SyntaxTree.GetRoot().DescendantTrivia())
        //{
        //    if (trivia.Kind() == SyntaxKind.SingleLineDocumentationCommentTrivia)
        //    {
        //        //modelType.TypeSymbol.DeclaringSyntaxReferences[0].SyntaxTree.ToString().Substring(trivia.Span.Start, trivia.Span.Length)
        //        Console.WriteLine(modelType.TypeSymbol.DeclaringSyntaxReferences[0].SyntaxTree.ToString().Substring(trivia.Span.Start, trivia.Span.Length));
        //    }
        //    //Console.WriteLine(trivia.Kind());
        //}
        //foreach (var member in modelType.TypeSymbol.GetMembers())
        //{
        //    Console.WriteLine($"\t{member.Name}");
        //    foreach (var attribute in member.GetAttributes())
        //    {
        //        Console.WriteLine($"\t\t{attribute.AttributeClass.Name}");
        //    }
        //}

        #endregion
    }
}