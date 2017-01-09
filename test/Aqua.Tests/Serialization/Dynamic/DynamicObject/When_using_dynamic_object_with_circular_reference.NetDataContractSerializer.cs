// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

#if NET

namespace Aqua.Tests.Serialization.Dynamic.DynamicObject
{
    partial class When_using_dynamic_object_with_circular_reference
    {
        public class NetDataContractSerializer : When_using_dynamic_object_with_circular_reference
        {
            public NetDataContractSerializer()
                : base(NetDataContractSerializationHelper.Serialize)
            {
            }
        }
    }
}

#endif