using Coalesce.Web.Models;
using Coalesce.Web.Tests.Helpers;
using IntelliTect.Coalesce.DataAnnotations;
using IntelliTect.Coalesce.TypeDefinition;
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

        public override async Task<bool> RunTests()
        {
            var baseTests = await base.RunTests();
            var roleRestricted = await RoleRestrictedCreate(HttpStatusCode.NotFound);
            var authorized = await AuthorizedCreate(HttpStatusCode.OK);
            var anonymous = await AnonymousCreate(HttpStatusCode.OK);
            var notAllowed = await NotAllowedCreate(HttpStatusCode.NotFound);

            var results = baseTests
                && roleRestricted
                && authorized
                && anonymous
                && notAllowed;

            return results;
        }
    }
}
