using IntelliTect.Coalesce.CodeGeneration.Api.BaseGenerators;
using IntelliTect.Coalesce.CodeGeneration.Api.Generators;
using IntelliTect.Coalesce.CodeGeneration.Generation;
using IntelliTect.Coalesce.TypeDefinition;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace IntelliTect.Coalesce.CodeGeneration.Vue.Generators
{
    public class Controllers : IntelliTect.Coalesce.CodeGeneration.Api.Generators.Controllers
    {
        public Controllers(CompositeGeneratorServices services) : base(services) { }
    }
}
