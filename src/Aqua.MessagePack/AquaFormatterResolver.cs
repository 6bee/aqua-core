// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.MessagePack;

using Aqua.MessagePack.Formatters;
using global::MessagePack.Resolvers;

/// <summary>
/// An <see cref="IFormatterResolver"/> that provides hand-written, type-safe MessagePack formatters
/// for the <i>Aqua</i> object graph (<see cref="DynamicObject"/>, <see cref="Property"/>,
/// <see cref="PropertySet"/>, <see cref="TypeInfo"/>, and the leaf-value union), falling back to
/// <see cref="StandardResolver"/> for all other types.
/// </summary>
public sealed class AquaFormatterResolver(IFormatterResolver? fallback = null) : IFormatterResolver
{
    public static readonly AquaFormatterResolver Instance = new();

    private readonly IFormatterResolver _fallback = fallback ?? StandardResolver.Instance;

    private readonly static Dictionary<Type, object> _formatters = new()
    {
        [typeof(object)] = AquaValueFormatter.Instance,
        [typeof(DynamicObject)] = DynamicObjectFormatter.Instance,
        [typeof(TypeInfo)] = TypeInfoFormatter.Instance,
        [typeof(FieldInfo)] = FieldInfoFormatter.Instance,
        [typeof(PropertyInfo)] = PropertyInfoFormatter.Instance,
        [typeof(ConstructorInfo)] = ConstructorInfoFormatter.Instance,
        [typeof(MethodInfo)] = MethodInfoFormatter.Instance,
    };

    /// <inheritdoc/>
    public IMessagePackFormatter<T>? GetFormatter<T>()
    {
        if (_formatters.TryGetValue(typeof(T), out var formatter))
        {
            return (IMessagePackFormatter<T>)formatter;
        }

        return _fallback.GetFormatter<T>();
    }
}
