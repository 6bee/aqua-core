// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Protobuf;

using System.Numerics;
using Proto = Aqua.Protobuf.Schema;

internal static class DataTypeExtensions
{
    extension(Proto.Scalar.Types.DataType)
    {
        public static Proto.Scalar.Types.DataType? FromType(Type type) => type switch
        {
            _ when type == typeof(byte) => Proto.Scalar.Types.DataType.Uint8,
            _ when type == typeof(sbyte) => Proto.Scalar.Types.DataType.Int8,
            _ when type == typeof(short) => Proto.Scalar.Types.DataType.Int16,
            _ when type == typeof(ushort) => Proto.Scalar.Types.DataType.Uint16,
            _ when type == typeof(int) => Proto.Scalar.Types.DataType.Int32,
            _ when type == typeof(uint) => Proto.Scalar.Types.DataType.Uint32,
            _ when type == typeof(long) => Proto.Scalar.Types.DataType.Int64,
            _ when type == typeof(ulong) => Proto.Scalar.Types.DataType.Uint64,
#if NET7_0_OR_GREATER
            _ when type == typeof(Int128) => Proto.Scalar.Types.DataType.Int128,
            _ when type == typeof(UInt128) => Proto.Scalar.Types.DataType.Uint128,
#endif
#if NET5_0_OR_GREATER
            _ when type == typeof(Half) => Proto.Scalar.Types.DataType.Float16,
#endif
            _ when type == typeof(float) => Proto.Scalar.Types.DataType.Float32,
            _ when type == typeof(double) => Proto.Scalar.Types.DataType.Float64,
            _ when type == typeof(BigInteger) => Proto.Scalar.Types.DataType.BigInteger,
            _ when type == typeof(Complex) => Proto.Scalar.Types.DataType.Complex128,
            _ when type == typeof(bool) => Proto.Scalar.Types.DataType.Bool,
            _ when type == typeof(char) => Proto.Scalar.Types.DataType.Char,
            _ when type == typeof(decimal) => Proto.Scalar.Types.DataType.Decimal,
            _ when type == typeof(Guid) => Proto.Scalar.Types.DataType.Uuid,
            _ when type == typeof(DateTime) => Proto.Scalar.Types.DataType.DateTime,
            _ when type == typeof(DateTimeOffset) => Proto.Scalar.Types.DataType.DateTimeOffset,
            _ when type == typeof(TimeSpan) => Proto.Scalar.Types.DataType.TimeSpan,
#if NET6_0_OR_GREATER
            _ when type == typeof(DateOnly) => Proto.Scalar.Types.DataType.DateOnly,
            _ when type == typeof(TimeOnly) => Proto.Scalar.Types.DataType.TimeOnly,
#endif
            _ => null,
        };

        public static Type? ToType(Proto.Scalar.Types.DataType dataType) => dataType switch
        {
            Proto.Scalar.Types.DataType.Uint8 => typeof(byte),
            Proto.Scalar.Types.DataType.Int8 => typeof(sbyte),
            Proto.Scalar.Types.DataType.Int16 => typeof(short),
            Proto.Scalar.Types.DataType.Uint16 => typeof(ushort),
            Proto.Scalar.Types.DataType.Int32 => typeof(int),
            Proto.Scalar.Types.DataType.Uint32 => typeof(uint),
            Proto.Scalar.Types.DataType.Int64 => typeof(long),
            Proto.Scalar.Types.DataType.Uint64 => typeof(ulong),
#if NET7_0_OR_GREATER
            Proto.Scalar.Types.DataType.Int128 => typeof(Int128),
            Proto.Scalar.Types.DataType.Uint128 => typeof(UInt128),
#endif
#if NET5_0_OR_GREATER
            Proto.Scalar.Types.DataType.Float16 => typeof(Half),
#endif
            Proto.Scalar.Types.DataType.Float32 => typeof(float),
            Proto.Scalar.Types.DataType.Float64 => typeof(double),
            Proto.Scalar.Types.DataType.BigInteger => typeof(BigInteger),
            Proto.Scalar.Types.DataType.Complex128 => typeof(Complex),
            Proto.Scalar.Types.DataType.Bool => typeof(bool),
            Proto.Scalar.Types.DataType.Char => typeof(char),
            Proto.Scalar.Types.DataType.Decimal => typeof(decimal),
            Proto.Scalar.Types.DataType.Uuid => typeof(Guid),
            Proto.Scalar.Types.DataType.DateTime => typeof(DateTime),
            Proto.Scalar.Types.DataType.DateTimeOffset => typeof(DateTimeOffset),
            Proto.Scalar.Types.DataType.TimeSpan => typeof(TimeSpan),
#if NET6_0_OR_GREATER
            Proto.Scalar.Types.DataType.DateOnly => typeof(DateOnly),
            Proto.Scalar.Types.DataType.TimeOnly => typeof(TimeOnly),
#endif
            _ => null,
        };
    }
}
