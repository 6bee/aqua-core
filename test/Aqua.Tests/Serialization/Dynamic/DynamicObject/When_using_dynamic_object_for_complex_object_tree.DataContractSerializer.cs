// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Tests.Serialization.Dynamic.DynamicObject
{
    partial class When_using_dynamic_object_for_complex_object_tree
    {
        public class DataContractSerializer : When_using_dynamic_object_for_complex_object_tree
        {
            public DataContractSerializer()
                : base(DataContractSerializationHelper.Serialize)
            {
            }
        }
    }
}
