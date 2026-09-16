// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.MessagePack.Tests.Formatters;

using Aqua.MessagePack.Formatters;
using Aqua.TypeSystem;

public class When_using_method_info_formatter
{
    [Fact]
    public void Null_should_round_trip() => FormatterTestHelper.RoundTrip(MethodInfoFormatter.Instance, default(MethodInfo)).ShouldBeNull();

    [Fact]
    public void Method_should_round_trip()
    {
        var result = FormatterTestHelper.RoundTrip(MethodInfoFormatter.Instance, new MethodInfo("Convert", new TypeInfo { Name = "Converter" }, [new TypeInfo { Name = "T" }], [new TypeInfo { Name = "String" }], new TypeInfo { Name = "Boolean" }) { IsStatic = true });

        result.Name.ShouldBe("Convert");
        result.IsStatic.ShouldBe(true);
        result.DeclaringType.Name.ShouldBe("Converter");
        result.GenericArgumentTypes.Single().Name.ShouldBe("T");
        result.ParameterTypes.Single().Name.ShouldBe("String");
        result.ReturnType.Name.ShouldBe("Boolean");
    }

    [Fact]
    public void Null_type_lists_should_round_trip()
    {
        var result = FormatterTestHelper.RoundTrip(MethodInfoFormatter.Instance, new MethodInfo { Name = "M" });

        result.GenericArgumentTypes.ShouldBeNull();
        result.ParameterTypes.ShouldBeNull();
        result.ReturnType.ShouldBeNull();
    }
}
