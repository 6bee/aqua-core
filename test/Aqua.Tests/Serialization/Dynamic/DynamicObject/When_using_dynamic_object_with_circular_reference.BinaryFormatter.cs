// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

#if NET

namespace Aqua.Tests.Serialization.Dynamic.DynamicObject
{
    partial class When_using_dynamic_object_with_circular_reference
    {
        public class BinaryFormatter : When_using_dynamic_object_with_circular_reference
        {
            public BinaryFormatter()
                : base(BinarySerializationHelper.Serialize)
            {
            }
        }
    }
}

#endif