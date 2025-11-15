// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Text.Json.Converters;

using Aqua.TypeSystem;
using System;

public sealed class MemberInfoConverter : MemberInfoConverter<MemberInfo>
{
    public MemberInfoConverter(KnownTypesRegistry knownTypes)
        : base(knownTypes)
    {
    }

    public override bool CanConvert(Type typeToConvert)
        => typeof(MemberInfo).IsAssignableFrom(typeToConvert);
}