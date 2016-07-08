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
    public class SecurityTestsAnonymous : SecurityTestsCommon
    {
        public SecurityTestsAnonymous(TestModel model, StreamWriter output)
            : base (model, output, "", "Anonymous User") { }

        public override Task<bool> RunTests()
        {
            return base.RunTests();
        }

        //if (response.IsSuccessStatusCode)
        //{
        //    using (var content = response.Content)
        //    {
        //        if (content != null)
        //        {
        //            string contents = await content.ReadAsStringAsync();
        //            dynamic data = JsonConvert.DeserializeObject<dynamic>(contents);
        //            bool ws = data.wasSuccessful;
        //            int tc = data.totalCount;
        //            _model.Logger.LogInformation($"{ws}: {tc}");
        //            //_model.Logger.LogInformation(data.First());
        //            //_model.Logger.LogInformation(contents);
        //            return true;
        //        }
        //    }
        //}
    }
}
