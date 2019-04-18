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
        public void CasePlainAttachmentProperty_ReadPermitAuthorizedOnly_ControllerHasAttributeAuthorized()
        {
            Type caseController = typeof(CaseController);

            var methodInfo = caseController.GetMembers()
                .First(m => m.Name.Equals("PlainAttachmentGet"));
            var authorizedRoles = methodInfo.GetCustomAttribute<AuthorizeAttribute>().Roles;
            Assert.IsTrue(methodInfo.GetCustomAttributes(typeof(AuthorizeAttribute)).Any());
            Assert.IsNull(authorizedRoles);
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
        public void CaseRestrictedDownloadProperty_ReadAdminRoleOnly_ControllerHasRoleAdmin()
        {
            Type caseController = typeof(CaseController);
            var methodInfo = caseController.GetMembers()
                .First(m => m.Name.Equals("RestrictedDownloadAttachmentGet"));
             var authorizedRoles = methodInfo.GetCustomAttribute<AuthorizeAttribute>().Roles.Split();

            Assert.IsTrue(methodInfo.GetCustomAttributes(typeof(AuthorizeAttribute)).Any());
            Assert.IsTrue(authorizedRoles.Contains<string>("Admin"));
        }

        [TestMethod]
        public void CaseRestrictedUploadProperty_EditAdminRoleOnly_ControllerHasRoleAdmin()
        {
            Type caseController = typeof(CaseController);
            var methodInfo = caseController.GetMembers()
                .First(m => m.Name.Equals("RestrictedUploadAttachmentPut"));
            var authorizedRoles = methodInfo.GetCustomAttribute<AuthorizeAttribute>().Roles.Split();

            Assert.IsTrue(methodInfo.GetCustomAttributes(typeof(AuthorizeAttribute)).Any());
            Assert.IsTrue(authorizedRoles.Contains<string>("Admin"));

        }


    }
}
