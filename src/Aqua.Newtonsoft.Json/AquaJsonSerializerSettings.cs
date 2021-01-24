// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Newtonsoft.Json
{
    using global::Newtonsoft.Json;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    public class AquaJsonSerializerSettings : JsonSerializerSettings
    {
        private static readonly IReadOnlyCollection<PropertyInfo> _baseProperties = typeof(JsonSerializerSettings)
            .GetProperties()
            .Where(x => x.CanRead && x.CanWrite)
            .ToArray();

        public AquaJsonSerializerSettings(JsonSerializerSettings settings, KnownTypesRegistry? knownTypesRegistry = null)
        {
            settings.CheckNotNull(nameof(settings));
            KnownTypesRegistry = knownTypesRegistry ?? new KnownTypesRegistry();
            Copy(settings);
        }

        public KnownTypesRegistry KnownTypesRegistry { get; }

        private void Copy(JsonSerializerSettings source)
        {
            foreach (var property in _baseProperties)
            {
                var value = property.GetValue(source);
                property.SetValue(this, value);
            }
        }
    }
}
