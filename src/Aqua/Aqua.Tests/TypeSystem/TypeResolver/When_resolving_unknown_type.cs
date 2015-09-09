// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Tests.TypeSystem.TypeResolver
{
    using Aqua.TypeSystem;
    using System;
    using Xunit;
    using Xunit.Should;

    public class When_resolving_unknown_type
    {
        class A
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
            emitedType.ShouldNotBeNull();

            emitedType.Assembly.GetName().Name.ShouldBe("Aqua.TypeSystem.Emit.Types");

            emitedType.Assembly.IsDynamic.ShouldBeTrue();
        }

        [Fact]
        public void Emitted_type_shoult_have_name_special_name()
        {
            emitedType.Namespace.ShouldBe("<In Memory Module>");

            emitedType.Name.ShouldBe("<>__EmittedType__0");

            emitedType.FullName.ShouldBe("<In Memory Module>.<>__EmittedType__0");
        }

        [Fact]
        public void Emited_type_should_be_non_generic_type()
        {
            emitedType.IsGenericType.ShouldBeFalse();
        }

        [Fact]
        public void Emited_type_should_have_two_properties()
        {
            emitedType.GetProperties().Length.ShouldBe(2);
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
