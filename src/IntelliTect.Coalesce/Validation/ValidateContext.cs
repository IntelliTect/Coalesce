using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IntelliTect.Coalesce.TypeDefinition;
using IntelliTect.Coalesce.DataAnnotations;
using IntelliTect.Coalesce.Api.DataSources;

namespace IntelliTect.Coalesce.Validation
{
    internal static class ValidateContext
    {
        public static ValidationHelper Validate(ReflectionRepository repository)
        {
            var assert = new ValidationHelper();

            assert.IsTrue(repository.DiscoveredClassViewModels.Any(), "No types were discovered. Make sure all models have a DbSet on the context.");
            
            foreach (var model in repository.Entities)
            {
                assert.Area = model.ToString();
                assert.IsTrue(!string.IsNullOrWhiteSpace(model.Name), $"Name not found.");
                assert.IsNotNull(model.PrimaryKey, $"Primary key not found for {model}. Primary key should be named 'Id', '{model.Name}Id' or have the [Key] attribute.");
                if (model.PrimaryKey != null)
                {
                    assert.IsTrue(model.PrimaryKey.IsClientProperty, "Model primary keys must be exposed to the client.");
                }

                // Check object references to see if they all have keys and remote keys
                foreach (var prop in model.ClientProperties)
                {
                    assert.Area = $"{model}.{prop}";
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

                        if (prop.IsPOCO)
                        {
                            assert.IsNotNull(prop.Object.ListTextProperty, "The target object for the property has no discernable display text. Add a [ListTextAttribute] to one of its properties.");
                            if (!prop.IsReadOnly && !prop.HasNotMapped && prop.Object.HasDbSet)
                            {
                                // Validate navigation properties

                                assert.IsNotNull(prop.ObjectIdPropertyName, "No ID Property found for related object. Related object needs a foreign key that matches by name or is marked with the [ForeignKey] attribute.");
                                if (!prop.Object.IsOneToOne)
                                {
                                    assert.IsNotNull(prop.ObjectIdProperty, "Has no ID Property - Add a ForeignKey attribute to the object");
                                }
                                assert.IsNotNull(prop.Object.PrimaryKey, "No Primary key for related object. Ensure the target object has a [Key] attributed property.");
                            }
                        }
                        if (prop.IsId && !prop.IsPrimaryKey)
                        {
                            assert.IsNotNull(prop.IdPropertyObjectProperty, "Object property not found.");
                            assert.IsNotNull(prop.IdPropertyObjectProperty.Object, "Object property related object not found.");
                            assert.IsNotNull(prop.IdPropertyObjectProperty.Object.PrimaryKey, "Object Property Object primary key is missing.");
                        }
                        if (prop.Type.IsCollection)
                        {
                            assert.AreNotEqual(prop.Type.FullyQualifiedName, prop.PureType.FullyQualifiedName, "Collection is not defined correctly.");
                            if (!prop.IsReadOnly && !prop.HasNotMapped && prop.PureTypeOnContext)
                            {
                                assert.IsNotNull(prop.InverseProperty, $"A Inverse Property named '{prop.Parent.Name}' was not found on {prop.Object}. " +
                                    $"Add an InverseProperty attribute on {prop.Parent.Name}.{prop.Name} to specify the actual name of the inverse property.", isWarning: true);
                            }
                        }
                        if (prop.IsManytoManyCollection)
                        {
                            assert.IsNotNull(prop.ManyToManyCollectionName, $"Many to Many collection name does not exist");
                            assert.IsNotNull(prop.ManyToManyCollectionProperty.Object.ViewModelClassName, $"Many to Many contained type is: {prop.ManyToManyCollectionProperty.Object.ViewModelClassName}");
                        }
                    }
                    catch (Exception ex)
                    {
                        assert.IsTrue(false, $"Exception property validation. {ex.Message}");
                    }
                }

                // Validate Methods
                foreach (var method in model.ClientMethods)
                {
                    assert.Area = $"{model}.{method}";

                    foreach (var param in method.Parameters)
                    {
                        assert.Area = $"{model}.{method}: {param}";

                        if (method.IsModelInstanceMethod)
                        {
                            var reserved = new[] { "id", "dataSourceFactory" };
                            foreach (var name in reserved)
                            {
                                assert.AreNotEqual("id", param.Name, "id is a reserved parameter name");
                                assert.AreNotEqual("dataSourceFactory", param.Name, "dataSourceFactory is a reserved parameter name");
                            }
                        }
                    }
                }
            }

            // Validate data sources.
            foreach (var model in repository.ApiBackedClasses)
            {
                assert.IsTrue(
                    model.ClientDataSources(repository).Count(s => s.IsDefaultDataSource) <= 1, 
                    $"Cannot have multiple default data sources for {model}");

                foreach (var source in model.ClientDataSources(repository))
                {
                    // Prevent data sources named "Default" that aren't actually default.
                    assert.IsTrue(
                        source.IsDefaultDataSource || !source.ClientTypeName.Equals(DataSourceFactory.DefaultSourceName, StringComparison.InvariantCultureIgnoreCase),
                        $"Data sources can't be named {DataSourceFactory.DefaultSourceName} unless they're marked with {nameof(DefaultDataSourceAttribute)}"
                    );
                }

            }

            // Validate the non-DbSet items (DTOs)
            foreach (var model in repository.CustomDtos)
            {
                assert.Area = $"DTO: {model}";

                // Make sure the key matches the object
                assert.IsTrue(model.DtoBaseViewModel != null, $"Cannot find base model for DTO {model}. Add the base model as a DbSet to the context.");
                if (model.DtoBaseViewModel != null)
                {
                    assert.IsTrue(model.PrimaryKey != null, $"Cannot find primary key for DTO {model}. It must be the [name]Id, [base model]Id or marked with the [Key] attribute. ");
                }
            }

            // Validate the objects found that is not on the context. 
            foreach (var model in repository.ExternalTypes)
            {
                assert.Area = $"External Model: {model}";

                // Make sure these don't inherit from IClassDto because they should have an IEnumerable on the Context.
                assert.IsTrue(model.DtoBaseViewModel == null, $"{model.FullyQualifiedName} appears to be a DTO but isn't marked with [Coalesce].");
            }

            return assert;

        }
    }
}
