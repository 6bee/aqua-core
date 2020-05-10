// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Tests.TypeSystem.MethodInfo
{
    using Aqua.TypeSystem;
    using Shouldly;
    using System.Linq;
    using Xunit;
    using BindingFlags = System.Reflection.BindingFlags;

    public class When_resolving_method
    {
        private class A
        {
            public string Method(int i) => null;

            public virtual string Method<T>(T t) => null;
        }

        private class Subtype : A
        {
            public override string Method<T>(T t) => base.Method(t);
        }

        private class SubSubtype : Subtype
        {
        }

        [Fact]
        public void Should_throw_upon_casting_method_info_for_inexistent_method()
        {
            var methodInfo = new MethodInfo("method", typeof(A));
            Should.Throw<TypeResolverException>(() =>
            {
                _ = (System.Reflection.MethodInfo)methodInfo;
            }).Message.ShouldBe("Failed to resolve method, consider using extension method to specify ITypeResolver.");
        }

        [Fact]
        public void Should_throw_upon_casting_method_info_for_unknown_declaring_type()
        {
            var methodInfo = new MethodInfo("Method", new TypeInfo { Name = "Unknown" });
            Should.Throw<TypeResolverException>(() =>
            {
                _ = (System.Reflection.MethodInfo)methodInfo;
            }).Message.ShouldBe("Declaring type 'Unknown' could not be reconstructed for method Unknown.Method()");
        }

        [Fact]
        public void Should_resolve_method()
        {
            var methodInfo = new MethodInfo("Method", typeof(A));
            var method = (System.Reflection.MethodInfo)methodInfo;
            var expected = typeof(A)
                .GetMethods()
                .Single(x => x.Name == nameof(A.Method) && !x.IsGenericMethod);
            method.ShouldBeSameAs(expected);
        }

        [Fact]
        public void Should_resolve_generic_method()
        {
            var methodInfo = new MethodInfo("Method", typeof(A), new[] { typeof(byte) });
            var method = (System.Reflection.MethodInfo)methodInfo;
            var expected = typeof(A)
                .GetMethods()
                .Single(x => x.Name == nameof(A.Method) && x.IsGenericMethod)
                .MakeGenericMethod(typeof(byte));
            method.ShouldBeSameAs(expected);
        }

        [Fact]
        public void Should_resolve_method_of_base_type()
        {
            var methodInfo = new MethodInfo("Method", typeof(Subtype));
            var method = (System.Reflection.MethodInfo)methodInfo;
            var expected = typeof(Subtype)
                .GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .Single(x => x.Name == nameof(A.Method) && !x.IsGenericMethod);
            method.ShouldBeSameAs(expected);
        }

        [Fact]
        public void Should_resolve_method_override_of_sub_type()
        {
            var methodInfo = new MethodInfo("Method", typeof(Subtype), new[] { typeof(byte) });
            var method = (System.Reflection.MethodInfo)methodInfo;
            var expected = typeof(Subtype)
                .GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .Single(x => x.Name == nameof(A.Method) && x.IsGenericMethod)
                .MakeGenericMethod(typeof(byte));
            method.ShouldBeSameAs(expected);
        }

        [Fact]
        public void Should_resolve_method_of_sub_subtype()
        {
            var methodInfo = new MethodInfo("Method", typeof(SubSubtype), new[] { typeof(byte) });
            var method = (System.Reflection.MethodInfo)methodInfo;
            var expected = typeof(SubSubtype)
                .GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .Single(x => x.Name == nameof(A.Method) && x.IsGenericMethod)
                .MakeGenericMethod(typeof(byte));
            method.ShouldBeSameAs(expected);
        }
    }
}
