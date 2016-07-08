using Coalesce.Web.Tests.Helpers;
using Intellitect.ComponentModel.DataAnnotations;
using Intellitect.ComponentModel.TypeDefinition;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace Coalesce.Web.Tests
{
    public class SecurityTestsUser : SecurityTestsCommon
    {
        public SecurityTestsUser(TestModel model, StreamWriter output)
            : base (model, output, "User", "Standard User") { }

        public override Task<bool> RunTests()
        {
            return base.RunTests();
        }
    }
}
