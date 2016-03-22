// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Tests.Serialization.Dynamic.DynamicObject
{
    using Aqua.Dynamic;
    using System;
    using Xunit;
    using Xunit.Fluent;

    public abstract partial class When_using_dynamic_object_for_complex_object_tree
    {
        const double DoubleValue = 1.234567e-987;
        const string StringValue = "eleven";

        DynamicObject serializedObject;

        protected When_using_dynamic_object_for_complex_object_tree(Func<DynamicObject, DynamicObject> serialize)
        {
            var originalObject = new DynamicObject()
            {
                { "DoubleValue", DoubleValue },
                {
                    "Reference", new DynamicObject()
                    {
                        { "StringValue", StringValue },
                    }
                },
            };

            serializedObject = serialize(originalObject);
        }

        [Fact]
        public void Clone_should_contain_simple_numeric_property()
        {
            serializedObject["DoubleValue"].ShouldBe(DoubleValue);
        }

        [Fact]
        public void Clone_should_contain_nested_string_property()
        {
            var nestedObject = serializedObject["Reference"] as DynamicObject;

            nestedObject["StringValue"].ShouldBe(StringValue);
        }
    }
}
