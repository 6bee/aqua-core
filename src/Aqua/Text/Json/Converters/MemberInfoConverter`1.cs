// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Text.Json.Converters;

using Aqua.TypeSystem;

public class MemberInfoConverter<TMemberInfo>(KnownTypesRegistry knownTypes, bool handleSubtypes) : ObjectConverter<TMemberInfo>(knownTypes, handleSubtypes)
    where TMemberInfo : MemberInfo
{
    public MemberInfoConverter(KnownTypesRegistry knownTypes)
        : this(knownTypes, false)
    {
    }

    public MemberInfoConverter()
        : this(KnownTypesRegistry.Default)
    {
    }
}