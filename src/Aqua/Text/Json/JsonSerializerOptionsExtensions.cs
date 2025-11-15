// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace System.Text.Json;
#pragma warning restore IDE0130 // Namespace does not match folder structure

using Aqua.Dynamic;
using Aqua.EnumerableExtensions;
using Aqua.Text.Json;
using Aqua.Text.Json.Converters;
using Aqua.TypeSystem;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

[EditorBrowsable(EditorBrowsableState.Never)]
public static class JsonSerializerOptionsExtensions
{
    /// <summary>
    /// Configures <see cref="JsonSerializerOptions"/> and adds <see cref="JsonConverter"/>s for <i>Aqua</i> types.
    /// </summary>
    /// <param name="options">Json serializer options to be ammended.</param>
    public static JsonSerializerOptions ConfigureAqua(this JsonSerializerOptions options)
        => options.ConfigureAqua(default(KnownTypesRegistry));

    /// <summary>
    /// Configures <see cref="JsonSerializerOptions"/> and adds <see cref="JsonConverter"/>s for <i>Aqua</i> types.
    /// </summary>
    /// <param name="options">Json serializer options to be ammended.</param>
    /// <param name="knownTypesRegistry">Optional registry of known types.</param>
    public static JsonSerializerOptions ConfigureAqua(this JsonSerializerOptions options, KnownTypesRegistry? knownTypesRegistry)
    {
        options.AssertNotNull();

        if (options.ReferenceHandler is not AquaReferenceHandler)
        {
            options.ReferenceHandler = AquaReferenceHandler.Root;
        }

        knownTypesRegistry ??= KnownTypesRegistry.Default;

        if (!options.Converters.Any(static x => x is DynamicObjectConverter))
        {
            options.Converters.Add(new DynamicObjectConverter(knownTypesRegistry));
        }

        if (!options.Converters.Any(static x => x is TypeInfoConverter))
        {
            options.Converters.Add(new TypeInfoConverter(knownTypesRegistry));
        }

        // Workaround: there seems to be no proper way to deal with converters for abtract base types,
        // hence we register for abstract as well as non-abstract types.
        typeof(MemberInfo).Assembly
            .GetTypes()
            .Where(static x => !x.IsAbstract)
            .Where(typeof(MemberInfo).IsAssignableFrom)
            .RegisterJsonConverter(typeof(MemberInfoConverter<>), options, knownTypesRegistry);
        if (!options.Converters.Any(static x => x is MemberInfoConverter))
        {
            options.Converters.Add(new MemberInfoConverter(knownTypesRegistry));
        }

        typeof(DynamicObject).Assembly
            .GetTypes()
            .Where(static x => x.IsClass && !x.IsAbstract && !x.IsGenericType)
            .Where(static x => x.GetCustomAttributes(typeof(DataContractAttribute), false).Length > 0)
            .RegisterJsonConverter(typeof(ObjectConverter<>), options, knownTypesRegistry);

        return options;
    }

    [SuppressMessage("Major Code Smell", "S1172:Unused method parameters should be removed", Justification = "False positive: 'knownTypesRegistry' used in local function")]
    private static void RegisterJsonConverter(this IEnumerable<Type> types, Type genericConverterType, JsonSerializerOptions options, KnownTypesRegistry knownTypesRegistry)
    {
        types
            .Where(x => !options.Converters.Any(c => c.CanConvert(x)))
            .Select(CreateJsonConverter)
            .ForEach(options.Converters.Add);

        JsonConverter CreateJsonConverter(Type type)
        {
            var converterType = genericConverterType.MakeGenericType(type);
            var converter = Activator.CreateInstance(converterType, knownTypesRegistry);
            return (JsonConverter)converter!;
        }
    }

    /// <summary>
    /// Adds a converter and returns the same instance of <see cref="JsonSerializerOptions"/>.
    /// </summary>
    /// <param name="options">The options to be ammended.</param>
    /// <param name="converter">The converter to be added.</param>
    /// <returns>The options containing the added converter.</returns>
    public static JsonSerializerOptions AddConverter(this JsonSerializerOptions options, JsonConverter converter)
    {
        options.Converters.Add(converter);
        return options;
    }

    /// <summary>
    /// Creates a copy of <see cref="JsonSerializerOptions"/> serving a persistent <see cref="ReferenceHandler"/>
    /// to overcome limited support of circular references with custom <see cref="JsonConverter"/>.
    /// </summary>
    internal static JsonSerializerOptions ToSessionOptions(this JsonSerializerOptions options)
    {
        var referenceHandler = options.ReferenceHandler as AquaReferenceHandler;
        if (referenceHandler?.IsRoot is true)
        {
            return new JsonSerializerOptions(options)
            {
                ReferenceHandler = new AquaReferenceHandler(referenceHandler),
            };
        }

        return options;
    }
}