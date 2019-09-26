// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Tests.Dynamic.DynamicObject
{
    using Aqua.Dynamic;
    using Shouldly;
    using System.Reflection;
    using Xunit;

    public class When_converting_to_object_with_static_members
    {
        private class CustomType
        {
#pragma warning disable CS0414 // Private filed is assigned but never used
#pragma warning disable SA1401 // Fields should be private
#pragma warning disable IDE0051 // Remove unused private members
#pragma warning disable IDE0044 // Add readonly modifier
#pragma warning disable SA1306 // Field names should begin with lower-case letter

            private const string PrivateConstString = "DefaultPrivateConstStringValue";

            public const string ConstString = "DefaultConstStringValue";

            private static readonly string PrivateStaticReadonlyField = "DefaultPrivateStaticReadonlyFieldValue";

            public static readonly string StaticReadonlyField = "DefaultStaticReadonlyFieldValue";

            public string WriteonlyPropertyBackingField = "DefaultWriteonlyPropertyValue";

            private static string PrivateStaticField = "DefaultPrivateStaticFieldValue";

            public static string StaticField = "DefaultStaticFieldValue";

            private readonly string PrivateReadonlyField = "DefaultPrivateReadonlyFieldValue";

            public readonly string ReadonlyField = "DefaultReadonlyFieldValue";

            private string PrivateField = "DefaultPrivateFieldValue";

            public string Field = "DefaultFieldValue";

            private static string PrivateStaticProperty { get; set; } = "DefaultPrivateStaticPropertyValue";

            public static string StaticProperty { get; set; } = "DefaultStaticPropertyValue";

            private string PrivateProperty { get; set; } = "DefaultPrivatePropertyValue";

            public string Property { get; set; } = "DefaultPropertyValue";

            public string ReadonlyProperty { get; } = "DefaultReadonlyPropertyValue";

            public string WriteonlyProperty { set => WriteonlyPropertyBackingField = value; }

#pragma warning restore SA1306 // Field names should begin with lower-case letter
#pragma warning restore IDE0044 // Add readonly modifier
#pragma warning restore IDE0051 // Remove unused private members
#pragma warning restore SA1401 // Fields should be private
#pragma warning restore CS0414 // Private filed is assigned but never used
        }

        private readonly CustomType obj;

        public When_converting_to_object_with_static_members()
        {
            var dynamicObject = new DynamicObject
            {
                Properties = new PropertySet
                 {
                     { "PrivateConstString", "PrivateConstStringValue" },
                     { "ConstString", "ConstStringValue" },
                     { "PrivateStaticReadonlyField", "PrivateStaticReadonlyFieldValue" },
                     { "StaticReadonlyField", "StaticReadonlyFieldValue" },
                     { "PrivateStaticField", "PrivateStaticFieldValue" },
                     { "StaticField", "StaticFieldValue" },
                     { "PrivateReadonlyField", "PrivateReadonlyFieldValue" },
                     { "ReadonlyField", "ReadonlyFieldValue" },
                     { "PrivateField", "PrivateFieldValue" },
                     { "Field", "FieldValue" },
                     { "PrivateStaticProperty", "PrivateStaticPropertyValue" },
                     { "StaticProperty", "StaticPropertyValue" },
                     { "PrivateProperty", "PrivatePropertyValue" },
                     { "Property", "PropertyValue" },
                     { "ReadonlyProperty", "ReadonlyPropertyValue" },
                     { "WriteonlyProperty", "WriteonlyPropertyValue" },
                 },
            };

            obj = dynamicObject.CreateObject<CustomType>();
        }

        [Fact]
        public void Should_create_an_instance()
        {
            obj.ShouldNotBeNull();
        }

        [Fact]
        public void Should_have_writable_insatnce_properties_set_from_dynamic_object()
        {
            GetPropertyValue("Property").ShouldBe("PropertyValue");
            GetFieldValue("WriteonlyPropertyBackingField").ShouldBe("WriteonlyPropertyValue");
        }

        [Fact]
        public void Should_have_writable_insatnce_fields_set_from_dynamic_object()
        {
            GetFieldValue("Field").ShouldBe("FieldValue");
        }

        [Fact]
        public void Should_have_readonly_private_and_static_members_unchanged()
        {
            GetPropertyValue("ReadonlyProperty").ShouldBe("DefaultReadonlyPropertyValue");
            GetPropertyValue("PrivateProperty").ShouldBe("DefaultPrivatePropertyValue");
            GetPropertyValue("StaticProperty").ShouldBe("DefaultStaticPropertyValue");
            GetPropertyValue("PrivateStaticProperty").ShouldBe("DefaultPrivateStaticPropertyValue");

            GetFieldValue("PrivateField").ShouldBe("DefaultPrivateFieldValue");
            GetFieldValue("ReadonlyField").ShouldBe("DefaultReadonlyFieldValue");
            GetFieldValue("PrivateReadonlyField").ShouldBe("DefaultPrivateReadonlyFieldValue");
            GetFieldValue("StaticField").ShouldBe("DefaultStaticFieldValue");
            GetFieldValue("PrivateStaticField").ShouldBe("DefaultPrivateStaticFieldValue");
            GetFieldValue("StaticReadonlyField").ShouldBe("DefaultStaticReadonlyFieldValue");
            GetFieldValue("PrivateStaticReadonlyField").ShouldBe("DefaultPrivateStaticReadonlyFieldValue");
            GetFieldValue("ConstString").ShouldBe("DefaultConstStringValue");
            GetFieldValue("PrivateConstString").ShouldBe("DefaultPrivateConstStringValue");
        }

        private object GetPropertyValue(string propertyName)
            => typeof(CustomType)
                .GetProperty(propertyName, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static)
                .GetValue(obj);

        private object GetFieldValue(string propertyName)
            => typeof(CustomType)
                .GetField(propertyName, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static)
                .GetValue(obj);
    }
}