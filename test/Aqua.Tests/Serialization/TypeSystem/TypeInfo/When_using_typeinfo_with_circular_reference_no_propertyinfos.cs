// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Tests.Serialization.TypeSystem.TypeInfo
{
    using Aqua.TypeSystem;
    using Shouldly;
    using System;
    using Xunit;

    public abstract partial class When_using_typeinfo_with_circular_reference_no_propertyinfos
    {
        private abstract class A
        {
            public int Number { get; set; }
        }

        private class C<T> : A
        {
            public T Reference { get; set; }
        }

        private class X
        {
        }

        private readonly TypeInfo serializedTypeInfo;

        protected When_using_typeinfo_with_circular_reference_no_propertyinfos(Func<TypeInfo, TypeInfo> serialize)
        {
            var typeInfo = new TypeInfo(typeof(C<X>), false);

            serializedTypeInfo = serialize(typeInfo);
        }

        [Fact]
        public void Type_info_should_have_typename()
        {
            serializedTypeInfo.Name.ShouldBe("C`1");
        }

        [Fact]
        public void Type_info_should_be_generic()
        {
            serializedTypeInfo.IsGenericType.ShouldBeTrue();
        }
    }
}
