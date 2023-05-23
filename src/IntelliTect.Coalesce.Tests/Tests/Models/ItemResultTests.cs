using IntelliTect.Coalesce.Models;
using IntelliTect.Coalesce.Tests.TargetClasses;
using IntelliTect.Coalesce.Tests.TargetClasses.TestDbContext;
using IntelliTect.Coalesce.Tests.Util;
using IntelliTect.Coalesce.TypeDefinition;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace IntelliTect.Coalesce.Tests.Tests.Models
{
    public class ItemResultTests
    {
        public ItemResultTests()
        {
            ReflectionRepositoryFactory.Initialize();
        }

        [Fact]
        public void FromValidation_ForGenDto_ProducesErrors()
        {
            var dto = new ValidationTargetDtoGen()
            {
                ProductId = "asdf",
                Email = "x@",
                Number = 42
            };

            var result = ItemResult.FromValidation(dto);

            Assert.False(result.WasSuccessful);
            Assert.Equal("""
                The field Product Id must match the regular expression '^product:\d+:\d+$'.
                Custom email error message
                The field Email must be a string with a minimum length of 3 and a maximum length of 150.
                The field Fancy Number must be between 5 and 10.
                """.ReplaceLineEndings(), result.Message.ReplaceLineEndings());
        }

        [Fact]
        public void FromValidation_ForGenDto_ValidatesOnlySetProperties()
        {
            var dto = new ValidationTargetDtoGen()
            {
                ProductId = "product:1:3"
            };

            var result = ItemResult.FromValidation(dto);

            Assert.True(result.WasSuccessful);
        }

        [Fact]
        public void FromValidation_ForGenDto_WhenForceRequired_ProducesErrorsForUnsetProperties()
        {
            var dto = new ValidationTargetDtoGen()
            {
                ProductId = "product:1:3"
            };

            var result = ItemResult.FromValidation(dto, forceRequired: true);

            Assert.False(result.WasSuccessful);
            Assert.Equal("""
                The Fancy Number field is required.
                The Required Child field is required.
                """.ReplaceLineEndings(), result.Message.ReplaceLineEndings());
        }

        [Fact]
        public void FromValidation_ForGenDto_WhenDeep_ProducesErrorsForNestedUnsetProperties()
        {
            var dto = new ValidationTargetDtoGen()
            {
                ProductId = "product:1:3",
                RequiredChild = new()
            };

            var result = ItemResult.FromValidation(dto, deep: true, forceRequired: true);

            Assert.False(result.WasSuccessful);
            Assert.Equal("""
                The Fancy Number field is required.
                The Required Val field is required.
                """.ReplaceLineEndings(), result.Message.ReplaceLineEndings());
        }

        [Fact]
        public void FromValidation_ForGenDto_WhenPkIsIdentity_DoesNotProduceError()
        {
            var dto = new StringIdentityDtoGen();

            var result = ItemResult.FromValidation(dto, deep: true, forceRequired: true);

            Assert.True(result.WasSuccessful);
        }



        [Fact]
        public void FromValidation_ForModel_ProducesErrors()
        {
            var dto = new ValidationTarget()
            {
                ProductId = "asdf",
                Email = "x@",
                Number = 42
            };

            var result = ItemResult.FromValidation(dto);

            Assert.False(result.WasSuccessful);
            Assert.Equal("""
                The field Product Id must match the regular expression '^product:\d+:\d+$'.
                Custom email error message
                The field Email must be a string with a minimum length of 3 and a maximum length of 150.
                The field Fancy Number must be between 5 and 10.
                The Required Child field is required.
                """.ReplaceLineEndings(), result.Message.ReplaceLineEndings());
        }

        [Fact]
        public void FromValidation_ForModel_WhenDeep_ProducesErrorsForNestedProperties()
        {
            var dto = new ValidationTarget()
            {
                ProductId = "product:1:3",
                Email = "x@x.com", 
                Number = 7,
                RequiredChild = new()
                {
                    String = "asdf"
                }
            };

            var result = ItemResult.FromValidation(dto, deep: true);

            Assert.False(result.WasSuccessful);
            Assert.Equal("""
                The String field is not a valid fully-qualified http, https, or ftp URL.
                The Required Val field is required.
                """.ReplaceLineEndings(), result.Message?.ReplaceLineEndings());
        }

        [Fact]
        public void FromParameterValidation_ValidatesParameterAttributes()
        {
            var method = ReflectionRepository.Global
                .GetClassViewModel<ValidationTarget>()
                .MethodByName(nameof(ValidationTarget.MethodWithMixedParameters));
            var dto = new
            {
                email = "x@",
            };

            var result = ItemResult.FromParameterValidation(method, dto);

            Assert.False(result.WasSuccessful);
            Assert.Equal("""
                The Email field is not a valid e-mail address.
                The Target field is required.
                """.ReplaceLineEndings(), result.Message?.ReplaceLineEndings());
        }

        [Fact]
        public void FromParameterValidation_ValidatesComplexParameters()
        {
            var method = ReflectionRepository.Global
                .GetClassViewModel<ValidationTarget>()
                .MethodByName(nameof(ValidationTarget.MethodWithMixedParameters));
            var dto = new
            {
                email = "x@x.com",
                target = new ValidationTargetDtoGen { }
            };

            var result = ItemResult.FromParameterValidation(method, dto);

            Assert.False(result.WasSuccessful);
            Assert.Equal("""
                The Fancy Number field is required.
                The Required Child field is required.
                """.ReplaceLineEndings(), result.Message?.ReplaceLineEndings());
        }

        [Fact]
        public void FromParameterValidation_ValidatesNestedComplexParameters()
        {
            var method = ReflectionRepository.Global
                .GetClassViewModel<ValidationTarget>()
                .MethodByName(nameof(ValidationTarget.MethodWithMixedParameters));
            var dto = new
            {
                email = "x@x.com",
                target = new ValidationTargetDtoGen
                {
                    Number = 7,
                    RequiredChild = new()
                }
            };

            var result = ItemResult.FromParameterValidation(method, dto);

            Assert.False(result.WasSuccessful);
            Assert.Equal("""
                The Required Val field is required.
                """.ReplaceLineEndings(), result.Message?.ReplaceLineEndings());
        }

    }
}
