// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.ProtoBuf.TypeSystem
{
    using Aqua.TypeSystem;
    using global::ProtoBuf.Meta;

    internal static class TypeSystemTypeModel
    {
        public static RuntimeTypeModel ConfigureAquaTypeSystemTypes(this RuntimeTypeModel typeModel)
        {
            _ = typeModel.GetType<TypeInfo>();

            var n = 10;
            typeModel
                .GetType<MemberInfo>()
                .AddSubType<PropertyInfo>(ref n)
                .AddSubType<FieldInfo>(ref n)
                .AddSubType<MethodBaseInfo>(ref n, m =>
                {
                    m
                    .AddSubType<MethodInfo>(ref n)
                    .AddSubType<ConstructorInfo>(ref n);
                });

            return typeModel;
        }
    }
}
