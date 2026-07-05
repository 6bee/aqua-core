// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.MessagePack.Formatters;

internal enum ValueTag : byte
{
    Null = 1,
    String = 2,
    Scalar = 3,
    PackedArray = 4,
    Collection = 5,
    DynamicObject = 6,
    Property = 7,
    PropertySet = 8,
    TypeInfo = 9,
}
