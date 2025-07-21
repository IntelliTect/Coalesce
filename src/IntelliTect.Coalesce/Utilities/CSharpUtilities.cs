﻿using IntelliTect.Coalesce.TypeDefinition;
using System;
using System.Linq;

namespace IntelliTect.Coalesce.Utilities;

public static class CSharpUtilities
{
    /// <summary>
    /// Get a valid compile-time constant expression for the given literal value with the given type.
    /// </summary>
    /// <param name="type">The type of the literal value.</param>
    /// <param name="value">The literal value to represent as a C# constant expression.</param>
    /// <returns></returns>
    public static string GetCSharpLiteral(TypeViewModel type, object? value)
    {
        // We want a ReferenceEquals null check here to avoid poorly-implemented equality operator overloads.
        if (value is null) return "default";

        if (type.IsString) return $"\"{value.ToString().EscapeStringLiteralForCSharp()}\"";

        // https://docs.microsoft.com/en-us/dotnet/standard/base-types/standard-numeric-format-strings#the-round-trip-r-format-specifier
        if (value is Double doubleValue) return doubleValue.ToString("G17");
        if (value is Single floatValue) return floatValue.ToString("G9");

        // This code inspired by http://source.roslyn.io/#Microsoft.CodeAnalysis/SymbolDisplay/AbstractSymbolDisplayVisitor.cs,ee69b51773f9e7d0,references
        if (type.IsEnum)
        {
            // Coalesce assumes all enums are int32 based.
            // Roslyn handles them as uint64 (search for ConvertEnumUnderlyingTypeToUInt64) - perhaps we should too?
            var int64Value = Convert.ToInt64(value);
            // Find an explicit enum value.
            var enumValue = type.EnumValues.FirstOrDefault(e => Convert.ToInt64(e.Value) == int64Value);
            if (enumValue != null)
            {
                // Found a named enum value.
                return $"{type.FullyQualifiedName}.{enumValue.Name}";
            }
            else
            {
                // Can't find a named value. Use a cast instead.
                return $"({type.FullyQualifiedName}){int64Value.ToString()}";
            }
        }
        
        if (type.IsIntegral)
        {
            return value.ToString()!;
        }

        if (type.IsBool)
        {
            return Convert.ToBoolean(value).ToString().ToLower();
        }

        // Require explicit handling for any additional types.
        // This prevents us from accidentaly converting a value incorrectly that we're not aware of how to handle.
        throw new InvalidOperationException($"Coalesce currently doesn't know how to express value {value} of type {type} as a C# literal.");
    }
}
