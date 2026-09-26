// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Tests.Dynamic.DynamicObjectMapper;

using Aqua.Dynamic;

public class When_mapping_cyclic_reference
{
    [Fact]
    public void Should_map_and_restore_references_AB()
    {
        var orig = new A(new B());
        orig.B.A = orig;

        var d = DynamicObject.Create(orig);

        var a = d.CreateObject<A>();

        a.B.A.ShouldBeEquivalentTo(a);
    }

    [Fact]
    public void Should_map_and_restore_references_BA()
    {
        var orig = new B();
        orig.A = new A(orig);

        var d = DynamicObject.Create(orig);

        var b = d.CreateObject<B>();

        b.A.B.ShouldBeEquivalentTo(b);
    }

    private sealed record class A(B B);

    private sealed record class B
    {
        public A A { get; set; }
    }
}
