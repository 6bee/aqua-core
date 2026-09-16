// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.MessagePack.Tests.Formatters;

using Aqua.MessagePack.Formatters;
using Aqua.TypeSystem;
using global::MessagePack;
using System.Buffers;

public class When_using_member_info_formatter
{
    public static IEnumerable<object[]> Members()
    {
        var type = new TypeInfo { Name = "Container" };
        yield return [new FieldInfo("Field", type)];
        yield return [new PropertyInfo("Property", new TypeInfo { Name = "String" }, type)];
        yield return [new MethodInfo("Method", type)];
        yield return [new ConstructorInfo { Name = ".ctor", DeclaringType = type }];
    }

    [Fact]
    public void Null_should_round_trip() => FormatterTestHelper.RoundTrip(MemberInfoFormatter.Instance, default(MemberInfo)).ShouldBeNull();

    [Theory]
    [MemberData(nameof(Members))]
    public void Supported_member_kind_should_round_trip(MemberInfo value)
    {
        var result = FormatterTestHelper.RoundTrip(MemberInfoFormatter.Instance, value);

        result.GetType().ShouldBe(value.GetType());
        result.Name.ShouldBe(value.Name);
    }

    [Fact]
    public void Unknown_member_kind_should_throw()
    {
        var buffer = new ArrayBufferWriter<byte>();
        var writer = new MessagePackWriter(buffer);
        writer.WriteArrayHeader(1);
        writer.Write((byte)255);
        writer.Flush();

        Should.Throw<MessagePackSerializationException>(() => FormatterTestHelper.Decode(MemberInfoFormatter.Instance, buffer.WrittenSpan.ToArray()));
    }

    [Fact]
    public void Empty_array_should_throw()
    {
        var buffer = new ArrayBufferWriter<byte>();
        var writer = new MessagePackWriter(buffer);
        writer.WriteArrayHeader(0);
        writer.Flush();

        Should.Throw<MessagePackSerializationException>(() => FormatterTestHelper.Decode(MemberInfoFormatter.Instance, buffer.WrittenSpan.ToArray()));
    }
}
