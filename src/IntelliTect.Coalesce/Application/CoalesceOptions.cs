using IntelliTect.Coalesce.DataAnnotations;
using IntelliTect.Coalesce.Models;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.ComponentModel.DataAnnotations;

namespace IntelliTect.Coalesce
{
    public class CoalesceOptions
    {
        /// <summary>
        /// Determines whether API controllers will return the <see cref="Exception.Message"/> 
        /// of unhandled exceptions or not. 
        /// Defaults to true if IHostingEnvironment.EnvironmentName is "Development"; otherwise false.
        /// </summary>
        public bool DetailedExceptionMessages { get; set; }

        /// <summary>
        /// A function that will transform an unhandled exception in API controllers
        /// into a custom ApiResult object that will be sent to the client.
        /// Return null to use the default response handling.
        /// </summary>
        public Func<ActionExecutedContext, ApiResult?>? ExceptionResponseFactory { get; set; }

        private bool? _migrationErrors;
        /// <summary>
        /// Determines whether detailed error messages about EF model/migration errors are returned in error responses.
        /// Requires <see cref="DetailedExceptionMessages"/> to be enabled, and defaults to that value.
        /// </summary>
        public bool DetailedEfMigrationExceptionMessages
        {
            get => DetailedExceptionMessages ? (_migrationErrors ?? DetailedExceptionMessages) : false;
            set => _migrationErrors = value;
        }

        [Obsolete("Renamed to DetailedEFMigrationExceptionMessages")]
        public bool DetailedEntityFrameworkExceptionMessages
        {
            get => DetailedEfMigrationExceptionMessages;
            set => DetailedEfMigrationExceptionMessages = value;
        }

        /// <summary>
        /// If true, Coalesce will transform some database exceptions into user-friendly messages when these exceptions occur in Save and Delete operations through <see cref="StandardBehaviors{T}"/>.
        /// For SQL Server, this includes foreign key constraint violations and unique index violations.
        /// These messages respect the security configuration of your models. These messages only serve as a fallback to produce a more acceptable user experience in cases where the developer neglects to add appropriate validation or other handling of related entities.
        /// </summary>
        public bool DetailedEfConstraintExceptionMessages { get; set; } = true;

        /// <summary>
        /// If true, Coalesce will perform validation of incoming data using <see cref="ValidationAttribute"/>s
        /// present on your models during save operations (in <see cref="StandardBehaviors{T}.ValidateDto(SaveKind, IParameterDto{T})"/>).
        /// This can be overridden on individual Behaviors instances by setting <see cref="StandardBehaviors{T}.ValidateAttributesForSaves"/>.
        /// </summary>
        public bool ValidateAttributesForSaves { get; set; } = true;

        /// <summary>
        /// If true, Coalesce will perform validation of incoming parameters using <see cref="ValidationAttribute"/>s
        /// present on your parameters and for custom methods.
        /// This can be overridden on individual custom methods using <see cref="ExecuteAttribute.ValidateAttributes"/>.
        /// </summary>
        public bool ValidateAttributesForMethods { get; set; } = true;
    }
}
