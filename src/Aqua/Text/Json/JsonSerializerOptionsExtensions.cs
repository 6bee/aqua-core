// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace System.Text.Json;
#pragma warning restore IDE0130 // Namespace does not match folder structure

using Aqua.Dynamic;
using Aqua.EnumerableExtensions;
using Aqua.Text.Json;
using Aqua.Text.Json.Converters;
using Aqua.TypeSystem;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
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

        if (!options.Converters.Any(static c => c.CanConvert(typeof(DynamicObject))))
        {
            options.Converters.Add(new DynamicObjectConverter(knownTypesRegistry));
        }

        if (!options.Converters.Any(static c => c.CanConvert(typeof(TypeInfo))))
        {
            options.Converters.Add(new TypeInfoConverter(knownTypesRegistry));
        }

        typeof(MemberInfo).Assembly
            .GetTypes()
            .Where(static x => !x.IsAbstract)
            .Where(typeof(MemberInfo).IsAssignableFrom)
            .RegisterJsonConverter(typeof(MemberInfoConverter<>), options, knownTypesRegistry);

        if (!options.Converters.Any(c => c.CanConvert(typeof(MemberInfo))))
        {
            options.Converters.Add(new MemberInfoConverter<MemberInfo>(knownTypesRegistry, true));
        }

        typeof(DynamicObject).Assembly
            .GetTypes()
            .Where(static x => x.IsClass && !x.IsAbstract && !x.IsGenericType)
            .Where(static x => x.GetCustomAttributes(typeof(DataContractAttribute), false).Length is not 0)
            .Where(static x => x.GetCustomAttributes(typeof(JsonConverterAttribute), false).Length is 0)
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
        if (options.ReferenceHandler is { } referenceHandler)
        {
            if (referenceHandler is not AquaReferenceHandler aquaReferenceHandler)
            {
                // ensure we continue with a AquaReferenceHandler since the framework implementations ReferenceHandler.Preserve and  ReferenceHandler.IgnoreCycles
                // prevent custom converters take part in the reference handling game.
                aquaReferenceHandler = AquaReferenceHandler.Root;
            }

            if (aquaReferenceHandler.IsRoot)
            {
                options = new(options)
                {
                    ReferenceHandler = new AquaReferenceHandler(aquaReferenceHandler),
                };
            }
        }

        return options;
    }
}