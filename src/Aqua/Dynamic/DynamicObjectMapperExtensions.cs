// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Dynamic;

using Aqua.EnumerableExtensions;
using System.Collections;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;

[EditorBrowsable(EditorBrowsableState.Never)]
public static class DynamicObjectMapperExtensions
{
    /// <summary>
    /// Maps a <see cref="DynamicObject"/> into an instance of <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The target type in which the <see cref="DynamicObject"/> have to be mapped to.</typeparam>
    /// <param name="objectMapper">The <see cref="IDynamicObjectMapper"/> instance used to map the <see cref="DynamicObject"/>s.</param>
    /// <param name="obj"><see cref="DynamicObject"/> to be mapped.</param>
    /// <returns>The object created based on the <see cref="DynamicObject"/> specified.</returns>
    public static T Map<T>(this IDynamicObjectMapper objectMapper, DynamicObject? obj)
    {
        objectMapper.AssertNotNull();

        return (T)objectMapper.Map(obj, typeof(T))!;
    }

    /// <summary>
    /// Maps a collection of <see cref="DynamicObject" />s into a collection of objects.
    /// </summary>
    /// <param name="objectMapper">The <see cref="IDynamicObjectMapper"/> instance used to map the <see cref="DynamicObject"/>s.</param>
    /// <param name="objects">Collection of <see cref="DynamicObject" /> to be mapped.</param>
    /// <param name="type">Target type for mapping, set this parameter to <see langword="null"/> if type information included within individual <see cref="DynamicObject" />s should be used.</param>
    /// <returns>Collection of objects created based on the <see cref="DynamicObject" />s specified.</returns>
    [return: NotNullIfNotNull(nameof(objects))]
    public static IEnumerable? Map(this IDynamicObjectMapper objectMapper, IEnumerable<DynamicObject?>? objects, Type? type = null)
    {
        objectMapper.AssertNotNull();

        if (objects is null)
        {
            return default!;
        }

        IEnumerable<object?> source = objects.Select(x => objectMapper.Map(x, type));
        return type is null || type == typeof(object)
            ? source.ToArray()
            : source.CastCollectionToArrayOfType(type);
    }

    /// <summary>
    /// Maps a collection of <see cref="DynamicObject" />s into a collection of <typeparamref name="T" />.
    /// </summary>
    /// <typeparam name="T">The target type in which the <see cref="DynamicObject" /> have to be mapped to.</typeparam>
    /// <param name="objectMapper">The <see cref="IDynamicObjectMapper"/> instance used to map the <see cref="DynamicObject"/>s.</param>
    /// <param name="objects">Collection of <see cref="DynamicObject" />s to be mapped.</param>
    /// <returns>Collection of <typeparamref name="T" /> created based on the <see cref="DynamicObject" />s specified.</returns>
    [return: NotNullIfNotNull(nameof(objects))]
    public static IEnumerable<T>? Map<T>(this IDynamicObjectMapper objectMapper, IEnumerable<DynamicObject>? objects)
    {
        objectMapper.AssertNotNull();

        if (objects is null)
        {
            return default!;
        }

        return (IEnumerable<T>)objectMapper.Map(objects, typeof(T));
    }

    /// <summary>
    /// Maps a collection of objects into a collection of <see cref="DynamicObject"/>.
    /// </summary>
    /// <param name="objectMapper">The <see cref="IDynamicObjectMapper"/> instance used to map the <see cref="DynamicObject"/>s.</param>
    /// <param name="objects">The object to be mapped.</param>
    /// <param name="setTypeInformation">Set this parameter to <see langword="true"/> if type information should be included within the <see cref="DynamicObject"/>s,
    /// set it to <see langword="false"/> otherwise.</param>
    /// <returns>A collection of <see cref="DynamicObject"/> representing the objects specified.</returns>
    [return: NotNullIfNotNull(nameof(objects))]
    public static IReadOnlyList<DynamicObject?>? MapCollection(this IDynamicObjectMapper objectMapper, object? objects, Func<Type, bool>? setTypeInformation = null)
    {
        objectMapper.AssertNotNull();

        IEnumerable<DynamicObject?>? enumerable;
        if (objects is null)
        {
            enumerable = null;
        }
        else if (objects is IEnumerable<DynamicObject> x)
        {
            enumerable = x;
        }
        else if (objects.IsCollection(out var collection))
        {
            enumerable = collection
                .Cast<object>()
                .Select(x => objectMapper.MapObject(x, setTypeInformation));
        }
        else
        {
            // put single object into dynamic object
            var value = objectMapper.MapObject(objects, setTypeInformation);
            enumerable = new[] { value };
        }

        return enumerable?.ToArray();
    }
}