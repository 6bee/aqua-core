// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Tests.Text.Json.KnownTypesRegistry;

using Aqua;
using Aqua.Dynamic;
using Aqua.TypeSystem;
using System.Numerics;

public sealed class When_checking_default_registry_alignment
{
    public static IEnumerable<object[]> NativeScalarTypes()
    {
        yield return [typeof(string)];
        yield return [typeof(int)];
        yield return [typeof(int?)];
        yield return [typeof(uint)];
        yield return [typeof(byte)];
        yield return [typeof(byte?)];
        yield return [typeof(sbyte)];
        yield return [typeof(short)];
        yield return [typeof(ushort)];
        yield return [typeof(long)];
        yield return [typeof(ulong)];
        yield return [typeof(float)];
        yield return [typeof(double)];
        yield return [typeof(decimal)];
        yield return [typeof(char)];
        yield return [typeof(bool)];
        yield return [typeof(Guid)];
        yield return [typeof(DateTime)];
        yield return [typeof(TimeSpan)];
        yield return [typeof(DateTimeOffset)];
        yield return [typeof(BigInteger)];
        yield return [typeof(BigInteger?)];
        yield return [typeof(Complex)];
        yield return [typeof(Complex?)];
        yield return [typeof(byte[])];
        yield return [typeof(DynamicObject)];
        yield return [typeof(TypeInfo)];
        yield return [typeof(PropertyInfo)];
        yield return [typeof(FieldInfo)];
        yield return [typeof(ConstructorInfo)];
        yield return [typeof(MethodInfo)];
    }

    [Theory]
    [MemberData(nameof(NativeScalarTypes))]
    public void Default_registry_should_contain_key_for_type(Type type)
    {
        var registry = KnownTypesRegistry.Default;

        registry.TryGetTypeKey(type, out var key).ShouldBeTrue($"Expected '{type}' to have a key in the default registry");
        key.ShouldNotBeNullOrEmpty();
    }

    public static IEnumerable<object[]> AbstractTypes()
    {
        yield return [typeof(MemberInfo)];
        yield return [typeof(MethodBaseInfo)];
    }

    [Theory]
    [MemberData(nameof(AbstractTypes))]
    public void Default_registry_should_not_contain_key_for_abstract_type(Type type)
    {
        var registry = KnownTypesRegistry.Default;

        registry.TryGetTypeKey(type, out _).ShouldBeFalse($"Abstract type '{type}' should not be in the default registry");
    }

    [Theory]
    [MemberData(nameof(NativeScalarTypes))]
    public void Default_registry_key_should_resolve_back_to_same_type(Type type)
    {
        var registry = KnownTypesRegistry.Default;

        registry.TryGetTypeKey(type, out var key).ShouldBeTrue();
        registry.TryGetTypeInfo(key, out var typeInfo).ShouldBeTrue($"Key '{key}' for '{type}' did not resolve back to a TypeInfo");
        typeInfo.ToType().ShouldBe(type);
    }
}
