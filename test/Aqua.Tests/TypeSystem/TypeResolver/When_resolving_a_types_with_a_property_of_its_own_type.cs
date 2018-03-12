// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Tests.TypeSystem.TypeResolver
{
    using Aqua.TypeSystem;
    using Shouldly;
    using System;
    using Xunit;

    public class When_resolving_a_types_with_a_property_of_its_own_type
    {
        private class A
        {
            public A P { get; set; }
        }

        private readonly Type originalType;
        private readonly Type resolvedType;

        public When_resolving_a_types_with_a_property_of_its_own_type()
        {
            originalType = typeof(A);

            var typeInfo = new TypeInfo(originalType);

            resolvedType = new TypeResolver().ResolveType(typeInfo);
        }

        [Fact]
        public void Resolved_types_should_be_equal_to_original_type()
        {
            resolvedType.ShouldBe(originalType);
        }
    }
}
