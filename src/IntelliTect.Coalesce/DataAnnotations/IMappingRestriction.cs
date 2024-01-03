using Microsoft.EntityFrameworkCore;

namespace IntelliTect.Coalesce.DataAnnotations
{
    /// <summary>
    /// Implements restrictions that are declared on properties using <see cref="RestrictAttribute{T}"/>.
    /// These restrictions will be checked each time such a property is mapped to or from a generated <see cref="IClassDto{T}"/>,
    /// allowing for per-row, per-column security restrictions to be implemented.
    /// </summary>
    public interface IPropertyRestriction
    {
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
        bool UserCanWrite(IMappingContext context, string propertyName, object model, object? incomingValue);


        bool UserCanFilter(IMappingContext context, string propertyName) => false;
    }

    public interface IPropertyRestriction<TModel>
        where TModel : class
    {
        /// <summary>
        /// Restricts whether values of a property will be mapped from a model instance to its DTO.
        /// </summary>
        bool UserCanRead(IMappingContext context, string propertyName, TModel model);

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
        bool UserCanWrite(IMappingContext context, string propertyName, TModel? model, object? incomingValue);

        bool UserCanFilter(IMappingContext context, string propertyName) => false;
    }
}
