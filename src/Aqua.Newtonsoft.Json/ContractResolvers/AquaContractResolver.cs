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
            _decorated = decorated;
        }

        public override JsonContract ResolveContract(Type type)
        {
            var contract = _decorated is null || typeof(DynamicObject).IsAssignableFrom(type)
                ? base.ResolveContract(type)
                : _decorated.ResolveContract(type);

            var dataContractAttribute = type.GetCustomAttributes(typeof(DataContractAttribute), false);
            if (dataContractAttribute?.Cast<DataContractAttribute>().SingleOrDefault()?.IsReference == true)
            {
                contract.IsReference = true;
                if (contract is JsonObjectContract objectContract)
                {
                    objectContract.ItemIsReference = true;
                    objectContract.ItemReferenceLoopHandling = ReferenceLoopHandling.Serialize;
                }
            }

            return contract;
        }

        protected override JsonContract CreateContract(Type objectType)
            => typeof(DynamicObject).IsAssignableFrom(objectType)
            ? CreateObjectContract(objectType)
            : base.CreateContract(objectType);

        protected override JsonObjectContract CreateObjectContract(Type objectType)
        {
            var contract = base.CreateObjectContract(objectType);

            if (typeof(DynamicObject).IsAssignableFrom(objectType))
            {
                contract.Converter = new DynamicObjectConverter();
                foreach (var property in contract.Properties.Where(x => !x.Writable || !x.Readable))
                {
                    property.Ignored = true;
                }
            }

            return contract;
        }
    }
}
