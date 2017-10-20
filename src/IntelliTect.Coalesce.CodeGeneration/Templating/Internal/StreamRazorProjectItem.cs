using Microsoft.AspNetCore.Razor.Language;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;

namespace IntelliTect.Coalesce.CodeGeneration.Templating.Internal
{
    // TODO: delete me if unused.
    public class StreamRazorProjectItem : RazorProjectItem
    {
        private Stream stream;

        public StreamRazorProjectItem(Stream stream)
        {
            this.stream = stream;
        }

        public override string BasePath => "";

        public override string FilePath => "";

        public override string PhysicalPath => "";

        public override bool Exists => true;

        public override Stream Read() => stream;
    }
}
