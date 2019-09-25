// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua
{
    using Aqua.Dynamic;
    using global::Newtonsoft.Json.Serialization;
    using System;
    using System.Linq;

    public class AquaContractResolver : DefaultContractResolver
    {
        private readonly IContractResolver _decorated;

        public AquaContractResolver(IContractResolver decorated = null)
        {
            _decorated = decorated;
        }

        public override JsonContract ResolveContract(Type type)
        {
            if (_decorated is null || typeof(DynamicObject).IsAssignableFrom(type))
            {
                return base.ResolveContract(type);
            }

            return _decorated.ResolveContract(type);
        }

        protected override JsonContract CreateContract(Type objectType)
        {
            if (typeof(DynamicObject).IsAssignableFrom(objectType))
            {
                return CreateObjectContract(objectType);
            }

            return base.CreateContract(objectType);
        }

        protected override JsonObjectContract CreateObjectContract(Type objectType)
        {
            var contract = base.CreateObjectContract(objectType);

            if (typeof(DynamicObject).IsAssignableFrom(objectType))
            {
                contract.OnDeserializedCallbacks.Add(NativeValueInspector.DynamicObjectSerializationCallback);
                contract.IsReference = true;
                foreach (var property in contract.Properties.Where(x => !x.Writable || !x.Readable))
                {
                    property.Ignored = true;
                }
            }

            return contract;
        }
    }
}
