// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Tests.Dynamic.DynamicObjectMapper;

using Aqua.Dynamic;
using Shouldly;
using Xunit;

public class When_mapping_object_with_static_members
{
    private class CustomType
    {
#pragma warning disable CS0414 // CS0414: The field is assigned but its value is never used
        private const string PrivateConstString = "PrivateConstStringValue";

        public const string ConstString = "ConstStringValue";

        private static readonly string PrivateStaticReadonlyField = "PrivateStaticReadonlyFieldValue";

        public static readonly string StaticReadonlyField = "StaticReadonlyFieldValue";

        private static string _privateStaticField = "PrivateStaticFieldValue";

        public static string StaticField = "StaticFieldValue";

        private readonly string _privateReadonlyField = "PrivateReadonlyFieldValue";

        public readonly string ReadonlyField = "ReadonlyFieldValue";

        private string _privateField = "PrivateFieldValue";

        public string Field = "FieldValue";

        private static string PrivateStaticProperty { get; set; } = "PrivateStaticPropertyValue";

        public static string StaticProperty { get; set; } = "StaticPropertyValue";

        private string PrivateProperty { get; set; } = "PrivatePropertyValue";

        public string Property { get; set; } = "PropertyValue";

        public string ReadonlyProperty { get; } = "ReadonlyPropertyValue";
#pragma warning restore CS0414 // CS0414: The field is assigned but its value is never used
    }

    private readonly DynamicObject dynamicObject;

    public When_mapping_object_with_static_members()
    {
        dynamicObject = new DynamicObjectMapper().MapObject(new CustomType());
    }

    [Fact]
    public void Dynamic_object_type_should_contain_non_readonly_instance_members()
    {
        dynamicObject.Properties.Count.ShouldBe(4);
    }

    [Fact]
    public void Dynamic_object_type_should_contain_readable_instance_properties()
    {
        dynamicObject[nameof(CustomType.Property)].ShouldBe("PropertyValue");
        dynamicObject[nameof(CustomType.ReadonlyProperty)].ShouldBe("ReadonlyPropertyValue");
    }

    [Fact]
    public void Dynamic_object_type_should_contain_readable_instance_fields()
    {
        dynamicObject[nameof(CustomType.Field)].ShouldBe("FieldValue");
        dynamicObject[nameof(CustomType.ReadonlyField)].ShouldBe("ReadonlyFieldValue");
    }
}