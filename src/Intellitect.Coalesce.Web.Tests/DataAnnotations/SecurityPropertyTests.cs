using Coalesce.Web.Api;
using Microsoft.AspNetCore.Authorization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Intellitect.Coalesce.Web.Tests.DataAnnotations
{
    //These tests rely upon the Coalesce.Web project controllers having been generated
    //by Coalesce prior to test execution.
    [TestClass]
    public class SecurityPropertyTests
    {
        [TestMethod]
        [DataRow(typeof(CaseController), "RestrictedDownloadAttachmentGet", "Admin")]
        [DataRow(typeof(CaseController), "RestrictedUploadAttachmentPut", "ADMIN, SuperUser")]
        public void PropertyHasRoleRetriction_GeneratedControllerHasRoles(
            Type controller, 
            string controllerMethodName, 
            string roles)
        {
            var methodInfo = controller.GetMembers()
                .First(m => m.Name.Equals(controllerMethodName));
            var authorizedRoles = methodInfo.GetCustomAttribute<AuthorizeAttribute>().Roles;
            var expectedRoles = roles.Split(",", StringSplitOptions.RemoveEmptyEntries);

            Assert.IsTrue(methodInfo.GetCustomAttributes(typeof(AuthorizeAttribute)).Any());
            foreach (string role in expectedRoles)
            {
                Assert.IsTrue(authorizedRoles.Contains(role.Trim(), StringComparison.InvariantCultureIgnoreCase));
            }
        }

        [TestMethod]
        [DataRow(typeof(CaseController),"PlainAttachmentPut", typeof(AuthorizeAttribute))]
        [DataRow(typeof(CaseController),"PlainAttachmentGet", typeof(AllowAnonymousAttribute))]
        public void PropertyHasSecurityAttribute_GeneratedControllerHasExpectedAttribute(
            Type controller,
            string controllerMethodName, 
            Type expectedControllerAttribute)
        {
            var methodInfo = controller.GetMembers()
                .First(m => m.Name.Equals(controllerMethodName));
            Assert.IsTrue(methodInfo.GetCustomAttributes(expectedControllerAttribute).Any());
        }

    }
}
