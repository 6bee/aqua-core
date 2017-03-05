// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Tests.Serialization.TypeSystem.TypeInfo
{
    partial class When_serializing_typeinfo_of_dynamicobject
    {
        public class JsonSerializer : When_serializing_typeinfo_of_dynamicobject
        {
            public JsonSerializer()
                : base(JsonSerializationHelper.Serialize)
            {
            }
        }
    }
}
