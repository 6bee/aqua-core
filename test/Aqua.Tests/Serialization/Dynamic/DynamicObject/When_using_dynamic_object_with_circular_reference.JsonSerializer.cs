// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Tests.Serialization.Dynamic.DynamicObject
{
    partial class When_using_dynamic_object_with_circular_reference
    {
        public class JsonSerializer : When_using_dynamic_object_with_circular_reference
        {
            public JsonSerializer()
                : base(JsonSerializationHelper.Serialize)
            {
            }
        }
    }
}
