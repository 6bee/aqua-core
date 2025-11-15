// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Text.Json.Converters;

using Aqua.TypeSystem;

public sealed class MemberInfoConverter(KnownTypesRegistry knownTypes) : MemberInfoConverter<MemberInfo>(knownTypes, true)
{
    public MemberInfoConverter()
        : this(KnownTypesRegistry.Default)
    {
    }
}