using IntelliTect.Coalesce.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.Json.Serialization;

namespace IntelliTect.Coalesce.Api.Controllers;

/// <summary>
/// A modified implementation of the default <see cref="PreserveReferenceResolver"/> that
/// avoids putting IDs/refs on items that will never be duplicated (the root response, and collections).
/// </summary>
internal sealed class CoalesceJsonReferenceHandler : ReferenceHandler
{
    public override ReferenceResolver CreateResolver() => new PreserveReferenceResolver();

    /// <summary>
    /// From https://source.dot.net/#System.Text.Json/System/Text/Json/Serialization/PreserveReferenceResolver.cs
    /// Same as the normal reference handler, but doesn't make refs for the root <see cref="ApiResult"/>,
    /// nor for any <see cref="ICollection"/>s because collections will never be duplicated in Coalesce responses.
    /// </summary>
    internal sealed class PreserveReferenceResolver : ReferenceResolver
    {
        private uint _referenceCount;
        private readonly Dictionary<object, string>? _objectToReferenceIdMap;

        public PreserveReferenceResolver()
        {
            _objectToReferenceIdMap = new Dictionary<object, string>(ReferenceEqualityComparer.Instance);
        }

        public override void AddReference(string referenceId, object value)
            => throw new NotSupportedException();

        public override string GetReference(object value, out bool alreadyExists)
        {
            Debug.Assert(_objectToReferenceIdMap != null);
            if (value is ApiResult || value is ICollection)
            {
                // Don't produce refs for the root response object,
                // nor for collections (collections will never be duplicated)
                alreadyExists = false;
                return null!;
            }

            if (_objectToReferenceIdMap.TryGetValue(value, out string? referenceId))
            {
                alreadyExists = true;
            }
            else
            {
                _referenceCount++;
                referenceId = _referenceCount.ToString();
                _objectToReferenceIdMap.Add(value, referenceId);
                alreadyExists = false;
            }

            return referenceId;
        }

        public override object ResolveReference(string referenceId)
            => throw new NotSupportedException();
    }
}