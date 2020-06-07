// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Tests.TypeSystem.ConstructorInfo
{
    using Aqua.Tests.Serialization;
    using Aqua.TypeSystem;
    using Shouldly;
    using System;
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

        public class Overload
        {
            public Overload()
            {
            }

            public Overload(string s)
            {
            }
        }

        public class NoDefaulConstructor
        {
            public NoDefaulConstructor(string s)
            {
            }
        }

        [Fact]
        public void Should_throw_upon_casting_constructor_info_for_inexistent_constructor()
        {
            var constructorInfo = new ConstructorInfo("Constructor", typeof(A));
            ShouldThrowOnResolve(constructorInfo);
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
        public void Should_throw_upon_resolve_type_initializer_missing_isstatic_set_to_true()
        {
            var constructorInfo = new ConstructorInfo(".cctor", typeof(A)) { IsStatic = null };
            ShouldThrowOnResolve(constructorInfo);
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

        [Fact]
        public void Should_resolve_constructor_created_by_memberinfo()
        {
            var expected = typeof(Overload).GetConstructor(Array.Empty<Type>());
            var ctor = new ConstructorInfo(expected);
            var ctor2 = JsonSerializationHelper.Serialize(ctor);
            ctor2.ToConstructorInfo().ShouldBeSameAs(expected);
        }

        [Fact]
        public void Should_resolve_default_constructor_created_by_name()
        {
            var ctor = new ConstructorInfo(".ctor", typeof(Overload));
            var expected = typeof(Overload).GetConstructor(Array.Empty<Type>());
            ctor.ToConstructorInfo().ShouldBeSameAs(expected);
        }

        [Fact]
        public void Should_resolve_default_constructor_created_by_name_with_empty_parameter_list()
        {
            var ctor = new ConstructorInfo(".ctor", typeof(Overload), Array.Empty<Type>());
            var expected = typeof(Overload).GetConstructor(Array.Empty<Type>());
            ctor.ToConstructorInfo().ShouldBeSameAs(expected);
        }

        [Fact]
        public void Should_resolve_default_constructor_created_by_name_with_parameter_list()
        {
            var ctor = new ConstructorInfo(".ctor", typeof(Overload), new[] { typeof(string) });
            var expected = typeof(Overload).GetConstructor(new[] { typeof(string) });
            ctor.ToConstructorInfo().ShouldBeSameAs(expected);
        }

        [Fact]
        public void Should_throw_on_resolve_non_default_constructor_with_no_parameter_list()
        {
            var ctor = new ConstructorInfo(".ctor", typeof(NoDefaulConstructor));
            ShouldThrowOnResolve(ctor);
        }

        private static void ShouldThrowOnResolve(ConstructorInfo constructorInfo)
            => Should.Throw<TypeResolverException>(() => _ = (System.Reflection.ConstructorInfo)constructorInfo)
            .Message.ShouldBe("Failed to resolve constructor, consider using extension method to specify ITypeResolver.");
    }
}
