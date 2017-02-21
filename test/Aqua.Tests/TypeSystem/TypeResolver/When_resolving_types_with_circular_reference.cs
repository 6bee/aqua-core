// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Tests.TypeSystem.TypeResolver
{
    using Aqua.TypeSystem;
    using Shouldly;
    using System;
    using Xunit;

    public class When_resolving_types_with_circular_reference
    {
        class A
        {
            public B B { get; set; }
        }

        class B
        {
            public A A { get; set; }
        }

        interface IOrigin<T>
        {
        }

        class Egg : Egg<Chicken>, IOrigin<Chicken>
        {
        }

        class Chicken : Chicken<Egg>, IOrigin<Egg>
        {
        }

        class Egg<T> : IOrigin<T>
        {
        }

        class Chicken<T> : IOrigin<T>
        {
        }

        [Theory]
        [InlineData(typeof(A), typeof(B))]
        [InlineData(typeof(Egg), typeof(Chicken))]
        [InlineData(typeof(Egg<Egg>), typeof(Chicken<Chicken>))]
        [InlineData(typeof(Egg<Chicken>), typeof(Chicken<Egg>))]
        public void Resolved_types_should_be_original(Type typeA, Type typeB)
        {
            var typeResolver = new TypeResolver();

            var resolverdA = typeResolver.ResolveType(new Aqua.TypeSystem.TypeInfo(typeA));
            var resolverdB = typeResolver.ResolveType(new Aqua.TypeSystem.TypeInfo(typeB));
            var resolverdA2 = typeResolver.ResolveType(new Aqua.TypeSystem.TypeInfo(typeA));

            resolverdA.ShouldBe(typeA);
            resolverdA.ShouldNotBe(typeB);
            resolverdA.ShouldBe(resolverdA2);
            resolverdB.ShouldBe(typeB);
        }
    }
}