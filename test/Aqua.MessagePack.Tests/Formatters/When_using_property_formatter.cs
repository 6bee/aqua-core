// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.MessagePack.Tests.Formatters;

using Aqua.Dynamic;
using Aqua.MessagePack.Formatters;

public class When_using_property_formatter
{
    [Fact]
    public void Null_should_round_trip() => FormatterTestHelper.RoundTrip(PropertyFormatter.Instance, default(Property)).ShouldBeNull();

    [Fact]
    public void Name_and_value_should_round_trip()
    {
        var result = FormatterTestHelper.RoundTrip(PropertyFormatter.Instance, new Property("Count", 42));

        result.Name.ShouldBe("Count");
        result.Value.ShouldBe(42);
    }
}
