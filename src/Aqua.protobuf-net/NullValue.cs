// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.ProtoBuf;

using global::ProtoBuf;

[ProtoContract]
public sealed class NullValue : Value
{
    internal static readonly NullValue Instance = new NullValue();
}