// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Tests.TypeSystem.ConstructorInfo
{
    using Aqua.TypeSystem;
    using Shouldly;
    using System.Linq;
    using Xunit;
    using BindingFlags = System.Reflection.BindingFlags;

    public class When_resolving_constructor
    {
        private class A
        {
            static A()
            {
            }

            public A()
            {
            }

            public A(int i)
            {
            }

            public A(byte b)
            {
            }
        }

        private class Subtype<T> : A
        {
            public Subtype(T t)
            {
            }
        }

        private class SubSubtype : Subtype<int>
        {
            public SubSubtype()
                : base(default)
            {
            }
        }

        [Fact]
        public void Should_throw_upon_casting_constructor_info_for_inexistent_constructor()
        {
            var constructorInfo = new ConstructorInfo("Constructor", typeof(A));
            Should.Throw<TypeResolverException>(() =>
            {
                var x = (System.Reflection.ConstructorInfo)constructorInfo;
            }).Message.ShouldBe("Failed to resolve constructor, consider using extension method to specify ITypeResolver.");
        }

        [Fact]
        public void Should_resolve_type_initializer()
        {
            var constructorInfo = new ConstructorInfo(".cctor", typeof(A));
            var constructor = (System.Reflection.ConstructorInfo)constructorInfo;
            var expected = typeof(A).GetConstructors(BindingFlags.NonPublic | BindingFlags.Static).Single();
            constructor.ShouldBeSameAs(expected);
        }

        [Fact]
        public void Should_resolve_constructor()
        {
            var constructorInfo = new ConstructorInfo(".ctor", typeof(A));
            var constructor = (System.Reflection.ConstructorInfo)constructorInfo;
            var expected = typeof(A)
                .GetConstructors()
                .Single(x => x.GetParameters().Length == 0);
            constructor.ShouldBeSameAs(expected);
        }

        [Fact]
        public void Should_resolve_constructor_in_absence_of_default_ctor()
        {
            var constructorInfo = new ConstructorInfo(".ctor", typeof(Subtype<int>), new[] { typeof(int) });
            var constructor = (System.Reflection.ConstructorInfo)constructorInfo;
            var expected = typeof(Subtype<int>).GetConstructor(new[] { typeof(int) });
            constructor.ShouldBeSameAs(expected);
        }
    }
}
