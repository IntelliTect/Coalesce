using Coalesce.Web.Api;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Xunit;

namespace IntelliTect.Coalesce.Web.Tests
{
    //These tests rely upon the Coalesce.Web project controllers having been generated
    //by Coalesce prior to test execution.
    public class SecurityPropertyTests
    {
        public static IEnumerable<object[]> RestrictedDownloadHasRoles()
        {
            yield return new object[]
            {
                typeof(CaseController).GetMethod(nameof(CaseController.RestrictedDownloadAttachmentGet)),
                "Admin"
            };
            yield return new object[]
            {
                typeof(CaseController).GetMethod(nameof(CaseController.RestrictedUploadAttachmentPut)),
                "ADMIN, SuperUser"
            };
        }

        [Theory]
        [MemberData(nameof(RestrictedDownloadHasRoles))]
        public void PropertyHasRoleRetriction_GeneratedControllerHasRoles(
            MethodInfo methodInfo,
            string roles)
        {
            var authorizedRoles = methodInfo.GetCustomAttribute<AuthorizeAttribute>().Roles;
            var expectedRoles = roles.Split(",", StringSplitOptions.RemoveEmptyEntries);

            Assert.True(methodInfo.GetCustomAttributes(typeof(AuthorizeAttribute)).Any());
            foreach (string role in expectedRoles)
            {
                Assert.True(authorizedRoles.Contains(role.Trim(), StringComparison.InvariantCultureIgnoreCase));
            }
        }


        public static IEnumerable<object[]> PlainAttachmentHasAttributes()
        {
            yield return new object[]
            {
                typeof(CaseController).GetMethod(nameof(CaseController.PlainAttachmentGet)),
                typeof(AllowAnonymousAttribute)
            };
            yield return new object[]
            {
                typeof(CaseController).GetMethod(nameof(CaseController.PlainAttachmentPut)),
                typeof(AuthorizeAttribute)
            };
        }
        [Theory]
        [MemberData(nameof(PlainAttachmentHasAttributes))]
        public void PropertyHasSecurityAttribute_GeneratedControllerHasExpectedAttribute(
            MethodInfo methodInfo,
            Type expectedControllerAttribute)
        {
            Assert.True(methodInfo.GetCustomAttributes(expectedControllerAttribute).Any());
        }

    }
}
