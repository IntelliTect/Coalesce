using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace IntelliTect.Coalesce
{
    /// <summary>
    /// Marker interface for the behaviors that will be used when handling entity types for which no custom behaviors are resolved.
    /// </summary>
    /// <typeparam name="T">The entity type handled.</typeparam>
    /// <typeparam name="TContext">The context that serves the entity.</typeparam>
    public interface IEntityFrameworkBehaviors<T, TContext> : IBehaviors<T>
        where T : class, new()
        where TContext : DbContext
    {
    }
}
