// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.MessagePack.Tests.SerializationReferenceTracker;

using Aqua.MessagePack;

public class When_using_serialization_reference_tracker
{
    private sealed class Entity
    {
        public string Name { get; set; }
    }

    // -------------------------------------------------------------------------
    // Constructor / ReferenceHandler
    // -------------------------------------------------------------------------

    [Theory]
    [InlineData(ReferenceHandler.Unspecified)]
    [InlineData(ReferenceHandler.Preserve)]
    [InlineData(ReferenceHandler.IgnoreCycles)]
    public void Constructor_should_expose_reference_handler(ReferenceHandler mode)
    {
        var tracker = new SerializationReferenceTracker(mode);

        tracker.ReferenceHandler.ShouldBe(mode);
    }

    [Fact]
    public void Constructor_with_unsupported_mode_should_throw_InvalidOperationException()
    {
        var ex = Should.Throw<InvalidOperationException>(() => new SerializationReferenceTracker((ReferenceHandler)999));
        ex.Message.ShouldContain("not supported");
    }

    // -------------------------------------------------------------------------
    // Preserve
    // -------------------------------------------------------------------------

    [Fact]
    public void Preserve_tryregister_should_write_new_values_with_incrementing_ids()
    {
        var tracker = new SerializationReferenceTracker(ReferenceHandler.Preserve);
        var first = new Entity { Name = "first" };
        var second = new Entity { Name = "second" };

        var writeFirst = tracker.TryRegister(first, out var firstId);
        var writeSecond = tracker.TryRegister(second, out var secondId);

        writeFirst.ShouldBeTrue();
        writeSecond.ShouldBeTrue();
        firstId.ShouldBe(1);
        secondId.ShouldBe(2);
    }

    [Fact]
    public void Preserve_tryregister_should_not_rewrite_shared_reference_but_return_original_id()
    {
        var tracker = new SerializationReferenceTracker(ReferenceHandler.Preserve);
        var shared = new Entity { Name = "shared" };

        var writeFirst = tracker.TryRegister(shared, out var firstId);
        var writeSecond = tracker.TryRegister(shared, out var secondId);

        writeFirst.ShouldBeTrue();
        writeSecond.ShouldBeFalse();
        firstId.ShouldBe(1);
        secondId.ShouldBe(1);
    }

    [Fact]
    public void Preserve_tryregister_should_track_references_not_value_equality()
    {
        var tracker = new SerializationReferenceTracker(ReferenceHandler.Preserve);
        var first = new Entity { Name = "same" };
        var second = new Entity { Name = "same" };

        var writeFirst = tracker.TryRegister(first, out var firstId);
        var writeSecond = tracker.TryRegister(second, out var secondId);

        // equal content but distinct references must be tracked independently
        writeFirst.ShouldBeTrue();
        writeSecond.ShouldBeTrue();
        firstId.ShouldBe(1);
        secondId.ShouldBe(2);
    }

    [Fact]
    public void Preserve_scope_should_return_null_disposable()
    {
        var tracker = new SerializationReferenceTracker(ReferenceHandler.Preserve);

        var scope = tracker.Scope();

        scope.ShouldBeNull();
    }

    [Fact]
    public void Preserve_should_detect_shared_reference_across_sibling_scopes()
    {
        var tracker = new SerializationReferenceTracker(ReferenceHandler.Preserve);
        var shared = new Entity { Name = "shared" };

        // Scope() is a no-op for Preserve, so tracking is global and flat:
        // a reference shared by two siblings is still detected.
        tracker.TryRegister(shared, out var firstId).ShouldBeTrue();

        tracker.TryRegister(shared, out var secondId).ShouldBeFalse();
        secondId.ShouldBe(firstId);
    }

    // -------------------------------------------------------------------------
    // Unspecified
    // -------------------------------------------------------------------------

    [Fact]
    public void Unspecified_tryregister_should_write_value_with_zero_id()
    {
        var tracker = new SerializationReferenceTracker(ReferenceHandler.Unspecified);
        var value = new Entity { Name = "x" };

        var write = tracker.TryRegister(value, out var id);

        write.ShouldBeTrue();
        id.ShouldBe(0);
    }

    [Fact]
    public void Unspecified_tryregister_should_throw_on_circular_reference_within_scope()
    {
        var tracker = new SerializationReferenceTracker(ReferenceHandler.Unspecified);
        var value = new Entity { Name = "cycle" };

        using (tracker.Scope())
        {
            tracker.TryRegister(value, out _).ShouldBeTrue();

            var ex = Should.Throw<InvalidOperationException>(() => tracker.TryRegister(value, out _));
            ex.Message.ShouldContain("cycle");
        }
    }

    [Fact]
    public void Unspecified_should_not_flag_same_reference_across_sibling_scopes()
    {
        var tracker = new SerializationReferenceTracker(ReferenceHandler.Unspecified);
        var value = new Entity { Name = "sibling" };

        using (tracker.Scope())
        {
            tracker.TryRegister(value, out _).ShouldBeTrue();
        }

        // after the scope is disposed the ancestor chain is restored,
        // so the same reference in a sibling scope is not a cycle
        using (tracker.Scope())
        {
            tracker.TryRegister(value, out _).ShouldBeTrue();
        }
    }

    // -------------------------------------------------------------------------
    // IgnoreCycles
    // -------------------------------------------------------------------------

    [Fact]
    public void IgnoreCycles_tryregister_should_write_value_with_zero_id()
    {
        var tracker = new SerializationReferenceTracker(ReferenceHandler.IgnoreCycles);
        var value = new Entity { Name = "x" };

        var write = tracker.TryRegister(value, out var id);

        write.ShouldBeTrue();
        id.ShouldBe(0);
    }

    [Fact]
    public void IgnoreCycles_tryregister_should_suppress_circular_reference_within_scope()
    {
        var tracker = new SerializationReferenceTracker(ReferenceHandler.IgnoreCycles);
        var value = new Entity { Name = "cycle" };

        using (tracker.Scope())
        {
            tracker.TryRegister(value, out _).ShouldBeTrue();

            // back-edge to an ancestor is suppressed rather than throwing
            tracker.TryRegister(value, out _).ShouldBeFalse();
        }
    }

    [Fact]
    public void IgnoreCycles_should_not_flag_same_reference_across_sibling_scopes()
    {
        var tracker = new SerializationReferenceTracker(ReferenceHandler.IgnoreCycles);
        var value = new Entity { Name = "sibling" };

        using (tracker.Scope())
        {
            tracker.TryRegister(value, out _).ShouldBeTrue();
        }

        using (tracker.Scope())
        {
            tracker.TryRegister(value, out _).ShouldBeTrue();
        }
    }
}
