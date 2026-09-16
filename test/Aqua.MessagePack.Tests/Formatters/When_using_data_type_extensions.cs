// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.MessagePack.Tests.Formatters;

using Aqua.MessagePack.Formatters;
using System.Numerics;

public class When_using_data_type_extensions
{
    public static IEnumerable<object[]> Mappings()
    {
        yield return [typeof(byte), (byte)DataType.UInt8]; yield return [typeof(sbyte), (byte)DataType.Int8];
        yield return [typeof(short), (byte)DataType.Int16]; yield return [typeof(ushort), (byte)DataType.UInt16];
        yield return [typeof(int), (byte)DataType.Int32]; yield return [typeof(uint), (byte)DataType.UInt32];
        yield return [typeof(long), (byte)DataType.Int64]; yield return [typeof(ulong), (byte)DataType.UInt64];
        yield return [typeof(float), (byte)DataType.Float32]; yield return [typeof(double), (byte)DataType.Float64];
        yield return [typeof(bool), (byte)DataType.Bool]; yield return [typeof(char), (byte)DataType.Char];
        yield return [typeof(decimal), (byte)DataType.Decimal]; yield return [typeof(BigInteger), (byte)DataType.BigInteger];
        yield return [typeof(Complex), (byte)DataType.Complex128]; yield return [typeof(Guid), (byte)DataType.Uuid];
        yield return [typeof(DateTime), (byte)DataType.DateTime]; yield return [typeof(DateTimeOffset), (byte)DataType.DateTimeOffset];
        yield return [typeof(TimeSpan), (byte)DataType.TimeSpan];
#if NET5_0_OR_GREATER
        yield return [typeof(Half), (byte)DataType.Float16];
#endif
#if NET6_0_OR_GREATER
        yield return [typeof(DateOnly), (byte)DataType.DateOnly]; yield return [typeof(TimeOnly), (byte)DataType.TimeOnly];
#endif
#if NET7_0_OR_GREATER
        yield return [typeof(Int128), (byte)DataType.Int128]; yield return [typeof(UInt128), (byte)DataType.UInt128];
#endif
    }

    [Theory]
    [MemberData(nameof(Mappings))]
    public void From_type_should_return_expected_data_type(Type type, byte expected)
        => DataType.FromType(type).ShouldBe((DataType)expected);

    [Theory]
    [MemberData(nameof(Mappings))]
    public void To_type_should_return_expected_runtime_type(Type type, byte dataType)
        => DataType.ToType((DataType)dataType).ShouldBe(type);

    [Theory]
    [MemberData(nameof(Mappings))]
    public void Type_mappings_should_round_trip(Type type, byte dataType)
    {
        DataType.FromType(DataType.ToType((DataType)dataType)).ShouldBe((DataType)dataType);
        DataType.ToType(DataType.FromType(type).Value).ShouldBe(type);
    }

    [Fact]
    public void Unsupported_type_should_return_null() => DataType.FromType(typeof(string)).ShouldBeNull();

    [Fact]
    public void Invalid_data_type_should_return_null() => DataType.ToType((DataType)255).ShouldBeNull();
}
