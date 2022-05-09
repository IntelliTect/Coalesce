

.. _Knockout-Validation: https://github.com/Knockout-Contrib/Knockout-Validation/

.. _ClientValidation:

[ClientValidation]
==================

The `[IntelliTect.Coalesce.DataAnnotations.ClientValidation]`
attribute is used to control the behavior of client-side model validation
and to add additional client-only validation parameters. Database validation is available via standard `System.ComponentModel.DataAnnotations` annotations. 

These propagate to the client as validations in TypeScript via generated :ref:`Metadata <VueMetadata>` and :ref:`ViewModel rules <VueViewModelsValidation>` (for Vue) or Knockout-Validation_ rules (for Knockout). For both stacks, any failing validation rules prevent saves from going to the server. 

.. warning::

    This attribute controls client-side validation only. To perform server-side validation, create a custom :ref:`Behaviors` for your types.

.. contents:: Contents
    :local:
    

Example Usage
-------------

.. code-block:: c#
    
    public class Person
    {
        public int PersonId { get; set; }

        [ClientValidation(IsRequired = true, AllowSave = true)]
        public string FirstName { get; set; }

        [ClientValidation(IsRequired = true, AllowSave = false, MinLength = 1, MaxLength = 100)]
        public string LastName { get; set; }
    }


Properties
----------

Behavioral Properties
.....................

`public bool AllowSave { get; set; };` (Knockout Only)
    If set to `true`, any client validation errors on the property will not prevent saving on the client. This includes **all** client-side validation, including null-checking for required foreign keys and other validations that are implicit. This also includes other explicit validation from `System.ComponentModel.DataAnnotations` annotations.
    
    Instead, validation errors will be treated only as warnings, and will be available through the `warnings: KnockoutValidationErrors` property on the TypeScript ViewModel.

    .. tip::

        Use `AllowSave = true` to allow partially complete data to still be saved, protecting your user from data loss upon navigation while still hinting to them that they are not done filling out data.


`public string ErrorMessage { get; set; }`
    Set an error message to be used if any client validations fail

Validation Rule Properties
..........................

.. tabs::

    .. group-tab:: Knockout

        The following attribute properties all map directly to Knockout-Validation_ properties.

        .. code-block:: c#

            public bool IsRequired { get; set; }
            public double MinValue { get; set; } = double.MaxValue;
            public double MaxValue { get; set; } = double.MinValue;
            public double MinLength { get; set; } = double.MaxValue;
            public double MaxLength { get; set; } = double.MinValue;
            public double Step { get; set; }
            public string Pattern { get; set; }
            public bool IsEmail { get; set; }
            public bool IsPhoneUs { get; set; }
            public bool IsDate { get; set; }
            public bool IsDateIso { get; set; }
            public bool IsNumber { get; set; }
            public bool IsDigit { get; set; }

        The following attribute properties are outputted to TypeScript unquoted. If you need to assert equality to a string, wrap the value you set to this property in quotes. Other literals (numerics, bools, etc) need no wrapping.

        .. code-block:: c#

            public string Equal { get; set; }
            public string NotEqual { get; set; }

                    
        The following two properties may be used together to specify a custom Knockout-Validation_ property.

        It will be emitted into the TypeScript as `this.extend({ CustomName: CustomValue })`. Neither value will be quoted in the emitted TypeScript - add quotes to your value as needed to generate valid TypeScript.

        .. code-block:: c#

            public string CustomName { get; set; }
            public string CustomValue { get; set; }

    .. group-tab:: Vue

        In addition to the following properties, you also customize validation on a per-instance basis of the :ref:`VueInstanceViewModels` using the :ref:`VueViewModelsValidation` methods.

        .. code-block:: c#

            public bool IsRequired { get; set; }
            public double MinValue { get; set; } = double.MaxValue;
            public double MaxValue { get; set; } = double.MinValue;
            public double MinLength { get; set; } = double.MaxValue;
            public double MaxLength { get; set; } = double.MinValue;
            public string Pattern { get; set; }
            public bool IsEmail { get; set; }
            public bool IsPhoneUs { get; set; }

