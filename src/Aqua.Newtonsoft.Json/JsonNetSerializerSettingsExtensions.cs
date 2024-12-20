// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace Newtonsoft.Json;
#pragma warning restore IDE0130 // Namespace does not match folder structure

using Aqua.Newtonsoft.Json;
using Aqua.Newtonsoft.Json.ContractResolvers;
using global::Newtonsoft.Json.Serialization;
using System.ComponentModel;

[EditorBrowsable(EditorBrowsableState.Never)]
public static class JsonNetSerializerSettingsExtensions
{
    /// <summary>
    /// Sets the <see cref="AquaContractResolver"/> in <see cref="JsonSerializerSettings"/>,
    /// decorating a previousely set <see cref="IContractResolver"/> if required.
    /// </summary>
    public static T ConfigureAqua<T>(this T settings, KnownTypesRegistry? knownTypesRegistry = null)
        where T : JsonSerializerSettings
    {
        settings.AssertNotNull();
        knownTypesRegistry ??= new KnownTypesRegistry();

        settings.TypeNameHandling = TypeNameHandling.None;
        if (settings.ContractResolver is not AquaContractResolver)
        {
            settings.ContractResolver = new AquaContractResolver(knownTypesRegistry, settings.ContractResolver);
        }

        return settings;
    }

    /// <summary>
    /// Creates a new instance of <see cref="AquaJsonSerializerSettings"/> class, based on the <see cref="JsonSerializerSettings"/> specified.
    /// </summary>
    public static AquaJsonSerializerSettings CreateAquaConfiguration(this JsonSerializerSettings settings, KnownTypesRegistry? knownTypesRegistry = null)
    {
        var aquaSettings = new AquaJsonSerializerSettings(settings, knownTypesRegistry);
        return aquaSettings.ConfigureAqua(aquaSettings.KnownTypesRegistry);
    }
}