﻿// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Tests.TypeSystem.Emit.TypeEmitter
{
    using Aqua.TypeSystem.Emit;
    using Shouldly;
    using System;
    using System.Collections.Generic;
    using Xunit;
    using PropertyInfo = Aqua.TypeSystem.PropertyInfo;
    using TypeInfo = Aqua.TypeSystem.TypeInfo;

    public class When_emitting_type
    {
        private readonly Type emittedType;

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
                },
            };

            emittedType = new TypeEmitter().EmitType(typeInfo);
        }

        [Fact]
        public void Emitted_type_shoult_have_name_special_name()
        {
            emittedType.Name.ShouldBe("<>__EmittedType__0");
        }

        [Fact]
        public void Emitted_type_shoult_have_two_properties()
        {
            emittedType.GetProperties().Length.ShouldBe(2);
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
