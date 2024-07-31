# [ClientValidation]

`IntelliTect.Coalesce.DataAnnotations.ClientValidationAttribute`

The `ClientValidation`
attribute is used to control the behavior of client-side model validation
and to add additional client-only validation parameters. Database validation is available via standard `System.ComponentModel.DataAnnotations` annotations. 

These propagate to the client as validations in TypeScript via generated [Metadata](/stacks/vue/layers/metadata.md) and [ViewModel rules](/stacks/vue/layers/viewmodels.md). Any failing validation rules prevent saves from going to the server. 

::: warning
This attribute controls client-side validation only. To perform server-side validation, create a custom [Behaviors class](/modeling/model-components/behaviors.md) for your types and/or place C# validation attributes on your models. [Read More](/topics/security.md#server-side-data-validation).
:::
    

## Example Usage

``` c#
public class Person
{
    public int PersonId { get; set; }

    [ClientValidation(IsRequired = true, AllowSave = true)]
    public string FirstName { get; set; }

    [ClientValidation(IsRequired = true, AllowSave = false, MinLength = 1, MaxLength = 100)]
    public string LastName { get; set; }
}
```

## Properties

<Prop def="public string ErrorMessage { get; set; }" />

Set an error message to be used if any client validations fail

### Validation Rule Properties

<CodeTabs>
<template #vue>

In addition to the following properties, you also customize validation on a per-instance basis of the [ViewModels](/stacks/vue/layers/viewmodels.md#viewmodels) using the [Rules/Validation](/stacks/vue/layers/viewmodels.md#rules-validation) methods.

``` c#
public bool IsRequired { get; set; }
public double MinValue { get; set; } = double.MaxValue;
public double MaxValue { get; set; } = double.MinValue;
public double MinLength { get; set; } = double.MaxValue;
public double MaxLength { get; set; } = double.MinValue;
public string Pattern { get; set; }
public bool IsEmail { get; set; }
public bool IsPhoneUs { get; set; }
```

</template>
</CodeTabs>
