# [ClientValidation]

The `[IntelliTect.Coalesce.DataAnnotations.ClientValidation]`
attribute is used to control the behavior of client-side model validation
and to add additional client-only validation parameters. Database validation is available via standard `System.ComponentModel.DataAnnotations` annotations. 

These propagate to the client as validations in TypeScript via generated [Metadata](/stacks/vue/layers/metadata.md) and [ViewModel rules](/stacks/vue/layers/viewmodels.md) (for Vue) or [Knockout-Validation](https://github.com/Knockout-Contrib/Knockout-Validation/) rules (for Knockout). For both stacks, any failing validation rules prevent saves from going to the server. 

::: warning
This attribute controls client-side validation only. To perform server-side validation, create a custom [Behaviors class](/modeling/model-components/behaviors.md) for your types.
:::

[[toc]]
    

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

### Behavioral Properties

<Prop def="public bool AllowSave { get; set; } // Knockout Only" />

If set to `true`, any client validation errors on the property will not prevent saving on the client. This includes **all** client-side validation, including null-checking for required foreign keys and other validations that are implicit. This also includes other explicit validation from `System.ComponentModel.DataAnnotations` annotations.
    
Instead, validation errors will be treated only as warnings, and will be available through the `warnings: KnockoutValidationErrors` property on the TypeScript ViewModel.

::: tip Note
Use `AllowSave = true` to allow partially complete data to still be saved, protecting your user from data loss upon navigation while still hinting to them that they are not done filling out data.
:::


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
<template #knockout>

The following attribute properties all map directly to [Knockout-Validation](https://github.com/Knockout-Contrib/Knockout-Validation/) properties.

``` c#
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
```

The following attribute properties are outputted to TypeScript unquoted. If you need to assert equality to a string, wrap the value you set to this property in quotes. Other literals (numerics, booleans, etc) need no wrapping.

``` c#
public string Equal { get; set; }
public string NotEqual { get; set; }
```

The following two properties may be used together to specify a custom [Knockout-Validation](https://github.com/Knockout-Contrib/Knockout-Validation/) property.

It will be emitted into the TypeScript as `this.extend({ CustomName: CustomValue })`. Neither value will be quoted in the emitted TypeScript - add quotes to your value as needed to generate valid TypeScript.

``` c#
public string CustomName { get; set; }
public string CustomValue { get; set; }
```

</template>
</CodeTabs>
