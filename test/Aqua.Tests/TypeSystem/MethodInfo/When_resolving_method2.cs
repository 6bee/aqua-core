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
    public void FindConflictingTypeDefinitions()
    {
        var m = typeof(TestClass).GetMethodEx(nameof(TestClass.MethodWithValueTaskReturnType));

        var t1 = typeof(ValueTask<>).MakeGenericType(typeof(int));
        var t2 = new TypeInfo(t1).ResolveType(new TypeResolver());
        var t3 = new TypeInfo(t2).ResolveType(new TypeResolver());

        var result1 = t1 == m.ReturnType;
        var result2 = t2 == m.ReturnType;
        var result3 = t2 == t3;

        // NOTE: xunit.runner.visualstudio 3.0.0 provides extra definition of ValueTask<> and causes conflict
        // System.Threading.Tasks.ValueTask`1, System.Threading.Tasks.Extensions, Version = 4.2.0.1, Culture = neutral, PublicKeyToken = cc7b13ffcd2ddd51
        // System.Threading.Tasks.ValueTask`1, xunit.runner.visualstudio.testadapter, Version = 3.0.0.0, Culture = neutral, PublicKeyToken = null
        var s1 = t1.GetGenericTypeDefinition().AssemblyQualifiedName;
        var s2 = t2.GetGenericTypeDefinition().AssemblyQualifiedName;

        result1.ShouldBeTrue();
        result2.ShouldBeTrue();
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