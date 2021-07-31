// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Tests.Dynamic.DynamicObject
{
    using Aqua.Dynamic;
    using Shouldly;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using Xunit;

    public class When_converting_to_object_with_static_members
    {
        [SuppressMessage("Major Code Smell", "S1144:Unused private types or members should be removed", Justification = "Fields are used via reflection only")]
        [SuppressMessage("Major Code Smell", "S2933:Fields that are only assigned in the constructor should be \"readonly\"", Justification = "Fields are used via reflection only")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:Fields should be private", Justification = "Public fields required by test scenario")]
        private class TestType
        {
#pragma warning disable CS0414 // CS0414: The field is assigned but its value is never used
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
#pragma warning restore CS0414 // CS0414: The field is assigned but its value is never used
        }

        private const BindingFlags Any = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static;

        private readonly TestType obj;

        public When_converting_to_object_with_static_members()
        {
            var properties = typeof(TestType)
                .GetMembers(Any)
                .Where(x => x is FieldInfo || x is PropertyInfo)
                .Where(x => x.GetCustomAttribute<CompilerGeneratedAttribute>() is null)
                .Select(x => x.Name)
                .Select(x => new Property(x, $"{x.Replace("BackingField", null)}Value"));
            var dynamicObject = new DynamicObject { Properties = new PropertySet(properties) };
            obj = dynamicObject.CreateObject<TestType>();
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
            => typeof(TestType)
                .GetProperty(propertyName, Any)
                .GetValue(obj);

        private object GetFieldValue(string propertyName)
            => typeof(TestType)
                .GetField(propertyName, Any)
                .GetValue(obj);
    }
}