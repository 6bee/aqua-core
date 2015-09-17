// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Tests.Serialization.TypeSystem.TypeInfo
{
    using Aqua.TypeSystem;
    using Xunit;
    using Xunit.Should;

    public class When_using_typeinfo_with_circular_reference_no_propertyinfos
    {
        abstract class A
        {
            public int Number { get; set; }
        }

        class C<T> : A
        {
            public T Reference { get; set; }
        }

        class X
        {

        }

        TypeInfo serializedTypeInfo;

        public When_using_typeinfo_with_circular_reference_no_propertyinfos()
        {
            var typeInfo = new TypeInfo(typeof(C<X>), false);

            serializedTypeInfo = typeInfo.Serialize();
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
