// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Tests.TypeSystem.Emit.TypeEmitter
{
    using Aqua.TypeSystem;
    using Aqua.TypeSystem.Emit;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Xunit;
    using Xunit.Fluent;

    public class When_emitting_type
    {
        private Type emittedType;

        public When_emitting_type()
        {
            var typeInfo = new TypeInfo
            {
                Name = "TestClass",
                Namespace = "TestNamespace",
                Properties = new List<PropertyInfo>
                {
                    new PropertyInfo { Name = "Int32Value", PropertyType = new TypeInfo(typeof(int)) },
                    new PropertyInfo { Name = "StringValue", PropertyType = new TypeInfo(typeof(string)) },
                }
            };

            emittedType = new TypeEmitter().EmitType(typeInfo);
        }

        [Fact]
        public void Emitted_type_shoult_have_name_special_name()
        {
            emittedType.Name.ShouldBe("<>__EmittedType__0");
            emittedType.Namespace.ShouldBe("<In Memory Module>");
            emittedType.FullName.ShouldBe("<In Memory Module>.<>__EmittedType__0");
        }

        [Fact]
        public void Emitted_type_shoult_have_two_properties()
        {
            emittedType.GetProperties().Count().ShouldBe(2);
        }

        [Fact]
        public void Emitted_type_properties_should_be_readable_and_writable()
        {
            var properties = emittedType.GetProperties();
            foreach (var property in properties)
            {
                property.CanRead.ShouldBeTrue();
                property.CanWrite.ShouldBeTrue();
            }
        }

        [Fact]
        public void Emitted_type_shoult_have_int_property()
        {
            emittedType.GetProperty("Int32Value").PropertyType.ShouldBe(typeof(int));
        }

        [Fact]
        public void Emitted_type_shoult_have_string_property()
        {
            emittedType.GetProperty("StringValue").PropertyType.ShouldBe(typeof(string));
        }

        [Fact]
        public void Emitted_type_shoult_have_default_constructor()
        {
            var instance = Activator.CreateInstance(emittedType);

            instance.GetType().ShouldBe(emittedType);
        }
    }
}
