// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Tests.Text.Json.KnownTypesRegistry;

using Aqua;

public sealed class When_registrying_type
{
    private readonly struct CustomValue;

    [Theory]
    [InlineData(typeof(string))]
    [InlineData(typeof(byte))]
    [InlineData(typeof(byte?))]
    public void Should_return_false_when_type_already_contained(Type type, string typeKey = null)
    {
        var registry = KnownTypesRegistry.Default;

        var result = registry.TryRegister(type, typeKey);

        result.ShouldBeFalse();
    }

    [Theory]
    [InlineData(typeof(CustomValue))]
    [InlineData(typeof(CustomValue?))]
    [InlineData(typeof(CustomValue?[]))]
    public void Should_return_true_when_new_type_registered(Type type, string typeKey = null)
    {
        var registry = KnownTypesRegistry.Default;

        var result = registry.TryRegister(type, typeKey);

        result.ShouldBeTrue();
    }

    [Fact]
    public void Should_throw_on_open_generic_type()
    {
        var registry = KnownTypesRegistry.Default;

        Should.Throw<ArgumentException>(() => registry.TryRegister(typeof(List<>)));
    }

    [Fact]
    public void Should_allow_multiple_closed_generic_types()
    {
        var registry = KnownTypesRegistry.Default;

        registry.TryRegister<KeyValuePair<string, long>>().ShouldBeTrue();
        registry.TryRegister<KeyValuePair<string, byte>>().ShouldBeTrue();
    }
}
