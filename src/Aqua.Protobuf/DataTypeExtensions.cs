// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Protobuf;

using System.Numerics;
using Proto = Aqua.Protobuf.Schema;

internal static class DataTypeExtensions
{
    extension(Proto.DataType)
    {
        public static Proto.DataType? FromType(Type type) => type switch
        {
            _ when type == typeof(byte) => Proto.DataType.Uint8,
            _ when type == typeof(sbyte) => Proto.DataType.Int8,
            _ when type == typeof(short) => Proto.DataType.Int16,
            _ when type == typeof(ushort) => Proto.DataType.Uint16,
            _ when type == typeof(int) => Proto.DataType.Int32,
            _ when type == typeof(uint) => Proto.DataType.Uint32,
            _ when type == typeof(long) => Proto.DataType.Int64,
            _ when type == typeof(ulong) => Proto.DataType.Uint64,
#if NET7_0_OR_GREATER
            _ when type == typeof(Int128) => Proto.DataType.Int128,
            _ when type == typeof(UInt128) => Proto.DataType.Uint128,
#endif
#if NET5_0_OR_GREATER
            _ when type == typeof(Half) => Proto.DataType.Float16,
#endif
            _ when type == typeof(float) => Proto.DataType.Float32,
            _ when type == typeof(double) => Proto.DataType.Float64,
            _ when type == typeof(BigInteger) => Proto.DataType.BigInteger,
            _ when type == typeof(Complex) => Proto.DataType.Complex128,
            _ when type == typeof(bool) => Proto.DataType.Bool,
            _ when type == typeof(char) => Proto.DataType.Char,
            _ when type == typeof(decimal) => Proto.DataType.Decimal,
            _ when type == typeof(Guid) => Proto.DataType.Uuid,
            _ when type == typeof(DateTime) => Proto.DataType.DateTime,
            _ when type == typeof(DateTimeOffset) => Proto.DataType.DateTimeOffset,
            _ when type == typeof(TimeSpan) => Proto.DataType.TimeSpan,
#if NET6_0_OR_GREATER
            _ when type == typeof(DateOnly) => Proto.DataType.DateOnly,
            _ when type == typeof(TimeOnly) => Proto.DataType.TimeOnly,
#endif
            _ => null,
        };

        public static Type? ToType(Proto.DataType dataType) => dataType switch
        {
            Proto.DataType.Uint8 => typeof(byte),
            Proto.DataType.Int8 => typeof(sbyte),
            Proto.DataType.Int16 => typeof(short),
            Proto.DataType.Uint16 => typeof(ushort),
            Proto.DataType.Int32 => typeof(int),
            Proto.DataType.Uint32 => typeof(uint),
            Proto.DataType.Int64 => typeof(long),
            Proto.DataType.Uint64 => typeof(ulong),
#if NET7_0_OR_GREATER
            Proto.DataType.Int128 => typeof(Int128),
            Proto.DataType.Uint128 => typeof(UInt128),
#endif
#if NET5_0_OR_GREATER
            Proto.DataType.Float16 => typeof(Half),
#endif
            Proto.DataType.Float32 => typeof(float),
            Proto.DataType.Float64 => typeof(double),
            Proto.DataType.BigInteger => typeof(BigInteger),
            Proto.DataType.Complex128 => typeof(Complex),
            Proto.DataType.Bool => typeof(bool),
            Proto.DataType.Char => typeof(char),
            Proto.DataType.Decimal => typeof(decimal),
            Proto.DataType.Uuid => typeof(Guid),
            Proto.DataType.DateTime => typeof(DateTime),
            Proto.DataType.DateTimeOffset => typeof(DateTimeOffset),
            Proto.DataType.TimeSpan => typeof(TimeSpan),
#if NET6_0_OR_GREATER
            Proto.DataType.DateOnly => typeof(DateOnly),
            Proto.DataType.TimeOnly => typeof(TimeOnly),
#endif
            _ => null,
        };
    }
}
