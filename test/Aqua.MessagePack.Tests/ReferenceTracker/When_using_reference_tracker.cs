// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.MessagePack.Tests.ReferenceTracker;

using Aqua.Dynamic;
using Aqua.MessagePack;
using global::MessagePack;
using System.IO;

public class When_using_reference_tracker
{
    private static T Clone<T>(T graph, ReferenceHandler referenceHandler)
    {
        var options = MessagePackSerializerOptions.Standard
            .ConfigureAqua()
            .With(referenceHandler);

        using var stream = new MemoryStream();
        MessagePackSerializer.Serialize(stream, graph, options);
        stream.Position = 0;
        return MessagePackSerializer.Deserialize<T>(stream, options);
    }

    // -------------------------------------------------------------------------
    // Shared references
    // -------------------------------------------------------------------------

    [Fact]
    public void Preserve_should_restore_shared_reference_identity()
    {
        dynamic shared = new DynamicObject();
        shared.Value = "shared";

        dynamic root = new DynamicObject();
        root.A = shared;
        root.B = shared;

        var result = Clone((DynamicObject)root, ReferenceHandler.Preserve);

        var a = result.Get<DynamicObject>("A");
        var b = result.Get<DynamicObject>("B");

        a.ShouldNotBeNull();
        b.ShouldNotBeNull();
        a.ShouldBeSameAs(b);
    }

    [Fact]
    public void Unspecified_should_not_restore_shared_reference_identity()
    {
        dynamic shared = new DynamicObject();
        shared.Value = "shared";

        dynamic root = new DynamicObject();
        root.A = shared;
        root.B = shared;

        var result = Clone((DynamicObject)root, ReferenceHandler.Unspecified);

        var a = result.Get<DynamicObject>("A");
        var b = result.Get<DynamicObject>("B");

        a.ShouldNotBeNull();
        b.ShouldNotBeNull();
        a.ShouldNotBeSameAs(b);
        a.Get<string>("Value").ShouldBe("shared");
        b.Get<string>("Value").ShouldBe("shared");
    }

    [Fact]
    public void IgnoreCycles_should_not_restore_shared_reference_identity()
    {
        dynamic shared = new DynamicObject();
        shared.Value = "shared";

        dynamic root = new DynamicObject();
        root.A = shared;
        root.B = shared;

        var result = Clone((DynamicObject)root, ReferenceHandler.IgnoreCycles);

        var a = result.Get<DynamicObject>("A");
        var b = result.Get<DynamicObject>("B");

        // IgnoreCycles only suppresses true ancestor-chain cycles, not shared siblings.
        // Both references are written independently; identity is not restored.
        a.ShouldNotBeNull();
        b.ShouldNotBeNull();
        a.ShouldNotBeSameAs(b);
        a.Get<string>("Value").ShouldBe("shared");
        b.Get<string>("Value").ShouldBe("shared");
    }

    // -------------------------------------------------------------------------
    // Circular references
    // -------------------------------------------------------------------------

    [Fact]
    public void Preserve_should_restore_circular_reference()
    {
        dynamic obj0 = new DynamicObject();
        dynamic obj1 = new DynamicObject();
        dynamic obj2 = new DynamicObject();

        obj0.Next = obj1;
        obj1.Next = obj2;
        obj2.Next = obj0;

        var result = Clone((DynamicObject)obj0, ReferenceHandler.Preserve);

        var resolved = result
            .Get<DynamicObject>("Next")
            .Get<DynamicObject>("Next")
            .Get<DynamicObject>("Next");

        resolved.ShouldBeSameAs(result);
    }

    [Fact]
    public void IgnoreCycles_should_break_circular_reference()
    {
        dynamic obj0 = new DynamicObject();
        dynamic obj1 = new DynamicObject();
        dynamic obj2 = new DynamicObject();

        obj0.Next = obj1;
        obj1.Next = obj2;
        obj2.Next = obj0;

        var result = Clone((DynamicObject)obj0, ReferenceHandler.IgnoreCycles);

        var next1 = result.Get<DynamicObject>("Next");
        next1.ShouldNotBeNull();

        var next2 = next1.Get<DynamicObject>("Next");
        next2.ShouldNotBeNull();

        // cycle is broken: the back-reference to obj0 is omitted
        next2.Get<DynamicObject>("Next").ShouldBeNull();
    }

    [Fact]
    public void Unspecified_throws_on_circular_reference()
    {
        dynamic obj0 = new DynamicObject();
        dynamic obj1 = new DynamicObject();

        obj0.Next = obj1;
        obj1.Next = obj0;

        // MessagePack wraps serializer exceptions in MessagePackSerializationException;
        // the root cause is the InvalidOperationException from cycle detection.
        var ex = Should.Throw<MessagePackSerializationException>(() => Clone((DynamicObject)obj0, ReferenceHandler.Unspecified));
        ex.GetBaseException().Message.ShouldContain("cycle");
    }
}
