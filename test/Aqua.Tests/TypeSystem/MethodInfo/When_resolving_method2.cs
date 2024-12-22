// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Tests.TypeSystem.MethodInfo;

using Aqua.Tests.TestObjects;
using Aqua.TypeExtensions;
using Aqua.TypeSystem;
using Shouldly;
using System;
using System.Threading.Tasks;
using Xunit;

public class When_resolving_method2
{
    private class TestClass
    {
        public int? MethodWithNullableReturnType() => throw new NotImplementedException();

        public ValueTask<int> MethodWithValueTaskReturnType() => throw new NotImplementedException();

        public Task<int> MethodWithTaskReturnType() => throw new NotImplementedException();

        public Value<int> MethodWithCustomStructReturnType() => throw new NotImplementedException();

        public void MethodWithValueTaskArgument(ValueTask<int> task) => throw new NotImplementedException();

        public void MethodWithTaskArgument(Task<int> task) => throw new NotImplementedException();

        public void MethodWithCustomStructArgument(Value<int> task) => throw new NotImplementedException();
    }

    [Fact]
    public void Should_resolve_valuetask()
    {
        var m = typeof(TestClass).GetMethodEx(nameof(TestClass.MethodWithValueTaskReturnType));
        var resolvedType = new TypeInfo(m.ReturnType).ResolveType(new TypeResolver());

        // NOTE: xunit.runner.visualstudio 3.0.0 provides extra definition of ValueTask<> (by packing dependencies via ILRepack) and causes conflict
        // System.Threading.Tasks.ValueTask`1, System.Threading.Tasks.Extensions, Version = 4.2.0.1, Culture = neutral, PublicKeyToken = cc7b13ffcd2ddd51
        // System.Threading.Tasks.ValueTask`1, xunit.runner.visualstudio.testadapter, Version = 3.0.0.0, Culture = neutral, PublicKeyToken = null
        resolvedType.ShouldBeEquivalentTo(m.ReturnType);
    }

    [Fact]
    public void Should_resolve_method_with_nullable_return_type()
    {
        var m = typeof(TestClass).GetMethodEx(nameof(TestClass.MethodWithNullableReturnType));
        var method = new MethodInfo(m);

        var resolved = method.ResolveMethod(new TypeResolver());
        resolved.ShouldNotBeNull();
    }

    [Fact]
    public void Should_resolve_method_with_valuetask_return_type()
    {
        var m = typeof(TestClass).GetMethodEx(nameof(TestClass.MethodWithValueTaskReturnType));
        var method = new MethodInfo(m);

        var resolved = method.ResolveMethod(new TypeResolver());
        resolved.ShouldNotBeNull();
    }

    [Fact]
    public void Should_resolve_method_with_task_return_type()
    {
        var m = typeof(TestClass).GetMethodEx(nameof(TestClass.MethodWithTaskReturnType));
        var method = new MethodInfo(m);

        var resolved = method.ResolveMethod(new TypeResolver());
        resolved.ShouldNotBeNull();
    }

    [Fact]
    public void Should_resolve_method_with_customstruct_return_type()
    {
        var m = typeof(TestClass).GetMethodEx(nameof(TestClass.MethodWithCustomStructReturnType));
        var method = new MethodInfo(m);

        var resolved = method.ResolveMethod(new TypeResolver());
        resolved.ShouldNotBeNull();
    }

    [Fact]
    public void Should_resolve_method_with_valuetask_argument()
    {
        var m = typeof(TestClass).GetMethodEx(nameof(TestClass.MethodWithValueTaskArgument));
        var method = new MethodInfo(m);

        var resolved = method.ResolveMethod(new TypeResolver());
        resolved.ShouldNotBeNull();
    }

    [Fact]
    public void Should_resolve_method_with_task_argument()
    {
        var m = typeof(TestClass).GetMethodEx(nameof(TestClass.MethodWithTaskArgument));
        var method = new MethodInfo(m);

        var resolved = method.ResolveMethod(new TypeResolver());
        resolved.ShouldNotBeNull();
    }

    [Fact]
    public void Should_resolve_method_with_customstruct_argument()
    {
        var m = typeof(TestClass).GetMethodEx(nameof(TestClass.MethodWithCustomStructArgument));
        var method = new MethodInfo(m);

        var resolved = method.ResolveMethod(new TypeResolver());
        resolved.ShouldNotBeNull();
    }
}