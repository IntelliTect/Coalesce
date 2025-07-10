using Microsoft.EntityFrameworkCore;

namespace IntelliTect.Coalesce.DataAnnotations
{
    /// <summary>
    /// Implements restrictions that are declared on properties using <see cref="RestrictAttribute{T}"/>.
    /// These restrictions will be checked each time such a property is mapped to or from a generated DTO,
    /// allowing for per-row, per-column security restrictions to be implemented.
    /// </summary>
    public interface IPropertyRestriction
    {
        /// <summary>
        /// <para>
        /// Restricts whether the user can sort, search, or filter the values 
        /// of a property. By default, this always returns <see langword="false"/> 
        /// and can be overridden with a nuanced implementation if it can be determined for a 
        /// particular user if these actions are allowed.
        /// </para>
        /// <para>
        /// Improper permissions implemented here can result in information disclosure vulnerabilities.
        /// In general, filtering should only be allowed by users for whom 
        /// <see cref="UserCanRead(IMappingContext, string, object)"/> will always return <see langword="true"/>,
        /// regardless of the particular `model` instance.
        /// </para>
        /// </summary>
        bool UserCanFilter(IMappingContext context, string propertyName) => false;

        /// <summary>
        /// Restricts whether values of a property will be mapped from a model instance to its DTO.
        /// </summary>
        bool UserCanRead(IMappingContext context, string propertyName, object model);

        /// <summary>
        /// <para>
        /// Restricts whether values of a property will be mapped from a DTO to a model instance.
        /// </para>
        /// <para>
        /// The <paramref name="model"/> parameter may be null if mapping to a new model instance
        /// where property initializers must be used - for example, when
        /// a <see langword="required"/> or <see langword="init"/> property is used.
        /// </para>
        /// <para>
        /// The <paramref name="model"/> parameter may also already contain some user input
        /// data from other properties that have already been mapped. If your result is predicated
        /// on user-modifiable properties, consider injecting a <see cref="DbContext"/> and reading the
        /// original, fresh values from the database.
        /// </para>
        /// </summary>
        bool UserCanWrite(IMappingContext context, string propertyName, object? model, object? incomingValue);
    }

    /// <summary>
    /// Generic version of <see cref="IPropertyRestriction"/> that provides strongly-typed
    /// method overloads for a specific model type.
    /// </summary>
    /// <typeparam name="TModel">The model type that this restriction applies to.</typeparam>
    public interface IPropertyRestriction<TModel> : IPropertyRestriction
        where TModel : class
    {
        /// <inheritdoc cref="IPropertyRestriction.UserCanRead(IMappingContext, string, object)"/>
        bool UserCanRead(IMappingContext context, string propertyName, TModel model);

        /// <inheritdoc cref="IPropertyRestriction.UserCanWrite(IMappingContext, string, object, object)"/>
        bool UserCanWrite(IMappingContext context, string propertyName, TModel? model, object? incomingValue);

        bool IPropertyRestriction.UserCanRead(IMappingContext context, string propertyName, object model)
            => this.UserCanRead(context, propertyName, (TModel)model);

        bool IPropertyRestriction.UserCanWrite(IMappingContext context, string propertyName, object? model, object? incomingValue)
            => this.UserCanWrite(context, propertyName, (TModel?)model, incomingValue);
    }
}
