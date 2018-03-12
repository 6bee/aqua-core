// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Tests.TypeSystem.TypeInfo
{
    using Aqua.TypeSystem;
    using Shouldly;
    using System.Linq;
    using Xunit;

    public class When_creating_type_info_of_generic_type
    {
        private class A<T>
        {
            public T Value { get; set; }
        }

        private class B
        {
            public string StringValue { get; set; }
        }

        private readonly TypeInfo typeInfo;

        public When_creating_type_info_of_generic_type()
        {
            typeInfo = new TypeInfo(typeof(A<B>));
        }

        [Fact]
        public void Type_info_should_have_is_array_false()
        {
            typeInfo.IsArray.ShouldBeFalse();
        }

        [Fact]
        public void Type_info_should_have_is_generic__type_true()
        {
            typeInfo.IsGenericType.ShouldBeTrue();
        }

        [Fact]
        public void Type_info_should_have_is_generic_type_definition_false()
        {
            typeInfo.IsGenericTypeDefinition.ShouldBeFalse();
        }

        [Fact]
        public void Type_info_should_have_is_nested_true()
        {
            typeInfo.IsNested.ShouldBeTrue();
        }

        [Fact]
        public void Type_info_name_should_have_array_brackets()
        {
            typeInfo.Name.ShouldBe("A`1");
        }

        [Fact]
        public void Type_info_should_contain_generic_property()
        {
            typeInfo.Properties.Single().Name.ShouldBe("Value");
            typeInfo.Properties.Single().PropertyType.Type.ShouldBe(typeof(B));
        }

        [Fact]
        public void Type_info_should_contain_generic_argument_type()
        {
            typeInfo.GenericArguments.Single().Name.ShouldBe("B");
        }

        [Fact]
        public void Generic_argument_type_should_contain_property()
        {
            typeInfo.GenericArguments.Single().Properties.Single().Name.ShouldBe("StringValue");
        }
    }
}
