// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.MessagePack.Formatters;

internal enum ValueKind : byte
{
    Null = 1,
    String = 2,
    Scalar = 3,
    PackedArray = 4,
    Collection = 5,
    DynamicObject = 80,
    Property = 81,
    PropertySet = 82,
    TypeInfo = 83,
}
