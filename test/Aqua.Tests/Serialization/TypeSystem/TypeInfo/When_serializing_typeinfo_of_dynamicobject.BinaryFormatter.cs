// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

#if !NETCOREAPP1_0

namespace Aqua.Tests.Serialization.TypeSystem.TypeInfo
{
    partial class When_serializing_typeinfo_of_dynamicobject
    {
        public class BinaryFormatter : When_serializing_typeinfo_of_dynamicobject
        {
            public BinaryFormatter()
                : base(BinarySerializationHelper.Serialize)
            {
            }
        }
    }
}

#endif