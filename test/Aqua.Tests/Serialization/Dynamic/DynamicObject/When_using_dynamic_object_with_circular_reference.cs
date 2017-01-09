// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Tests.Serialization.Dynamic.DynamicObject
{
    using Aqua.Dynamic;
    using Shouldly;
    using System;
    using Xunit;

    public abstract partial class When_using_dynamic_object_with_circular_reference
    {
        DynamicObject serializedObject;

        protected When_using_dynamic_object_with_circular_reference(Func<DynamicObject, DynamicObject> serialize)
        {
            dynamic object_0 = new DynamicObject();
            dynamic object_1 = new DynamicObject();
            dynamic object_2 = new DynamicObject();

            object_0["Ref_1"] = object_1;
            object_1["Ref_2"] = object_2;
            object_2["Ref_0"] = object_0;

            serializedObject = serialize(object_0);

            ShouldBeTestExtensions.ShouldNotBeSameAs(serializedObject, object_0);
        }

        [Fact]
        public void Clone_should_contain_circular_reference()
        {
            var reference = serializedObject
                .Get<DynamicObject>("Ref_1")
                .Get<DynamicObject>("Ref_2")
                .Get<DynamicObject>("Ref_0");

            ShouldBeTestExtensions.ShouldBeSameAs(reference, serializedObject);
        }
    }
}
