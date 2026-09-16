// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.MessagePack.Tests.Formatters;

using Aqua.MessagePack.Formatters;
using Aqua.TypeSystem;

public class When_using_constructor_info_formatter
{
    [Fact]
    public void Null_should_round_trip() => FormatterTestHelper.RoundTrip(ConstructorInfoFormatter.Instance, default(ConstructorInfo)).ShouldBeNull();

    [Fact]
    public void Constructor_should_round_trip()
    {
        var result = FormatterTestHelper.RoundTrip(ConstructorInfoFormatter.Instance, new ConstructorInfo
        {
            Name = ".ctor",
            DeclaringType = new TypeInfo { Name = "Container" },
            GenericArgumentTypes = [new TypeInfo { Name = "T" }],
            ParameterTypes = [new TypeInfo { Name = "String" }],
            IsStatic = false,
        });

        result.Name.ShouldBe(".ctor");
        result.IsStatic.ShouldBe(false);
        result.DeclaringType.Name.ShouldBe("Container");
        result.GenericArgumentTypes.Single().Name.ShouldBe("T");
        result.ParameterTypes.Single().Name.ShouldBe("String");
    }

    [Fact]
    public void Null_type_lists_should_round_trip()
    {
        var result = FormatterTestHelper.RoundTrip(ConstructorInfoFormatter.Instance, new ConstructorInfo { Name = ".ctor" });

        result.GenericArgumentTypes.ShouldBeNull();
        result.ParameterTypes.ShouldBeNull();
    }
}
