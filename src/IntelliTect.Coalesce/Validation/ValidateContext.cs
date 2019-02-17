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
            
            foreach (var model in repository.ApiBackedClasses)
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
                                assert.IsNotNull(prop.ForeignKeyProperty, "No ID Property found for related object. Related object needs a foreign key that matches by name or is marked with the [ForeignKey] attribute.");
                                assert.IsNotNull(prop.Object.PrimaryKey, "No Primary key for related object. Ensure the target object has a [Key] attributed property.");
                            }
                        }
                        if (prop.IsForeignKey)
                        {
                            assert.IsNotNull(prop.ReferenceNavigationProperty, "Object property not found.");
                            assert.IsNotNull(prop.ReferenceNavigationProperty.Object, "Object property related object not found.");
                            assert.IsNotNull(prop.ReferenceNavigationProperty.Object.PrimaryKey, "No primary key on type of this ID's Navigation Property.");
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
                        if (prop.IsFile)
                        {
                            if (prop.HasFileFilenameProperty) assert.IsNotNull(prop.FileFilenameProperty, $"Cannot find filename property: {prop.Parent.Name}.{prop.FileFilenameProperty} for {prop.Name}");
                            if (prop.HasFileHashProperty) assert.IsNotNull(prop.FileHashProperty, $"Cannot find file hash property: {prop.Parent.Name}.{prop.FileHashProperty} for {prop.Name}");
                            if (prop.HasFileSizeProperty) assert.IsNotNull(prop.FileSizeProperty, $"Cannot find file size property: {prop.Parent.Name}.{prop.FileSizeProperty} for {prop.Name}");
                        }
                    }
                    catch (Exception ex)
                    {
                        assert.IsTrue(false, $"Exception property validation. {ex.Message}");
                    }
                }

                foreach (var method in model.Methods)
                {
                    assert.Area = $"{model}: {method}";

                    if (method.IsClientMethod)
                    {
                        foreach (var param in method.Parameters)
                        {
                            assert.Area = $"{model}: {method}: {param}";

                            if (method.IsModelInstanceMethod)
                            {
                                var reserved = new[] {
                                    "id",
                                    "dataSourceFactory"
                                };
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

                // Make sure these don't inherit from IClassDto because if they do, the [Coalesce] attribute was probably missed.
                assert.IsTrue(model.DtoBaseViewModel == null, $"{model.FullyQualifiedName} appears to be a DTO but isn't marked with [Coalesce].");
            }

            return assert;

        }
    }
}
