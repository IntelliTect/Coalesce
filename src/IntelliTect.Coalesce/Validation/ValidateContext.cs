using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IntelliTect.Coalesce.TypeDefinition;
using IntelliTect.Coalesce.DataAnnotations;
using IntelliTect.Coalesce.Api.DataSources;
using IntelliTect.Coalesce.TypeDefinition.Enums;

namespace IntelliTect.Coalesce.Validation
{
    internal static class ValidateContext
    {
        public static ValidationHelper Validate(ReflectionRepository repository)
        {
            var assert = new ValidationHelper();

            assert.IsTrue(repository.DiscoveredClassViewModels.Any(), "No types were discovered. Make sure all models have a DbSet on the context.");

            foreach (var model in repository.CrudApiBackedClasses)
            {
                assert.Area = model.ToString();

                assert.IsTrue(!string.IsNullOrWhiteSpace(model.Name), $"Name not found.");
                assert.IsNotNull(model.PrimaryKey, $"Primary key not found for {model}. Primary key should be named 'Id', '{model.Name}Id' or have the [Key] attribute.");
                if (model.PrimaryKey != null)
                {
                    assert.IsTrue(model.PrimaryKey.IsClientProperty, "Model primary keys must be exposed to the client.");
                }

                var dataSources = model.ClientDataSources(repository).ToList();
                if (model.IsStandaloneEntity)
                {
                    assert.IsTrue(dataSources.Any(), "Standalone entities must declare at least one data source.");
                    if (dataSources.Count > 1)
                    {
                        assert.IsTrue(dataSources.Count(s => s.IsDefaultDataSource) == 1, "Standalone entities that declare multiple data sources must mark exactly one as the [DefaultDataSource]");
                    }
                }
                else
                {
                    assert.IsNotNull(model.DbContext, 
                        "Cannot determine the DbContext that provides this type. " 
                        + (model.IsDto ? "Try using IClassDto<T, TDbContext> for this type instead." : "")
                    );
                }

                assert.IsTrue(
                    model.ClientDataSources(repository).Count(s => s.IsDefaultDataSource) <= 1,
                    $"Cannot have multiple default data sources for {model}");

                // Check object references to see if they all have keys and remote keys
                foreach (var prop in model.ClientProperties)
                {
                    assert.Area = $"{model}.{prop.Name}";
                    try
                    {
                        assert.IsNull(prop.GetAttributeValue<ReadAttribute, SecurityPermissionLevels>(a => a.PermissionLevel),
                            "Property-level ReadAttribute security doesn't support the PermissionLevel property");
                        assert.IsNull(prop.GetAttributeValue<EditAttribute, SecurityPermissionLevels>(a => a.PermissionLevel),
                            "Property-level EditAttribute security doesn't support the PermissionLevel property");
                        assert.IsFalse(prop.HasAttribute<CreateAttribute>(),
                            "Property-level security doesn't support CreateAttribute");
                        assert.IsFalse(prop.HasAttribute<DeleteAttribute>(),
                            "Property-level security doesn't support DeleteAttribute");

                        if (model.IsDto)
                        {
                            const string dtoPropSecWarningPreamble =
                                "Property-level security for an IClassDto must be implemented in MapTo/MapFrom, or in a DataSource or Behaviors. ";

                            assert.IsFalse(prop.HasAttribute<ReadAttribute>(),
                                dtoPropSecWarningPreamble + "ReadAttribute has no effect here.");

                            assert.IsFalse(prop.HasAttribute<EditAttribute>(),
                                dtoPropSecWarningPreamble + "EditAttribute has no effect here.");

                            assert.IsFalse(prop.HasAttribute<DtoIncludesAttribute>(),
                                "[DtoIncludesAttribute] has no effect on an IClassDto. This logic must be implemented manually in MapFrom.");
                            assert.IsFalse(prop.HasAttribute<DtoExcludesAttribute>(),
                                "[DtoExcludesAttribute] has no effect on an IClassDto. This logic must be implemented manually in MapFrom.");
                        }

                        assert.IsFalse(prop.Type.IsFile, "IFile is not supported as a property.");

                        if (prop.IsPOCO)
                        {
                            assert.IsNotNull(prop.Object?.ListTextProperty, "The target object for the property has no discernible display text. Add a [ListTextAttribute] to one of its properties.");
                            if (!prop.IsReadOnly && !prop.HasNotMapped && prop.Object?.HasDbSet == true)
                            {
                                // Validate navigation properties
                                assert.IsNotNull(prop.ForeignKeyProperty, "No ID Property found for related object. Related object needs a foreign key that matches by name or is marked with the [ForeignKey] attribute.");
                                assert.IsNotNull(prop.Object.PrimaryKey, "No Primary key for related object. Ensure the target object has a [Key] attributed property.");
                            }
                        }
                        if (prop.IsForeignKey)
                        {
                            assert.IsNotNull(prop.ReferenceNavigationProperty, "Object property not found.");
                            assert.IsNotNull(prop.ReferenceNavigationProperty?.Object, "Object property related object not found.");
                            assert.IsNotNull(prop.ReferenceNavigationProperty?.Object?.PrimaryKey, "No primary key on type of this ID's Navigation Property.");
                        }
                        if (prop.Type.IsCollection)
                        {
                            assert.AreNotEqual(prop.Type.FullyQualifiedName, prop.PureType.FullyQualifiedName, "Collection is not defined correctly.");
                            if (!prop.IsReadOnly && !prop.HasNotMapped && prop.PureTypeOnContext)
                            {
                                assert.IsNotNull(prop.InverseProperty, $"An Inverse Property named '{prop.Parent.Name}' was not found on {prop.Object}. " +
                                    $"Add an InverseProperty attribute on {prop.Parent.Name}.{prop.Name} to specify the actual name of the inverse property.", isWarning: true);
                            }
                            if (prop.Role == PropertyRole.CollectionNavigation)
                            {
                                assert.IsTrue(prop.InverseProperty!.IsPOCO, "The inverse property of a collection navigation should reference the corresponding reference navigation on the other side of the relationship.");
                                assert.IsNotNull(prop.InverseProperty.ForeignKeyProperty, "Could not find the foreign key of the referenced inverse property");
                            }
                        }
                        if (prop.IsManytoManyCollection &&
                            assert.IsNotNull(prop.ManyToManyCollectionName, $"Many to Many collection name does not exist") &&
                            assert.IsTrue(prop.PureTypeOnContext, $"ManyToManyAttribute cannot be used on {prop.PureType} because the type is not DB-mapped.")
                        )
                        {
                            var nearNavigation = prop.ManyToManyNearNavigationProperty;
                            if (assert.IsNotNull(nearNavigation, $"Many to Many near navigation prop could not be determined. Consider adding an [InverseProperty] as well."))
                            {
                                assert.IsNotNull(nearNavigation.ForeignKeyProperty, $"Many-to-many property's near-side navigation property ({nearNavigation}) has no corresponding foreign key property.");
                            }

                            var farNavigation = prop.ManyToManyFarNavigationProperty;
                            if (assert.IsNotNull(farNavigation, $"Many to Many far navigation prop could not be determined. "))
                            {
                                assert.IsNotNull(farNavigation.ForeignKeyProperty, $"Many-to-many property's far-side navigation property ({farNavigation}) has no corresponding foreign key property.");
                            }
                        }

                        if (prop.DefaultOrderBy != null && model.IsDbMappedType)
                        {
                            assert.IsTrue(prop.IsDbMapped, "Property with [DefaultOrderBy] must be DB mapped.");
                        }
                    }
                    catch (Exception ex)
                    {
                        assert.IsTrue(false, $"Exception property validation. {ex.Message}");
                    }
                }

                foreach (var source in dataSources)
                {
                    assert.Area = source.ToString();
                    // Prevent data sources named "Default" that aren't actually default.

                    // Furthermore, prevent non-default data sources whose name contains "Default",
                    // as it suggests an intent that won't actually be fulfilled
                    // and therefore might be resulting in a security hole if a "default"-named
                    // data source is implementing security rules but is in fact not
                    // being enforced as a default.
                    assert.IsTrue(
                        source.IsDefaultDataSource 
                        || (model.IsStandaloneEntity && dataSources.Count() == 1) // A lone datasource for a standalone entity is also treated as the default.
                        || source.ClientTypeName.IndexOf(DataSourceFactory.DefaultSourceName, StringComparison.InvariantCultureIgnoreCase) == -1,
                        $"Data sources can't contain {DataSourceFactory.DefaultSourceName} in their name unless they're marked with {nameof(DefaultDataSourceAttribute)}"
                    );
                }
            }

            foreach (var model in repository.ControllerBackedClasses)
            {
                foreach (var method in model.Methods)
                {
                    assert.Area = $"{model}.{method.ToStringWithoutReturn()}";

                    if (method.IsClientMethod)
                    {
                        if (method.Name != method.NameWithoutAsync)
                        {
                            // If the name and name w/o async are different, this method has "Async" at the end.
                            // Assert that there isn't also a method that exists without "Async" at the end,
                            // as this will cause name conflicts caused by the fact that we drop "Async" from method names.
                            assert.IsNull(model.MethodByName(method.NameWithoutAsync), 
                                "Do not expose both an async and non-async version of the same method. Prefer the async version.");
                        }

                        assert.IsFalse(
                            method.GetAttributeValue<ControllerActionAttribute>(a => a.VaryByProperty) != null && 
                            method.VaryByProperty == null,
                            $"{nameof(ControllerActionAttribute.VaryByProperty)} is only applicable to HTTP GET model instance methods, " +
                            $"and must reference a property on the parent instance."
                        );

                        assert.IsFalse(method.ResultType.IsInternalUse, "Method return types cannot be internal use.");

                        // TODO: Assert that the method name isn't a reserved endpoint name:
                        // get, save, delete, list, count
                        foreach (var param in method.Parameters)
                        {
                            assert.Area = $"{model}: {method}: {param}";

                            if (!param.IsDI)
                            {
                                assert.IsFalse(param.Type.IsInternalUse, "Method parameters cannot be internal use.");
                            }

                            if (method.IsModelInstanceMethod)
                            {
                                var reserved = new List<string> {
                                    "id",
                                    "dataSourceFactory"
                                };
                                if (method.VaryByProperty?.IsClientProperty == true) reserved.Add("etag");
                                foreach (var name in reserved)
                                {
                                    assert.AreNotEqual(name, param.Name, $"{name} is a reserved parameter name");
                                }
                            }
                        }
                    }
                    else
                    {
                        assert.IsFalse(
                            method.HasAttribute<ExecuteAttribute>(),
                            "Non-exposed method has the [Execute] attribute - did you forget to add [Coalesce]?",
                            isWarning: true);

                        assert.IsFalse(
                            method.HasAttribute<ControllerActionAttribute>(),
                            "Non-exposed method has the [ControllerAction] attribute - did you forget to add [Coalesce]?",
                            isWarning: true);
                    }
                }
            }

            // Validate the objects found that is not on the context. 
            foreach (var model in repository.ExternalTypes)
            {
                assert.Area = $"External Model: {model}";

                // Make sure these don't inherit from IClassDto because if they do, the [Coalesce] attribute was probably missed.
                assert.IsTrue(model.DtoBaseViewModel == null, $"{model.FullyQualifiedName} appears to be a DTO but isn't marked with [Coalesce].");
            }

            foreach (var model in repository.Services)
            {
                assert.Area = $"Service: {model}";

                assert.IsFalse(model.IsStatic, $"Service classes cannot be static.");
            }

            return assert;

        }
    }
}
