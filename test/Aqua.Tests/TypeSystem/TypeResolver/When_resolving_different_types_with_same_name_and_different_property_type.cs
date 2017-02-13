// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Tests.TypeSystem.TypeResolver
{
    using Aqua.TypeSystem;
    using Shouldly;
    using System;
    using Xunit;

    public class When_resolving_different_types_with_same_name_and_different_property_type
    {
        private readonly Type type1;
        private readonly Type type2;

        private readonly TypeInfo typeInfo1;
        private readonly TypeInfo typeInfo2;

        private readonly Type resolvedType1;
        private readonly Type resolvedType2;

        public When_resolving_different_types_with_same_name_and_different_property_type()
        {
            type1 = TestObjects1.Helper.GetCustomType2();
            type2 = TestObjects2.Helper.GetCustomType2();

            typeInfo1 = new TypeInfo(type1);
            typeInfo2 = new TypeInfo(type2);

            var typeResolver = new TypeResolver();
            resolvedType1 = typeResolver.ResolveType(typeInfo1);
            resolvedType2 = typeResolver.ResolveType(typeInfo2);
        }

        [Fact]
        public void Type_infos_should_not_be_equal()
        {
            typeInfo1.ShouldNotBe(typeInfo2);
        }

        [Fact]
        public void Resolved_types_should_not_be_equal()
        {
            resolvedType1.ShouldNotBe(resolvedType2);
        }

        [Fact]
        public void Resolved_types_should_be_equal_to_original_types()
        {
            resolvedType1.ShouldBe(type1);
            resolvedType2.ShouldBe(type2);
        }
    }
}
