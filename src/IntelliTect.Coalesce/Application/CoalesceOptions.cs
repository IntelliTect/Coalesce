﻿using IntelliTect.Coalesce.DataAnnotations;
using IntelliTect.Coalesce.Models;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
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

        /// <summary>
        /// If true, Coalesce will perform validation of incoming data using <see cref="ValidationAttribute"/>s
        /// present on your models during save operations (in <see cref="StandardBehaviors{T}.ValidateDto(SaveKind, IClassDto{T})"/>).
        /// This can be overridden on individual Behaviors instances with a prop of the same name.
        /// </summary>
        public bool ValidateAttributesForSaves { get; set; }

        /// <summary>
        /// If true, Coalesce will perform validation of incoming parameters using <see cref="ValidationAttribute"/>s
        /// present on your parameters and for custom methods.
        /// This can be overridden on individual methods using <see cref="ExecuteAttribute.ValidateAttributes"/>.
        /// </summary>
        public bool ValidateAttributesForMethods { get; set; }
    }
}
