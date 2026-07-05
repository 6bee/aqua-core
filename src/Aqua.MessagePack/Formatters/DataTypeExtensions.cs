// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.MessagePack.Formatters;

using System.Numerics;

internal static class DataTypeExtensions
{
    extension(DataType)
    {
        public static DataType? FromType(Type type) => type switch
        {
            _ when type == typeof(byte) => DataType.UInt8,
            _ when type == typeof(sbyte) => DataType.Int8,
            _ when type == typeof(short) => DataType.Int16,
            _ when type == typeof(ushort) => DataType.UInt16,
            _ when type == typeof(int) => DataType.Int32,
            _ when type == typeof(uint) => DataType.UInt32,
            _ when type == typeof(long) => DataType.Int64,
            _ when type == typeof(ulong) => DataType.UInt64,
#if NET7_0_OR_GREATER
            _ when type == typeof(Int128) => DataType.Int128,
            _ when type == typeof(UInt128) => DataType.UInt128,
#endif
#if NET5_0_OR_GREATER
            _ when type == typeof(Half) => DataType.Float16,
#endif
            _ when type == typeof(float) => DataType.Float32,
            _ when type == typeof(double) => DataType.Float64,
            _ when type == typeof(BigInteger) => DataType.BigInteger,
            _ when type == typeof(Complex) => DataType.Complex128,
            _ when type == typeof(bool) => DataType.Bool,
            _ when type == typeof(char) => DataType.Char,
            _ when type == typeof(decimal) => DataType.Decimal,
            _ when type == typeof(Guid) => DataType.Uuid,
            _ when type == typeof(DateTime) => DataType.DateTime,
            _ when type == typeof(DateTimeOffset) => DataType.DateTimeOffset,
            _ when type == typeof(TimeSpan) => DataType.TimeSpan,
#if NET6_0_OR_GREATER
            _ when type == typeof(DateOnly) => DataType.DateOnly,
            _ when type == typeof(TimeOnly) => DataType.TimeOnly,
#endif
            _ => null,
        };

        public static Type? ToType(DataType dataType) => dataType switch
        {
            DataType.UInt8 => typeof(byte),
            DataType.Int8 => typeof(sbyte),
            DataType.Int16 => typeof(short),
            DataType.UInt16 => typeof(ushort),
            DataType.Int32 => typeof(int),
            DataType.UInt32 => typeof(uint),
            DataType.Int64 => typeof(long),
            DataType.UInt64 => typeof(ulong),
#if NET7_0_OR_GREATER
            DataType.Int128 => typeof(Int128),
            DataType.UInt128 => typeof(UInt128),
#endif
#if NET5_0_OR_GREATER
            DataType.Float16 => typeof(Half),
#endif
            DataType.Float32 => typeof(float),
            DataType.Float64 => typeof(double),
            DataType.BigInteger => typeof(BigInteger),
            DataType.Complex128 => typeof(Complex),
            DataType.Bool => typeof(bool),
            DataType.Char => typeof(char),
            DataType.Decimal => typeof(decimal),
            DataType.Uuid => typeof(Guid),
            DataType.DateTime => typeof(DateTime),
            DataType.DateTimeOffset => typeof(DateTimeOffset),
            DataType.TimeSpan => typeof(TimeSpan),
#if NET6_0_OR_GREATER
            DataType.DateOnly => typeof(DateOnly),
            DataType.TimeOnly => typeof(TimeOnly),
#endif
            _ => null,
        };
    }
}
