// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Tests.TypeSystem.TypeResolver
{
    using Aqua.TypeSystem;
    using Shouldly;
    using System;
    using Xunit;

    public class When_resolving_types_with_circular_reference
    {
        private class A
        {
            public B B { get; set; }
        }

        private class B
        {
            public A A { get; set; }
        }

        private interface IOrigin<T>
        {
        }

        private class Egg : Egg<Chicken>, IOrigin<Chicken>
        {
        }

        private class Chicken : Chicken<Egg>, IOrigin<Egg>
        {
        }

        private class Egg<T> : IOrigin<T>
        {
        }

        private class Chicken<T> : IOrigin<T>
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

            var resolverdA = typeResolver.ResolveType(new TypeInfo(typeA));
            var resolverdB = typeResolver.ResolveType(new TypeInfo(typeB));
            var resolverdA2 = typeResolver.ResolveType(new TypeInfo(typeA));

            resolverdA.ShouldBe(typeA);
            resolverdA.ShouldNotBe(typeB);
            resolverdA.ShouldBe(resolverdA2);
            resolverdB.ShouldBe(typeB);
        }

        [Fact]
        public void Resolved_types_with_anonymous_generic_argument_should_be_original()
        {
            Type type1 = TestObjects1.Helper.GetAnonymousType1();
            Type type2 = TestObjects2.Helper.GetAnonymousType1();

            var chickeOne = typeof(Chicken<>).MakeGenericType(type1);
            var chickeTwo = typeof(Chicken<>).MakeGenericType(type2);

            var typeResolver = new TypeResolver();

            var resolverdChickenOne = typeResolver.ResolveType(new TypeInfo(chickeOne));
            var resolverdChickenTwo = typeResolver.ResolveType(new TypeInfo(chickeTwo));

            resolverdChickenOne.ShouldBe(chickeOne);
            resolverdChickenTwo.ShouldBe(chickeTwo);
            resolverdChickenOne.ShouldNotBe(resolverdChickenTwo);
        }
    }
}