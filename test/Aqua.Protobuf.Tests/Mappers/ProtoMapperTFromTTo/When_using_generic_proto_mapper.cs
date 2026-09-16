// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Protobuf.Tests.Mappers.ProtoMapperTFromTTo;

using Aqua.Protobuf.Mappers;
using Proto = Aqua.Protobuf.Schema;

public sealed class When_using_generic_proto_mapper
{
    [Fact]
    public void Should_dispatch_to_typed_methods_through_non_generic_interface()
    {
        IProtoMapper<string> mapper = new StringMapper();
        var context = ProtoContext.ForWrite();

        var proto = mapper.ToProto("aqua", context);
        var result = mapper.FromProto(proto, ProtoContext.ForRead());

        proto.ShouldBeOfType<Proto.Value>().String.ShouldBe("aqua");
        result.ShouldBe("aqua");
    }

    private sealed class StringMapper : ProtoMapper<string, Proto.Value>
    {
        public override string FromProto(Proto.Value proto, ProtoContext context) => proto.String;

        public override Proto.Value ToProto(string value, ProtoContext context) => new() { String = value };
    }
}
