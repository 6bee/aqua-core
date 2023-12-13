// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Tests.TypeSystem.MethodInfo;

using Aqua.Tests.Serialization;
using Aqua.TypeSystem;
using Shouldly;
using System.Linq;
using Xunit;
using BindingFlags = System.Reflection.BindingFlags;

public class When_resolving_method
{
    private class A
    {
        public static string Method() => null;

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

    private static class StaticClass
    {
        public static string Method() => null;
    }

    public class Overload
    {
        public string Method() => null;

        public string Method(string s) => null;
    }

    public class NonEmptyParameterList
    {
        public string Method(string s) => null;
    }

    [Fact]
    public void Should_throw_upon_casting_method_info_for_inexistent_method()
    {
        var methodInfo = new MethodInfo("METHOD", typeof(A));
        ShouldThrowOnResolve(methodInfo);
    }

    [Fact]
    public void Should_throw_upon_casting_method_info_for_unknown_declaring_type()
    {
        var methodInfo = new MethodInfo(nameof(A.Method), new TypeInfo { Name = "Unknown" });
        ShouldThrowOnResolve(methodInfo, "Declaring type 'Unknown' could not be reconstructed for method Unknown.Method()");
    }

    [Fact]
    public void Should_resolve_method()
    {
        var methodInfo = new MethodInfo(nameof(A.Method), typeof(A), new[] { typeof(int) });
        var method = (System.Reflection.MethodInfo)methodInfo;
        var expected = typeof(A)
            .GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .Single(x => x.Name == nameof(A.Method) && !x.IsGenericMethod);
        method.ShouldBeSameAs(expected);
    }

    [Fact]
    public void Should_resolve_static_method()
    {
        var methodInfo = new MethodInfo(nameof(A.Method), typeof(A)) { IsStatic = true };
        var method = (System.Reflection.MethodInfo)methodInfo;
        var expected = typeof(A)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Single(x => x.Name == nameof(A.Method));
        method.ShouldBeSameAs(expected);
    }

    [Fact]
    public void Should_resolve_static_class_method()
    {
        var methodInfo = new MethodInfo(nameof(StaticClass.Method), typeof(StaticClass)) { IsStatic = true };
        var method = (System.Reflection.MethodInfo)methodInfo;
        var expected = typeof(StaticClass).GetMethod(nameof(StaticClass.Method), BindingFlags.Public | BindingFlags.Static);
        method.ShouldBeSameAs(expected);
    }

    [Fact]
    public void Should_throw_on_resolve_static_class_method_missing_is_static_set_true()
    {
        var methodInfo = new MethodInfo(nameof(StaticClass.Method), typeof(StaticClass));
        ShouldThrowOnResolve(methodInfo);
    }

    [Fact]
    public void Should_resolve_generic_method()
    {
        var methodInfo = new MethodInfo(nameof(A.Method), typeof(A), new[] { typeof(byte) }, new[] { typeof(byte) });
        var method = (System.Reflection.MethodInfo)methodInfo;
        var expected = typeof(A)
            .GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .Single(x => x.Name == nameof(A.Method) && x.IsGenericMethod)
            .MakeGenericMethod(typeof(byte));
        method.ShouldBeSameAs(expected);
    }

    [Fact]
    public void Should_resolve_method_of_base_type()
    {
        var methodInfo = new MethodInfo(nameof(A.Method), typeof(Subtype), new[] { typeof(int) });
        var method = (System.Reflection.MethodInfo)methodInfo;
        var expected = typeof(Subtype)
            .GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .Single(x => x.Name == nameof(A.Method) && !x.IsGenericMethod);
        method.ShouldBeSameAs(expected);
    }

    [Fact]
    public void Should_resolve_method_override_of_sub_type()
    {
        var methodInfo = new MethodInfo(nameof(A.Method), typeof(Subtype), new[] { typeof(byte) }, new[] { typeof(byte) });
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
        var methodInfo = new MethodInfo(nameof(A.Method), typeof(SubSubtype), new[] { typeof(byte) }, new[] { typeof(byte) });
        var method = (System.Reflection.MethodInfo)methodInfo;
        var expected = typeof(SubSubtype)
            .GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .Single(x => x.Name == nameof(A.Method) && x.IsGenericMethod)
            .MakeGenericMethod(typeof(byte));
        method.ShouldBeSameAs(expected);
    }

    [Fact]
    public void Should_resolve_method_created_by_memberinfo()
    {
        var expected = typeof(Overload)
            .GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .Single(x => x.Name == nameof(Overload.Method) && x.GetParameters().Length is 1);

        var methodInfo = new MethodInfo(expected);

        var methodInfo2 = NewtonsoftJsonSerializationHelper.Clone(methodInfo);

        methodInfo2.ToMethodInfo().ShouldBeSameAs(expected);
    }

    [Fact]
    public void Should_resolve_method_created_by_name()
    {
        var methodInfo = new MethodInfo(nameof(Overload.Method), typeof(Overload));

        var expected = typeof(Overload)
            .GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .Single(x => x.Name == nameof(Overload.Method) && x.GetParameters().Length is 0);

        methodInfo.ToMethodInfo().ShouldBeSameAs(expected);
    }

    [Fact]
    public void Should_throw_on_resolve_parameterized_method_with_no_parameter_list()
    {
        var methodInfo = new MethodInfo(nameof(NonEmptyParameterList.Method), typeof(NonEmptyParameterList));
        ShouldThrowOnResolve(methodInfo);
    }

    private static void ShouldThrowOnResolve(MethodInfo methodInfo, string expectedExceptionMessage = "Failed to resolve method, consider using extension method to specify ITypeResolver.")
        => Should.Throw<TypeResolverException>(() =>
        {
            _ = (System.Reflection.MethodInfo)methodInfo;
        }).Message.ShouldBe(expectedExceptionMessage);
}