// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Newtonsoft.Json.ContractResolvers
{
    using Aqua.Dynamic;
    using Aqua.Newtonsoft.Json.Converters;
    using global::Newtonsoft.Json;
    using global::Newtonsoft.Json.Serialization;
    using System;
    using System.Linq;
    using System.Runtime.Serialization;

    public sealed class AquaContractResolver : DefaultContractResolver
    {
        private readonly IContractResolver _decorated;

        public AquaContractResolver(IContractResolver decorated = null)
        {
            if (decorated is AquaContractResolver self)
            {
                decorated = self._decorated;
            }

            _decorated = decorated?.GetType() == typeof(DefaultContractResolver) ? null : decorated;
        }

        public override JsonContract ResolveContract(Type type)
            => _decorated is null || typeof(DynamicObject).IsAssignableFrom(type)
            ? base.ResolveContract(type)
            : _decorated.ResolveContract(type);

        protected override JsonContract CreateContract(Type objectType)
            => IsTypeHandled(objectType)
            ? CreateObjectContract(objectType)
            : base.CreateContract(objectType);

        private static bool IsTypeHandled(Type type)
            => Equals(type.Assembly, typeof(DynamicObject).Assembly)
            && type.GetCustomAttributes(typeof(DataContractAttribute), false).Length > 0;

        protected override JsonObjectContract CreateObjectContract(Type objectType)
        {
            var contract = base.CreateObjectContract(objectType);

            if (IsTypeHandled(objectType))
            {
                contract.IsReference = true;
                contract.ItemReferenceLoopHandling = ReferenceLoopHandling.Serialize;
                contract.Converter = typeof(DynamicObject).IsAssignableFrom(objectType)
                    ? new DynamicObjectConverter()
                    : CreateObjectConverter(objectType);
                foreach (var property in contract.Properties.Where(x => !x.Writable || !x.Readable))
                {
                    property.Ignored = true;
                }
            }

            return contract;
        }

        private static JsonConverter CreateObjectConverter(Type type)
            => (JsonConverter)Activator.CreateInstance(typeof(ObjectConverter<>).MakeGenericType(type));
    }
}
