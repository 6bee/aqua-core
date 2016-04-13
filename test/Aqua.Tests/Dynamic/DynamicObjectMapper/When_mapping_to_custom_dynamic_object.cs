// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Tests.Dynamic.DynamicObjectMapper
{
    using Aqua.Dynamic;
    using System;
    using Xunit;
    using Xunit.Fluent;

    public class When_mapping_to_custom_dynamic_object
    {
        class DynamicObjectWithRefToSource : DynamicObject
        {
            public DynamicObjectWithRefToSource(Type type, object source)
                : base(type)
            {
                Source = source;
            }

            public object Source { get; }
        }

        class DynamicObjectFactory : IDynamicObjectFactory
        {
            public DynamicObject CreateDynamicObject(Type type, object instance)
            {
                return new DynamicObjectWithRefToSource(type, instance);
            }
        }

        class A
        {
            public B B { get; set; }
        }

        class B
        {
            public A A { get; set; }
            public C C { get; set; }
        }

        class C
        {
            public A A { get; set; }
            public int Int32Value { get; set; }
        }

        const int Int32Value = -1234;

        A source;

        DynamicObject dynamicObject;

        public When_mapping_to_custom_dynamic_object()
        {
            source = new A
            {
                B = new B
                {
                    C = new C
                    {
                        Int32Value = Int32Value
                    }
                }
            };

            source.B.A = source;
            source.B.C.A = source;

            var mapper = new DynamicObjectMapper(dynamicObjectFactory: new DynamicObjectFactory());

            dynamicObject = mapper.MapObject(source);
        }

        [Fact]
        public void Dynamic_object_should_be_custom_type_with_reference_to_source()
        {
            dynamicObject.ShouldBeOfType<DynamicObjectWithRefToSource>();

            ((DynamicObjectWithRefToSource)dynamicObject).Source.ShouldBeSameInstance(source);
        }

        [Fact]
        public void Dynamic_object_nested_reference_should_be_custom_type()
        {
            dynamicObject["B"].ShouldBeOfType<DynamicObjectWithRefToSource>();

            ((DynamicObjectWithRefToSource)dynamicObject["B"]).Source.ShouldBeSameInstance(source.B);
        }

        [Fact]
        public void Dynamic_object_nested_reference_on_sublevel_should_be_custom_type()
        {
            var dynamicB = (DynamicObject)dynamicObject["B"];
            
            dynamicB["A"].ShouldBeOfType<DynamicObjectWithRefToSource>();
            dynamicB["C"].ShouldBeOfType<DynamicObjectWithRefToSource>();

            dynamicB["A"].ShouldBeSameInstance(dynamicObject);
            ((DynamicObjectWithRefToSource)dynamicB["A"]).Source.ShouldBeSameInstance(source);
            ((DynamicObjectWithRefToSource)dynamicB["C"]).Source.ShouldBeSameInstance(source.B.C);

            var dynamicC = (DynamicObject)dynamicB["C"];
            dynamicC["Int32Value"].ShouldBe(Int32Value);
            dynamicC["A"].ShouldBeSameInstance(dynamicObject);
            ((DynamicObjectWithRefToSource)dynamicC["A"]).Source.ShouldBeSameInstance(source);
        }
    }
}
