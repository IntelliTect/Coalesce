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
        public void CasePlainAttachmentProperty_ReadAllowAll_ControllerAllowAnonymous()
        {
            Type caseController = typeof(CaseController);

            var methodInfo = caseController.GetMembers()
                .First(m => m.Name.Equals("PlainAttachmentGet"));

            Assert.IsTrue(methodInfo.GetCustomAttributes(typeof(AllowAnonymousAttribute)).Any());
        }

        [TestMethod]
        public void CasePlainAttachmentProperty_EditPermitAuthorizedOnly_ControllerHasAttributeAuthorized()
        {
            Type caseController = typeof(CaseController);
            var methodInfo = caseController.GetMembers()
                .First(m => m.Name.Equals("PlainAttachmentPut"));
            var authorizedRoles = methodInfo.GetCustomAttribute<AuthorizeAttribute>().Roles;

            Assert.IsTrue(methodInfo.GetCustomAttributes(typeof(AuthorizeAttribute)).Any());
            Assert.IsNull(authorizedRoles);

        }

        [TestMethod]
        [DataRow("RestrictedDownloadAttachmentGet", "Admin")]
        [DataRow("RestrictedUploadAttachmentPut", "Admin, SuperUser")]
        public void PropertyHasRoleRetriction_ContollerHasRoles(string controllerMethodName, string roles)
        {
            Type caseController = typeof(CaseController);
            var methodInfo = caseController.GetMembers()
                .First(m => m.Name.Equals(controllerMethodName));
            var authorizedRoles = methodInfo.GetCustomAttribute<AuthorizeAttribute>().Roles;
            var expectedRoles = roles.Split(",", StringSplitOptions.RemoveEmptyEntries);

            Assert.IsTrue(methodInfo.GetCustomAttributes(typeof(AuthorizeAttribute)).Any());
            foreach (string role in expectedRoles)
            {
                Assert.IsTrue(authorizedRoles.Contains(role.Trim()));
            }
        }

    }
}
