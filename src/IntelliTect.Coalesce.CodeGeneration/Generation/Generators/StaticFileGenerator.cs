using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IntelliTect.Coalesce.CodeGeneration.Templating;
using IntelliTect.Coalesce.CodeGeneration.Templating.Resolution;
using IntelliTect.Coalesce.CodeGeneration.Configuration;
using IntelliTect.Coalesce.CodeGeneration.Utilities;
using Microsoft.Extensions.Logging;

namespace IntelliTect.Coalesce.CodeGeneration.Generation
{
    public class StaticFileGenerator : FileGenerator
    {
        protected ITemplateResolver Resolver { get; }

        public StaticFileGenerator(GeneratorServices services, ITemplateResolver resolver) : base(services)
        {
            Resolver = resolver;
        }

        public TemplateDescriptor Template { get; set; }

        public bool NoOverwrite { get; set; } = false;


        public override Task<Stream> GetOutputAsync()
        {
            return Task.FromResult(Resolver.Resolve(Template).GetContents());
        }

        public override bool ShouldGenerate() => NoOverwrite ? !File.Exists(OutputPath) : true;

        public override string ToString()
        {
            if (OutputPath != null)
            {
                return $"{Template.ToString()} => {OutputPath}";
            }
            return Template.ToString();
        }

        #region Fluent

        public StaticFileGenerator WithTemplate(TemplateDescriptor template)
        {
            this.Template = template;
            return this;
        }

        public StaticFileGenerator PreventOverwrite(bool noOverwrite = true)
        {
            NoOverwrite = noOverwrite;
            return this;
        }

        #endregion
    }
}
