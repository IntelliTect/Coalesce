﻿using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace IntelliTect.Coalesce
{

    public class StandardBehaviors<T, TContext> : StandardBehaviors<T>, IEntityFrameworkBehaviors<T, TContext>
        where T : class, new()
        where TContext : DbContext
    {
        /// <summary>
        /// Contains contextual information about the request.
        /// </summary>
        public new CrudContext<TContext> Context => (CrudContext<TContext>)base.Context;

        /// <summary>
        /// The DbContext from the <see cref="CrudContext{TContext}"/> for the behaviors.
        /// </summary>
        public TContext Db => Context.DbContext;

        public StandardBehaviors(CrudContext<TContext> context) : base(context)
        {
            ClassViewModel = Context.ReflectionRepository.GetClassViewModel<T>()
                ?? throw new ArgumentException("Generic type T has no ClassViewModel.", nameof(T));

            // Ensure that the DbContext is in the ReflectionRepository.
            // We do this so that unit tests will work without having to always do this manually.
            // Cost is very low.
            Context.ReflectionRepository.GetOrAddType(typeof(TContext));
        }

        /// <summary>
        /// Get a DbSet representing the type handled by these behaviors.
        /// </summary>
        protected virtual DbSet<T> GetDbSet() => Db.Set<T>();

        #region Save

        protected override void MapIncomingDto<TDto>(SaveKind kind, T item, TDto dto, IDataSourceParameters parameters)
        {
            if (kind == SaveKind.Update)
            {
                // Ensure that the entity is tracked.
                // We want to allow for item retrieval from data sources that build their query with .AsNoTracking().
                // This is done before mapping so that EF can get the original state of the item.
                var entry = Db.Entry(item);
                if (entry.State == EntityState.Detached)
                {
                    entry.State = EntityState.Unchanged;
                }
            }

            base.MapIncomingDto(kind, item, dto, parameters);
        }

        /// <summary>
        /// Executes the save action against the database and saves the change.
        /// This may be overridden to change what action is actually performed against the database 
        /// on save of an item (e.g. setting a deleted flag instead of deleting the row).
        /// </summary>
        /// <param name="kind">Descriminator between a create and a update operation.</param>
        /// <param name="oldItem">A shallow copy of the original item as it was retrieved from the database.
        /// If kind == SaveKind.Create, this will be null.</param>
        /// <param name="item">An entity instance with its properties set to incoming, new values.</param>
        public override Task ExecuteSaveAsync(SaveKind kind, T? oldItem, T item)
        {
            if (kind == SaveKind.Create)
            {
                GetDbSet().Add(item);
            }

            return Db.SaveChangesAsync();
        }

        #endregion

        #region Delete

        public override Task ExecuteDeleteAsync(T item)
        {
            GetDbSet().Remove(item);
            return Db.SaveChangesAsync();
        }

        #endregion

    }
}
