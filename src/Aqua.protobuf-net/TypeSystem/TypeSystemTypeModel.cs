// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.ProtoBuf.TypeSystem;

using Aqua.TypeSystem;

internal static class TypeSystemTypeModel
{
    public static AquaTypeModel ConfigureAquaTypeSystemTypes(this AquaTypeModel typeModel)
        => typeModel
        .AddType<TypeInfo>()
        .AddType<MemberInfo>()
        .AddSubType<MemberInfo, PropertyInfo>()
        .AddSubType<MemberInfo, FieldInfo>()
        .AddSubType<MemberInfo, MethodBaseInfo>()
        .AddSubType<MethodBaseInfo, MethodInfo>()
        .AddSubType<MethodBaseInfo, ConstructorInfo>();
}
