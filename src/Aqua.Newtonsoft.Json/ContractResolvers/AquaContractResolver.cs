// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Newtonsoft.Json.ContractResolvers
{
    using Aqua.Dynamic;
    using Aqua.Newtonsoft.Json.Converters;
    using Aqua.TypeSystem;
    using global::Newtonsoft.Json;
    using global::Newtonsoft.Json.Serialization;
    using System;
    using System.Linq;
    using System.Runtime.Serialization;

    public sealed class AquaContractResolver : DefaultContractResolver
    {
        private readonly KnownTypesRegistry _knownTypes;
        private readonly IContractResolver? _decorated;

        public AquaContractResolver(KnownTypesRegistry knownTypes, IContractResolver? decorated = null)
        {
            if (decorated is AquaContractResolver self)
            {
                decorated = self._decorated;
            }

            _knownTypes = knownTypes.CheckNotNull(nameof(knownTypes));
            _decorated = decorated?.GetType() == typeof(DefaultContractResolver) ? null : decorated;
        }

        public override JsonContract ResolveContract(Type type)
        {
            type.AssertNotNull(nameof(type));
            return _decorated is null || typeof(DynamicObject).IsAssignableFrom(type) || typeof(TypeInfo).IsAssignableFrom(type)
                ? base.ResolveContract(type)
                : _decorated.ResolveContract(type);
        }

        protected override JsonContract CreateContract(Type objectType)
            => IsTypeHandled(objectType.CheckNotNull(nameof(objectType)))
            ? CreateObjectContract(objectType)
            : base.CreateContract(objectType);

        private static bool IsTypeHandled(Type type)
            => type.CheckNotNull(nameof(type)).IsClass
            && Equals(type.Assembly, typeof(DynamicObject).Assembly)
            && type.GetCustomAttributes(typeof(DataContractAttribute), false).Length > 0;

        protected override JsonObjectContract CreateObjectContract(Type objectType)
        {
            var contract = base.CreateObjectContract(objectType.CheckNotNull(nameof(objectType)));
            if (IsTypeHandled(objectType.CheckNotNull(nameof(objectType))))
            {
                contract.IsReference = true;
                contract.ItemReferenceLoopHandling = ReferenceLoopHandling.Serialize;
                contract.Converter = typeof(DynamicObject).IsAssignableFrom(objectType)
                    ? new DynamicObjectConverter(_knownTypes)
                    : typeof(TypeInfo).IsAssignableFrom(objectType)
                        ? new TypeInfoConverter(_knownTypes)
                        : CreateObjectConverter(objectType, _knownTypes);
                foreach (var property in contract.Properties.Where(x => !x.Writable || !x.Readable))
                {
                    property.Ignored = true;
                }
            }

            return contract;
        }

        private static JsonConverter CreateObjectConverter(Type type, KnownTypesRegistry knownTypes)
            => (JsonConverter)Activator.CreateInstance(typeof(ObjectConverter<>).MakeGenericType(type), knownTypes) !;
    }
}
