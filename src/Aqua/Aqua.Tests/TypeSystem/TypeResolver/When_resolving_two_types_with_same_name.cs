// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Tests.TypeSystem.TypeResolver
{
    using Aqua.TypeSystem;
    using System;
    using Xunit;
    using Xunit.Should;

    public class When_resolving_two_types_with_same_name
    {
        class A
        {
            public int Int32Value { get; set; }

            public string StringValue { get; set; }
        }

        private readonly Type resolvedTypeA1;
        private readonly Type resolvedTypeA2;
        private readonly Type resolvedTypeB1;
        private readonly Type resolvedTypeB2;

        public When_resolving_two_types_with_same_name()
        {
            var a = new A { Int32Value = 0, StringValue = "" };
            var b = new { Int32Value = 0, StringValue = "" };

            var typeInfoA = new TypeInfo(a.GetType());
            var typeInfoB = new TypeInfo(b.GetType());

            typeInfoA.Name = "TestTypeName";
            typeInfoB.Name = "TestTypeName";

            typeInfoA.Namespace = "TestNamespace";
            typeInfoB.Namespace = "TestNamespace";

            typeInfoA.DeclaringType = null;
            typeInfoB.DeclaringType = null;

            var typeResolver = new TypeResolver();

            resolvedTypeA1 = typeResolver.ResolveType(typeInfoA);
            resolvedTypeB1 = typeResolver.ResolveType(typeInfoB);

            resolvedTypeA2 = typeResolver.ResolveType(typeInfoA);
            resolvedTypeB2 = typeResolver.ResolveType(typeInfoB);
        }

        [Fact]
        public void Resolved_type_A_should_be_same()
        {
            resolvedTypeA1.ShouldBeSameAs(resolvedTypeA2);
        }

        [Fact]
        public void Resolved_type_B_should_be_same()
        {
            resolvedTypeB1.ShouldBeSameAs(resolvedTypeB2);
        }

        [Fact]
        public void Resolved_type_A_and_B_should_be_different()
        {
            resolvedTypeA1.ShouldNotBe(resolvedTypeB1);
        }
    }
}
