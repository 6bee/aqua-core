// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

#if NET && !NETCOREAPP2

namespace Aqua.Tests.Serialization.TypeSystem.TypeInfo
{
    partial class When_serializing_typeinfo_of_dynamicobject
    {
        public class NetDataContractSerializer : When_serializing_typeinfo_of_dynamicobject
        {
            public NetDataContractSerializer()
                : base(NetDataContractSerializationHelper.Serialize)
            {
            }
        }
    }
}

#endif