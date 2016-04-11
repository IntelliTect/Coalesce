using Intellitect.ComponentModel.TypeDefinition;
using Microsoft.Data.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Intellitect.ComponentModel.Validation
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
            assert.IsTrue(models.Count > 0, "No models were created");
            // Make sure everyone has an id property
            foreach (var model in models.Where(f=>f.HasDbSet))
            {
                assert.Area = model.Name;
                assert.IsTrue(!string.IsNullOrWhiteSpace(model.Name), $"Has name");
                assert.IsNotNull(model.PrimaryKey, "Has primary key");
                assert.IsTrue(model.SearchProperties.Any(), "Has search properties");
                assert.IsNotNull(model.ListTextProperty, "Has list text");
                assert.IsTrue(model.DefaultOrderBy.Any(), "Has order by");
                // Check object references to see if they all have keys and remote keys
                foreach (var prop in model.Properties.Where(f => !f.IsInternalUse))
                {
                    assert.Area = $"{model.Name}: {prop.Name}";
                    try
                    {
                        assert.IsNotNull(prop.JsVariable, $"JS Variable is: {prop.JsVariable}");
                        assert.IsNotNull(prop.Type.TsKnockoutType, $"TS Knockout Type is: {prop.Type.TsKnockoutType}");
                        assert.IsNotNull(prop.Type.JsKnockoutType, $"JS Knockout Type is: {prop.Type.JsKnockoutType}");
                        if (prop.IsPOCO && !prop.IsComplexType)
                        {
                            assert.IsNotNull(prop.Object, "Has target object");
                            assert.IsNotNull(prop.ObjectIdPropertyName, "Has ID Property Name");
                            if (!prop.Object.IsOneToOne)
                            {
                                assert.IsNotNull(prop.ObjectIdProperty, "Has no ID Property - Add a ForiegnKey attribute to the object");
                            }
                            assert.IsNotNull(prop.Object.PrimaryKey, "Has Primary key for related object");
                        }
                        if (prop.IsId && !prop.IsPrimaryKey)
                        {
                            assert.IsNotNull(prop.IdPropertyObjectProperty, "Has Object property");
                            assert.IsNotNull(prop.IdPropertyObjectProperty.Object, "Has Object property object");
                            assert.IsNotNull(prop.IdPropertyObjectProperty.Object.PrimaryKey, "Has object Property Object primary key");
                        }
                        if (prop.Type.IsCollection)
                        {
                            assert.AreNotEqual(prop.TypeName, prop.PureType, "Has correct collection type");
                        }
                        if (prop.IsManytoManyCollection)
                        {
                            assert.IsNotNull(prop.ManyToManyCollectionName, $"Many to Many collection name is: {prop.ManyToManyCollectionName}");
                            assert.IsNotNull(prop.ManyToManyCollectionProperty.Object.ViewModelClassName, $"Many to Many contained type is: {prop.ManyToManyCollectionProperty.Object.ViewModelClassName}");
                        }
                        if (prop.Type.IsEnum)
                        {
                            assert.IsNotNull(prop.JsVariable, $"Enum JS Text Variable is: {prop.JsTextPropertyName}");
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
                foreach (var method in model.Methods.Where(f=>!f.IsInternalUse))
                {
                    assert.Area = $"{model.Name}: {method.Name}";
                    try
                    {
                        assert.IsNotNull(method.JsVariable, $"JS Variable is: {method.JsVariable}");
                        assert.IsNotNull(method.ReturnType.TsKnockoutType, $"TS Knockout Type is: {method.ReturnType.TsKnockoutType}");
                        assert.IsNotNull(method.ReturnType.JsKnockoutType, $"JS Knockout Type is: {method.ReturnType.JsKnockoutType}");

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
                            else
                            {
                                assert.isFalse(param.IsDI, "No DI");
                            }
                        }

                    }
                    catch (Exception ex)
                    {
                        assert.IsTrue(false, $"Exception method validation. {ex.Message}");
                    }

                }

            }
            return assert;


        }
    }
}
