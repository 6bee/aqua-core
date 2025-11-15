// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Tests.Dynamic.DynamicObjectMapper;

using Aqua.Dynamic;
using Shouldly;
using Xunit;

public class When_mapping_object_to_custom_dynamic_object
{
    private class DynamicObjectWithRefToSource : DynamicObject
    {
        public DynamicObjectWithRefToSource(Type type, object source)
            : base(type)
            => Source = source;

        public object Source { get; }
    }

    private class DynamicObjectFactory : IDynamicObjectFactory
    {
        public DynamicObject CreateDynamicObject(Type type, object instance)
            => new DynamicObjectWithRefToSource(type, instance);
    }

    private class A
    {
        public B BRef { get; set; }
    }

    private class B
    {
        public A ARef { get; set; }

        public C CRef { get; set; }
    }

    private class C
    {
        public A ARef { get; set; }

        public int Int32Property { get; set; }
    }

    private const int Int32Value = -1234;

    private readonly A source;

    private readonly DynamicObject dynamicObject;

    public When_mapping_object_to_custom_dynamic_object()
    {
        source = new A
        {
            BRef = new B
            {
                CRef = new C
                {
                    Int32Property = Int32Value,
                },
            },
        };

        source.BRef.ARef = source;
        source.BRef.CRef.ARef = source;

        var mapper = new DynamicObjectMapper(dynamicObjectFactory: new DynamicObjectFactory());

        dynamicObject = mapper.MapObject(source);
    }

    [Fact]
    public void Dynamic_object_should_be_custom_type_with_reference_to_source()
    {
        var obj = dynamicObject.ShouldBeOfType<DynamicObjectWithRefToSource>();

        obj.Source.ShouldBeSameAs(source);
    }

    [Fact]
    public void Dynamic_object_nested_reference_should_be_custom_type()
    {
        var obj = dynamicObject[nameof(A.BRef)].ShouldBeOfType<DynamicObjectWithRefToSource>();

        obj.Source.ShouldBeSameAs(source.BRef);
    }

    [Fact]
    public void Dynamic_object_nested_reference_on_sublevel_should_be_custom_type()
    {
        var ab = dynamicObject[nameof(A.BRef)].ShouldBeOfType<DynamicObjectWithRefToSource>();

        var ba = ab[nameof(B.ARef)].ShouldBeOfType<DynamicObjectWithRefToSource>();
        var bc = ab[nameof(B.CRef)].ShouldBeOfType<DynamicObjectWithRefToSource>();

        ba.ShouldBeSameAs(dynamicObject);
        ba.Source.ShouldBeSameAs(source);
        bc.Source.ShouldBeSameAs(source.BRef.CRef);

        bc[nameof(C.Int32Property)].ShouldBe(Int32Value);
        bc[nameof(C.ARef)].ShouldBeSameAs(dynamicObject);
        bc[nameof(C.ARef)].ShouldBeOfType<DynamicObjectWithRefToSource>().Source.ShouldBeSameAs(source);
    }
}