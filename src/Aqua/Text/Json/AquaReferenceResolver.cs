// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Text.Json;

using Aqua.Utils;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

internal sealed class AquaReferenceResolver : ReferenceResolver
{
    private readonly Dictionary<string, object> _registry = new(StringComparer.Ordinal);
    private readonly Dictionary<object, string> _lookup = new(ReferenceEqualityComparer<object>.Default);
    private uint _refCount;

    public override void AddReference(string referenceId, object value)
    {
        lock (_registry)
        {
            _registry.Add(referenceId, value);
            _lookup.Add(value, referenceId);
        }
    }

    public override string GetReference(object value, out bool alreadyExists)
    {
        lock (_registry)
        {
            alreadyExists = _lookup.TryGetValue(value, out var referenceId);
            if (!alreadyExists)
            {
                referenceId = $"{++_refCount}";
                AddReference(referenceId, value);
            }

            return referenceId!;
        }
    }

    public override object ResolveReference(string referenceId)
        => _registry.TryGetValue(referenceId, out var value)
        ? value
        : throw new JsonException($"Cannot resolve reference '{referenceId}'.");
}