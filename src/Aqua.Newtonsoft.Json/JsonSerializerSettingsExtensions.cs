// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua
{
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
        public static JsonSerializerSettings ConfigureAqua(this JsonSerializerSettings jsonSerializerSettings)
        {
            jsonSerializerSettings.TypeNameHandling = TypeNameHandling.None;

            jsonSerializerSettings.ContractResolver = jsonSerializerSettings.ContractResolver?.GetType() == typeof(DefaultContractResolver)
                ? new AquaContractResolver()
                : new AquaContractResolver(jsonSerializerSettings.ContractResolver);

            return jsonSerializerSettings;
        }
    }
}
