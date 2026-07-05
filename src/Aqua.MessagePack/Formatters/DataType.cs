// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.MessagePack.Formatters;

internal enum DataType : byte
{
    UInt8 = 1,
    Int8 = 2,
    Int16 = 3,
    UInt16 = 4,
    Int32 = 5,
    UInt32 = 6,
    Int64 = 7,
    UInt64 = 8,
#if NET7_0_OR_GREATER
    Int128 = 9,
    UInt128 = 10,
#endif // NET7_0_OR_GREATER
#if NET5_0_OR_GREATER
    Float16 = 11,
#endif // NET5_0_OR_GREATER
    Float32 = 12,
    Float64 = 13,
    BigInteger = 14,
    Complex128 = 16,
    Bool = 17,
    Char = 18,
    Decimal = 19,
    Uuid = 20,
    DateTime = 21,
    DateTimeOffset = 22,
    TimeSpan = 23,
#if NET6_0_OR_GREATER
    DateOnly = 24,
    TimeOnly = 25,
#endif // NET6_0_OR_GREATER
}
