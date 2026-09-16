// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Protobuf.Tests.DeserializationReferenceTracker;

using Aqua.Protobuf;

public class When_registering_a_deserialization_reference
{
    [Fact]
    public void Should_resolve_the_registered_value_and_overwrite_duplicate_ids()
    {
        var tracker = new DeserializationReferenceTracker();
        var first = new object();
        var second = new object();

        tracker.Register(first, 1);
        tracker.Register(second, 1);

        tracker.Resolve<object>(1).ShouldBeSameAs(second);
    }
}

public class When_registering_a_null_deserialization_reference
{
    [Fact]
    public void Should_resolve_to_null()
    {
        var tracker = new DeserializationReferenceTracker();

        tracker.Register<object>(null, 1);

        tracker.Resolve<object>(1).ShouldBeNull();
    }
}

public class When_registering_a_deserialization_reference_with_the_default_id
{
    [Fact]
    public void Should_not_register_the_value()
    {
        var tracker = new DeserializationReferenceTracker();

        tracker.Register(new object(), 0);

        Should.Throw<InvalidOperationException>(() => tracker.Resolve<object>(0)).Message.ShouldContain("Reference 0 is not valid");
    }
}

public class When_resolving_an_unregistered_deserialization_reference
{
    [Fact]
    public void Should_throw_an_invalid_operation_exception()
    {
        var tracker = new DeserializationReferenceTracker();

        Should.Throw<InvalidOperationException>(() => tracker.Resolve<object>(1)).Message.ShouldContain("Reference 1 is not registered");
    }
}

public class When_resolving_a_deserialization_reference_as_an_incompatible_type
{
    [Fact]
    public void Should_throw_an_invalid_cast_exception()
    {
        var tracker = new DeserializationReferenceTracker();
        tracker.Register("value", 1);

        Should.Throw<InvalidCastException>(() => tracker.Resolve<Exception>(1)).Message.ShouldContain(nameof(Exception));
    }
}
