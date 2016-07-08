using Intellitect.ComponentModel.TypeDefinition;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;

namespace Coalesce.Web.Tests.Helpers
{
    public class TestModel
    {
        public List<ClassViewModel> Models { get; set; }
        public Uri ApiUrl { get; set; }
    }
}
