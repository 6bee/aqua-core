// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.MessagePack.Tests.Formatters;

using Aqua.MessagePack.Formatters;
using Aqua.TypeSystem;

public class When_using_property_info_formatter
{
    [Fact]
    public void Null_should_round_trip() => FormatterTestHelper.RoundTrip(PropertyInfoFormatter.Instance, default(PropertyInfo)).ShouldBeNull();

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Property_should_round_trip_with_static_flag(bool isStatic)
    {
        var result = FormatterTestHelper.RoundTrip(PropertyInfoFormatter.Instance, new PropertyInfo("Name", new TypeInfo { Name = "String" }, new TypeInfo { Name = "Container" }) { IsStatic = isStatic });

        result.Name.ShouldBe("Name");
        result.IsStatic.ShouldBe(isStatic);
        result.DeclaringType.Name.ShouldBe("Container");
        result.PropertyType.Name.ShouldBe("String");
    }

    [Fact]
    public void Property_should_preserve_null_static_flag()
    {
        var result = FormatterTestHelper.RoundTrip(PropertyInfoFormatter.Instance, new PropertyInfo("Name", new TypeInfo { Name = "String" }, new TypeInfo { Name = "Container" }));

        result.IsStatic.ShouldBeNull();
    }
}
