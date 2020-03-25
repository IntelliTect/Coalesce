﻿using IntelliTect.Coalesce.TypeDefinition;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;

namespace IntelliTect.Coalesce.Api
{
    public abstract class StandardCrudStrategy<T, TContext>
        where T : class, new()
        where TContext : DbContext
    {
        public StandardCrudStrategy(CrudContext<TContext> context)
        {
            Context = context ?? throw new ArgumentNullException(nameof(context));
            ClassViewModel = Context.ReflectionRepository.GetClassViewModel<T>();

            // Ensure that the DbContext is in the ReflectionRepository.
            // We do this so that unit tests will work without having to always do this manually.
            // Cost is very low.
            Context.ReflectionRepository.GetOrAddType(typeof(TContext));
        }

        /// <summary>
        /// Contains contextual information about the request.
        /// </summary>
        public CrudContext<TContext> Context { get; }

        /// <summary>
        /// The DbContext to be used for this request.
        /// </summary>
        public TContext Db => Context.DbContext;

        /// <summary>
        /// The user making the request.
        /// </summary>
        public ClaimsPrincipal User => Context.User;

        /// <summary>
        /// A ClassViewModel representing the type T that is handled by these strategies.
        /// </summary>
        public ClassViewModel ClassViewModel { get; protected set; }
    }
}
