using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IntelliTect.Coalesce.TypeDefinition;

namespace IntelliTect.Coalesce.Validation
{
    public static class ValidateContext
    {

        public static ValidationHelper Validate<T>() where T : DbContext
        {
            var models = ReflectionRepository.AddContext<T>();

            return Validate(models);
        }

        public static ValidationHelper Validate(List<ClassViewModel> models)
        {
            var assert = new ValidationHelper();
            assert.IsTrue(models.Count > 0, "No models were created. Make sure all models have a DbSet on the context.");
            // Make sure everyone has an id property
            foreach (var model in models.Where(f => f.HasDbSet))
            {
                assert.Area = model.Name;
                assert.IsTrue(!string.IsNullOrWhiteSpace(model.Name), $"Name not found.");
                assert.IsNotNull(model.PrimaryKey, $"Primary key not found for {model.Name}. Primary key should be named {model.Name}Id or have the [Key] attribute.");
                assert.IsTrue(model.SearchProperties().Any(), $"No searchable properties found for {model.Name}. Annotate a property with [Search].");
                assert.IsNotNull(model.ListTextProperty, $"No default text for dropdown lists found for {model.Name}. Add a Name property or use the [ListText] annotation on the property to be used.");
                assert.IsTrue(model.DefaultOrderBy.Any(), $"No default order found for {model.Name}. Use the [DefaultOrderBy] annotation.");
                // Check object references to see if they all have keys and remote keys
                foreach (var prop in model.Properties.Where(f => !f.IsInternalUse))
                {
                    assert.Area = $"{model.Name}: {prop.Name}";
                    try
                    {
                        assert.IsNull(prop.Wrapper.GetAttributeValue<DataAnnotations.ReadAttribute>(nameof(DataAnnotations.ReadAttribute.PermissionLevel)),
                            "Property-level ReadAttribute security doesn't support the PermissionLevel property");
                        assert.IsNull(prop.Wrapper.GetAttributeValue<DataAnnotations.EditAttribute>(nameof(DataAnnotations.ReadAttribute.PermissionLevel)),
                            "Property-level EditAttribute security doesn't support the PermissionLevel property");
                        assert.isFalse(prop.Wrapper.HasAttribute<DataAnnotations.CreateAttribute>(),
                            "Property-level security doesn't support CreateAttribute");
                        assert.isFalse(prop.Wrapper.HasAttribute<DataAnnotations.DeleteAttribute>(),
                            "Property-level security doesn't support DeleteAttribute");

                        if (prop.IsPOCO && !prop.IsComplexType)
                        {
                            assert.IsNotNull(prop.Object, "The target object for the property was not found. Make sure naming is consistent.");
                            assert.IsNotNull(prop.Object.ListTextProperty, "The target object for the property has no discernable list text. Add a [ListTextAttribute] to one of its properties.");
                            if (!prop.IsReadOnly && !prop.HasNotMapped)
                            {
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
                            assert.AreNotEqual(prop.TypeName, prop.PureType, "Collection is not defined correctly.");
                        }
                        if (prop.IsManytoManyCollection)
                        {
                            assert.IsNotNull(prop.ManyToManyCollectionName, $"Many to Many collection name does not exist");
                            assert.IsNotNull(prop.ManyToManyCollectionProperty.Object.ViewModelClassName, $"Many to Many contained type is: {prop.ManyToManyCollectionProperty.Object.ViewModelClassName}");
                        }
                        // See if we are using an invalid name
                        assert.AreNotEqual("active", prop.Name.ToLower(), "Property name cannot be 'Active' because it conflicts with standard API parameters");
                    }
                    catch (Exception ex)
                    {
                        assert.IsTrue(false, $"Exception property validation. {ex.Message}");
                    }
                }

                // Validate Methods
                foreach (var method in model.Methods.Where(f => !f.IsInternalUse))
                {
                    assert.Area = $"{model.Name}: {method.Name}";
                    try
                    {
                        assert.IsNotNull(method.JsVariable, $"JS Variable is: {method.JsVariable}");

                        // Check di
                        foreach (var param in method.Parameters)
                        {
                            assert.Area = $"{model.Name}: {method.Name}: {param.Name}";
                            assert.IsNotNull(param.Type, "Parameter Type");
                            // These only work if the names are db and user.
                            if (param.Name == "user")
                            {
                                assert.IsTrue(param.IsAUser, "Is a User");
                                assert.IsTrue(param.IsDI, "Is DI");
                            }
                            else if (param.Name == "db")
                            {
                                assert.IsTrue(param.IsAContext, "Is a context");
                                assert.IsTrue(param.IsDI, "Is DI");
                            }
                            else if (param.Name == "includeTree")
                            {
                                assert.IsTrue(param.IsAnIncludeTree, "Is an IncludeTree");
                                assert.IsTrue(param.IsDI, "Is DI");
                            }
                            else
                            {
                                assert.isFalse(param.IsManualDI, $"DI properties must be named either 'db' or 'user' or 'out includeTree'. Got {param.Name}");
                            }
                        }

                    }
                    catch (Exception ex)
                    {
                        assert.IsTrue(false, $"Exception method validation. {ex.Message}");
                    }

                }

            }

            // Validate the non-DbSet items (DTOs)
            foreach (var model in models.Where(f => !f.HasDbSet && f.OnContext))
            {
                assert.Area = $"DTO: {model.Name}";
                // Console.WriteLine($"Validating DTO: {model.Name}");
                // Make sure the key matches the object
                if (model.IsDto)
                {
                    assert.IsTrue(model.DtoBaseViewModel != null, $"Cannot find base model for DTO {model.Name}. Add the base model as a DbSet to the context.");
                    if (model.DtoBaseViewModel != null)
                    {
                        assert.IsTrue(model.PrimaryKey != null, $"Cannot find primary key for DTO {model.Name}. It must be the [name]Id, [base model]Id or marked with the [Key] attribute. ");
                    }
                }else
                {
                    assert.IsTrue(model.DtoBaseViewModel == null, $"External type should not implement IClassDto. IClassDtos should have an IEnumerable<{model.Name}> in the context.");
                }
            }

            // Validate the objects found that is not on the context. 
            foreach (var model in models.Where(f => !f.OnContext))
            {
                assert.Area = $"External Model: {model.Name}";
                //Console.WriteLine($"Validating Other Object: {model.Name}");
                // Make sure these don't inherit from IClassDto because they should have an IEnumerable on the Context.
                assert.IsTrue(model.DtoBaseViewModel == null, $"{model.Name} appears to be a DTO but doesn't have an IEnumerable<{model.Name} entry in the Context. Add to the context or remove IClassDto.");
            }

            return assert;

        }
    }
}
