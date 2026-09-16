// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.MessagePack.Tests.DeserializationReferenceTracker;

using Aqua.MessagePack;
using System.Text;

public class When_using_deserialization_reference_tracker
{
    private class Entity
    {
        public string Name { get; set; }
    }

    private sealed class DerivedEntity : Entity
    {
    }

    // -------------------------------------------------------------------------
    // Register
    // -------------------------------------------------------------------------

    [Fact]
    public void Register_should_store_value_for_id()
    {
        var tracker = new DeserializationReferenceTracker();
        var value = new Entity { Name = "x" };

        tracker.Register(value, 1);

        var result = tracker.Resolve<Entity>(1);
        result.ShouldBeSameAs(value);
    }

    [Fact]
    public void Register_with_default_id_should_not_store_value()
    {
        var tracker = new DeserializationReferenceTracker();
        var value = new Entity { Name = "x" };

        tracker.Register(value, 0);

        // id 0 is not valid; resolving it must throw, proving the register no-op
        Should.Throw<MessagePackSerializationException>(() => tracker.Resolve<Entity>(0));
    }

    [Fact]
    public void Register_with_same_id_should_overwrite_value()
    {
        var tracker = new DeserializationReferenceTracker();
        var first = new Entity { Name = "first" };
        var second = new Entity { Name = "second" };

        tracker.Register(first, 7);
        tracker.Register(second, 7);

        var result = tracker.Resolve<Entity>(7);
        result.ShouldBeSameAs(second);
        result.Name.ShouldBe("second");
    }

    [Fact]
    public void Register_should_accept_null_value()
    {
        var tracker = new DeserializationReferenceTracker();

        tracker.Register<Entity>(null, 3);

        var result = tracker.Resolve<Entity>(3);
        result.ShouldBeNull();
    }

    // -------------------------------------------------------------------------
    // Resolve - happy path
    // -------------------------------------------------------------------------

    [Fact]
    public void Resolve_should_return_registered_value_of_matching_type()
    {
        var tracker = new DeserializationReferenceTracker();
        var value = new Entity { Name = "match" };
        tracker.Register(value, 11);

        var result = tracker.Resolve<Entity>(11);

        result.ShouldBeSameAs(value);
    }

    [Fact]
    public void Resolve_should_return_value_as_base_type_when_assignable()
    {
        var tracker = new DeserializationReferenceTracker();
        var value = new DerivedEntity { Name = "derived" };
        tracker.Register(value, 12);

        var result = tracker.Resolve<Entity>(12);

        result.ShouldBeSameAs(value);
    }

    // -------------------------------------------------------------------------
    // Resolve - edge cases and exceptions
    // -------------------------------------------------------------------------

    [Theory]
    [InlineData(0)]
    public void Resolve_with_invalid_id_should_throw_MessagePackSerializationException(int id)
    {
        var tracker = new DeserializationReferenceTracker();

        var ex = Should.Throw<MessagePackSerializationException>(() => tracker.Resolve<Entity>(id));
        ex.Message.ShouldContain($"Reference {id} is not valid");
    }

    [Theory]
    [InlineData(1)]
    [InlineData(-1)]
    [InlineData(int.MaxValue)]
    public void Resolve_with_unregistered_id_should_throw_MessagePackSerializationException(int id)
    {
        var tracker = new DeserializationReferenceTracker();

        var ex = Should.Throw<MessagePackSerializationException>(() => tracker.Resolve<Entity>(id));
        ex.Message.ShouldContain($"Reference {id} is not registered");
    }

    [Fact]
    public void Resolve_with_incompatible_type_should_throw_InvalidCastException()
    {
        var tracker = new DeserializationReferenceTracker();
        tracker.Register("a string value", 21);

        var ex = Should.Throw<InvalidCastException>(() => tracker.Resolve<StringBuilder>(21));
        ex.Message.ShouldContain(nameof(StringBuilder));
        ex.Message.ShouldContain(nameof(String));
    }

    [Fact]
    public void Resolve_with_registered_null_should_return_null()
    {
        var tracker = new DeserializationReferenceTracker();
        tracker.Register<object>(null, 22);

        var result = tracker.Resolve<object>(22);

        result.ShouldBeNull();
    }
}
