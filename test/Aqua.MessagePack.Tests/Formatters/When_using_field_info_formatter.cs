// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.MessagePack.Tests.Formatters;

using Aqua.MessagePack.Formatters;
using Aqua.TypeSystem;

public class When_using_field_info_formatter
{
    [Fact]
    public void Null_should_round_trip() => FormatterTestHelper.RoundTrip(FieldInfoFormatter.Instance, default(FieldInfo)).ShouldBeNull();

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Field_should_round_trip_with_static_flag(bool isStatic)
    {
        var result = FormatterTestHelper.RoundTrip(FieldInfoFormatter.Instance, new FieldInfo("Value", new TypeInfo { Name = "Container" }) { IsStatic = isStatic });

        result.Name.ShouldBe("Value");
        result.IsStatic.ShouldBe(isStatic);
        result.DeclaringType.Name.ShouldBe("Container");
    }

    [Fact]
    public void Field_should_preserve_null_static_flag()
    {
        var result = FormatterTestHelper.RoundTrip(FieldInfoFormatter.Instance, new FieldInfo("Value", new TypeInfo { Name = "Container" }));

        result.IsStatic.ShouldBeNull();
    }
}
