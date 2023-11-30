// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Text.Json.Converters;

using Aqua.TypeSystem;

public class MemberInfoConverter<TMemberInfo> : ObjectConverter<TMemberInfo>
    where TMemberInfo : MemberInfo
{
    public MemberInfoConverter(KnownTypesRegistry knownTypes)
        : base(knownTypes)
    {
    }
}
