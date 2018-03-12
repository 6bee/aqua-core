// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Tests.TypeSystem.TypeResolver
{
    using Aqua.TypeSystem;
    using Aqua.TypeSystem.Emit;
    using Aqua.TypeSystem.Extensions;
    using Shouldly;
    using System;
    using System.Linq;
    using System.Reflection;
    using Xunit;
    using TypeInfo = Aqua.TypeSystem.TypeInfo;

    public class When_resolving_unknown_type
    {
        private class A
        {
            public int Int32Value { get; set; }

            public string StringValue { get; set; }
        }

        private readonly Type emitedType;

        public When_resolving_unknown_type()
        {
            var typeInfo = new TypeInfo(typeof(A));

            typeInfo.Name = "UnknowTestClass";
            typeInfo.Namespace = "Unkown.Test.Namespace";
            typeInfo.DeclaringType = null;

            emitedType = new TypeResolver().ResolveType(typeInfo);
        }

        [Fact]
        public void Emitted_type_should_be_different_from_actual_type()
        {
            emitedType.ShouldNotBe(typeof(A));
        }

        [Fact]
        public void Emitted_type_should_be_dynamically_emited_type()
        {
            emitedType.ShouldBeAnnotatedWith<EmittedTypeAttribute>();
        }

        [Fact]
        public void Emitted_type_shoult_have_name_special_name()
        {
            emitedType.Name.ShouldBe("<>__EmittedType__0");
        }

        [Fact]
        public void Emited_type_should_be_non_generic_type()
        {
            emitedType.IsGenericType().ShouldBeFalse();
        }

        [Fact]
        public void Emited_type_should_have_two_properties()
        {
            emitedType.GetProperties().Count().ShouldBe(2);
        }

        [Fact]
        public void All_properties_should_be_readable()
        {
            foreach (var p in emitedType.GetProperties())
            {
                p.CanRead.ShouldBeTrue();
            }
        }

        [Fact]
        public void All_properties_should_be_writable()
        {
            foreach (var p in emitedType.GetProperties())
            {
                p.CanWrite.ShouldBeTrue();
            }
        }

        [Fact]
        public void Emited_type_should_have_int_property()
        {
            emitedType.GetProperty("Int32Value").PropertyType.ShouldBe(typeof(int));
        }

        [Fact]
        public void Emited_type_should_have_string_property()
        {
            emitedType.GetProperty("StringValue").PropertyType.ShouldBe(typeof(string));
        }
    }
}
