// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.MessagePack.Tests.Formatters;

using Aqua.Dynamic;
using Aqua.MessagePack.Formatters;
using global::MessagePack;
using System.Buffers;

public class When_using_property_set_formatter
{
    [Fact]
    public void Null_should_round_trip() => FormatterTestHelper.RoundTrip(PropertySetFormatter.Instance, default(PropertySet)).ShouldBeNull();

    [Fact]
    public void Properties_should_round_trip_in_order()
    {
        var result = FormatterTestHelper.RoundTrip(PropertySetFormatter.Instance, new PropertySet([("First", (object)1), ("Second", (object)"two")]));

        result.Count.ShouldBe(2);
        result.Select(x => x.Name).ShouldBe(["First", "Second"]);
        result["First"].ShouldBe(1);
        result["Second"].ShouldBe("two");
    }

    [Fact]
    public void Empty_set_should_round_trip()
    {
        FormatterTestHelper.RoundTrip(PropertySetFormatter.Instance, new PropertySet()).Count.ShouldBe(0);
    }

    [Fact]
    public void Null_property_entries_should_be_filtered_on_deserialization()
    {
        var buffer = new ArrayBufferWriter<byte>();
        var writer = new MessagePackWriter(buffer);
        writer.WriteArrayHeader(2);
        writer.WriteNil();
        PropertyFormatter.Instance.Serialize(ref writer, new Property("Name", "aqua"), FormatterTestHelper.Options());
        writer.Flush();

        var result = FormatterTestHelper.Decode(PropertySetFormatter.Instance, buffer.WrittenSpan.ToArray());

        result.Count.ShouldBe(1);
        result["Name"].ShouldBe("aqua");
    }
}
