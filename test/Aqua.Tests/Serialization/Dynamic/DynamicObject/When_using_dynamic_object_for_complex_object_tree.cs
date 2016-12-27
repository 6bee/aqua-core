// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Tests.Serialization.Dynamic.DynamicObject
{
    using Aqua.Dynamic;
    using System;
    using Xunit;
    using Shouldly;

    public abstract partial class When_using_dynamic_object_for_complex_object_tree
    {
        const double DoubleValue = 1.2345679e-87;
        const string StringValue = "eleven";
        const string CustomType = "system-string-type";

        DynamicObject serializedObject;

        protected When_using_dynamic_object_for_complex_object_tree(Func<DynamicObject, DynamicObject> serialize)
        {
            var originalObject = new DynamicObject()
            {
                Properties = new PropertySet
                {
                    { "DoubleValue", DoubleValue },
                    {
                        "Reference", new DynamicObject(typeof(string))
                        {
                            Properties = new PropertySet
                            {
                                { "StringValue", StringValue },
                                { "Type", CustomType },
                            }
                        }
                    },
                }
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

        [Fact]
        public void Clone_should_contain_nested_type_property()
        {
            var nestedObject = serializedObject["Reference"] as DynamicObject;

            nestedObject["Type"].ShouldBe(CustomType);
        }

        [Fact]
        public void Clone_should_contain_type_information()
        {
            var nestedObject = serializedObject["Reference"] as DynamicObject;

            (nestedObject.Type?.Type).ShouldBe(typeof(string));
        }
    }
}
