﻿using System;

namespace IntelliTect.Coalesce.DataAnnotations;

/// <summary>
/// Properties marked ReadOnlyApi are not saved.
/// </summary>
[System.AttributeUsage(System.AttributeTargets.Property)]
[Obsolete("Mark a property with the [Read] attribute with no [Edit] attribute to make it read-only")]
public class ReadOnlyApiAttribute : System.Attribute
{
}
