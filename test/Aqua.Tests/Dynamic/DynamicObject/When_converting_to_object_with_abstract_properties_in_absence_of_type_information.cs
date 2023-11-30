// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Tests.Dynamic.DynamicObject;

using Aqua.Dynamic;
using Shouldly;
using System;
using Xunit;

public class When_converting_to_object_with_abstract_properties_in_absence_of_type_information
{
    private class CustomMapper : DynamicObjectMapper
    {
        protected override object MapFromDynamicObjectGraph(object obj, Type targetType)
        {
            if (targetType == typeof(BaseA))
            {
                targetType = typeof(A);
            }

            return base.MapFromDynamicObjectGraph(obj, targetType);
        }
    }

    private abstract class BaseA
    {
    }

    private class A : BaseA
    {
    }

    private class ClassWithAbstractProperties
    {
        public BaseA Ref { get; set; }

        public object Value1 { get; set; }

        public object Value2 { get; set; }

        public object Value3 { get; set; }

        public object Value4 { get; set; }
    }

    private readonly object obj;

    public When_converting_to_object_with_abstract_properties_in_absence_of_type_information()
    {
        var dynamicObject = new DynamicObject
        {
            Properties = new PropertySet
            {
                { nameof(ClassWithAbstractProperties.Ref), new DynamicObject() },
                { nameof(ClassWithAbstractProperties.Value1), "the value's pay load" },
                { nameof(ClassWithAbstractProperties.Value2), 222 },
                { nameof(ClassWithAbstractProperties.Value3), null },
                { nameof(ClassWithAbstractProperties.Value4), new DynamicObject() },
            },
        };

        var mapper = new CustomMapper();

        obj = mapper.Map<ClassWithAbstractProperties>(dynamicObject);
    }

    [Fact]
    public void Should_recreate_object_with_original_values()
    {
        var instance = obj.ShouldBeOfType<ClassWithAbstractProperties>();
        instance.Ref.ShouldBeOfType<A>();
        instance.Value1.ShouldBe("the value's pay load");
        instance.Value2.ShouldBe(222);
        instance.Value3.ShouldBeNull();
        instance.Value4.ShouldBeOfType<object>();
    }
}
