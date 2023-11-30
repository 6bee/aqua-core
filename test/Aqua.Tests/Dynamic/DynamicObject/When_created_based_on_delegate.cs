// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Tests.Dynamic.DynamicObject;

using Aqua.Dynamic;
using Shouldly;
using System;
using Xunit;

public class When_created_based_on_delegate
{
    public delegate int F0();

    public delegate int F1(int i);

    public int Method0()
    {
        return 1;
    }

    public int Method1(int i)
    {
        return i * i;
    }

    [Fact]
    public void Should_map_action()
    {
#pragma warning disable SA1130 // Use lambda syntax
        Action a = delegate { };
#pragma warning restore SA1130 // Use lambda syntax
        var o = new DynamicObject(a);
        var r = new DynamicObjectMapper().Map<Action>(o);
        r();

        r.Method.ShouldBeSameAs(a.Method);
        r.Target.ShouldNotBeSameAs(a.Target);
    }

    [Fact]
    public void Should_map_action2()
    {
#pragma warning disable SA1130 // Use lambda syntax
        Action<int> a = delegate(int i) { };
#pragma warning restore SA1130 // Use lambda syntax
        var o = new DynamicObject(a);
        var r = new DynamicObjectMapper().Map<Action<int>>(o);
        r(2);

        r.Method.ShouldBeSameAs(a.Method);
        r.Target.ShouldNotBeSameAs(a.Target);
    }

    [Fact]
    public void Should_map_func()
    {
#pragma warning disable SA1130 // Use lambda syntax
        Func<int> f = delegate { return 1; };
#pragma warning restore SA1130 // Use lambda syntax
        var o = new DynamicObject(f);
        var r = new DynamicObjectMapper().Map<Func<int>>(o);
        var x = r();
        x.ShouldBe(1);
    }

    [Fact]
    public void Should_map_func2()
    {
#pragma warning disable SA1130 // Use lambda syntax
        Func<int, int> f = delegate(int i) { return i * i; };
#pragma warning restore SA1130 // Use lambda syntax
        var o = new DynamicObject(f);
        var r = new DynamicObjectMapper().Map<Func<int, int>>(o);
        var x = r(2);
        x.ShouldBe(4);
    }

    [Fact]
    public void Should_map_delegate()
    {
#pragma warning disable SA1130 // Use lambda syntax
        F0 f = delegate { return 1; };
#pragma warning restore SA1130 // Use lambda syntax
        var o = new DynamicObject(f);
        var r = new DynamicObjectMapper().Map<F0>(o);
        var x = r();
        x.ShouldBe(1);
    }

    [Fact]
    public void Should_map_local_function()
    {
        int F() => 1;
        var o = new DynamicObject((Func<int>)F);
        var r = new DynamicObjectMapper().Map<F0>(o);
        var x = r();
        x.ShouldBe(1);
    }

    [Fact]
    public void Should_map_static_local_function()
    {
        static int F() => 1;
        var o = new DynamicObject((Func<int>)F);
        var r = new DynamicObjectMapper().Map<F0>(o);
        var x = r();
        x.ShouldBe(1);
    }

    [Fact]
    public void Should_map_delegate3()
    {
        int F(int i)
        {
            return i * i;
        }

        var o = new DynamicObject((Func<int, int>)F);
        var r = new DynamicObjectMapper().Map<F1>(o);
        var x = r(2);
        x.ShouldBe(4);
    }

    [Fact]
    public void Should_map_delegate4()
    {
        F0 f = Method0;
        var o = new DynamicObject(f);
        var r = new DynamicObjectMapper().Map<F0>(o);
        var x = r();
        x.ShouldBe(1);
    }

    [Fact]
    public void Should_map_delegate5()
    {
        F1 f = Method1;
        var o = new DynamicObject(f);
        var r = new DynamicObjectMapper().Map<F1>(o);
        var x = r(2);
        x.ShouldBe(4);
    }

    [Fact]
    public void May_not_return_value_since_mapping_creates_a_fresh_instance_of_closure()
    {
        var value = 99;
        Func<int> get = () => value;
        var o = new DynamicObject(get);
        var r = new DynamicObjectMapper().Map<Func<int>>(o);
        var x = r();
        x.ShouldBe(0);
    }
}
