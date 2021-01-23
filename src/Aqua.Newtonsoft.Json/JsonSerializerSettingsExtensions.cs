// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua
{
    using Aqua.Newtonsoft.Json;
    using Aqua.Newtonsoft.Json.ContractResolvers;
    using global::Newtonsoft.Json;
    using global::Newtonsoft.Json.Serialization;
    using System.ComponentModel;

    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class JsonSerializerSettingsExtensions
    {
        /// <summary>
        /// Sets the <see cref="AquaContractResolver"/> in <see cref="JsonSerializerSettings"/>,
        /// decorating a previousely set <see cref="IContractResolver"/> if required.
        /// </summary>
        public static AquaJsonSerializerSettings ConfigureAqua(this JsonSerializerSettings jsonSerializerSettings)
        {
            var knownTypesRegistry = new KnownTypesRegistry();

            var aquaJsonSerializerSettings = new AquaJsonSerializerSettings(jsonSerializerSettings.CheckNotNull(nameof(jsonSerializerSettings)), knownTypesRegistry)
            {
                TypeNameHandling = TypeNameHandling.None,
            };

            if (aquaJsonSerializerSettings.ContractResolver?.GetType() != typeof(AquaContractResolver))
            {
                aquaJsonSerializerSettings.ContractResolver = new AquaContractResolver(aquaJsonSerializerSettings.ContractResolver, knownTypesRegistry);
            }

            return aquaJsonSerializerSettings;
        }
    }
}
