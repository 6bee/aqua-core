// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Protobuf.Tests.ContextExtensions;

using Aqua.Protobuf.Schema;
using Google.Protobuf.WellKnownTypes;

public class When_creating_a_reference_proto_for_a_null_value
{
    [Fact]
    public void Should_create_a_null_proto()
    {
        var context = ProtoContext.ForWrite();

        var proto = context.ToReferenceProto<TestReferenceProto, TestValueProto, TestValue>(null, (_, _, _) => throw new InvalidOperationException());

        proto.Null.ShouldBe(NullValue.NullValue);
    }
}

public class When_creating_a_reference_proto_for_a_new_value
{
    [Fact]
    public void Should_create_and_populate_a_value_proto()
    {
        var context = ProtoContext.ForWrite(new ProtoOptions { ReferenceHandler = ReferenceHandler.Preserve });
        var value = new TestValue { Name = "aqua" };

        var proto = context.ToReferenceProto<TestReferenceProto, TestValueProto, TestValue>(value, (target, source, _) => target.Name = source.Name);

        proto.Value.Id.ShouldBe(1U);
        proto.Value.Name.ShouldBe("aqua");
    }
}

public class When_creating_a_reference_proto_for_a_registered_value
{
    [Fact]
    public void Should_create_a_reference_proto_when_preserving_references()
    {
        var context = ProtoContext.ForWrite(new ProtoOptions { ReferenceHandler = ReferenceHandler.Preserve });
        var value = new TestValue();
        context.ToReferenceProto<TestReferenceProto, TestValueProto, TestValue>(value, (_, _, _) => { });

        var proto = context.ToReferenceProto<TestReferenceProto, TestValueProto, TestValue>(value, (_, _, _) => { });

        proto.Ref.Id.ShouldBe(1U);
    }
}

public class When_resolving_a_reference_proto
{
    [Fact]
    public void Should_return_the_registered_reference()
    {
        var context = ProtoContext.ForRead();
        var value = context.Resolve<TestValue, TestValueProto>(new TestValueProto { Id = 7 }, (target, _, _) => target.Name = "aqua");

        context.Resolve<TestValue>(new Ref { Id = 7 }).ShouldBeSameAs(value);
        value.Name.ShouldBe("aqua");
    }
}

internal sealed class TestValue
{
    public string Name { get; set; }
}

internal sealed class TestValueProto : IHaveId
{
    public uint Id { get; set; }

    public string Name { get; set; }
}

internal sealed class TestReferenceProto : IReferenceProto<TestValueProto>
{
    public NullValue Null { get; set; }

    public TestValueProto Value { get; set; }

    public Ref Ref { get; set; }
}
