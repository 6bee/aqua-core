// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua
{
    using Aqua.ProtoBuf;
    using Aqua.ProtoBuf.Dynamic;
    using Aqua.ProtoBuf.TypeSystem;
    using global::ProtoBuf.Meta;
    using System;
    using System.ComponentModel;

    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class ProtoBufTypeModel
    {
        public static RuntimeTypeModel ConfigureAquaTypes(string? name = null)
            => ConfigureAquaTypes(RuntimeTypeModel.Create(name));

        public static RuntimeTypeModel ConfigureAquaTypes(this RuntimeTypeModel typeModel)
            => (typeModel ?? throw new ArgumentNullException(nameof(typeModel)))
            .ConfigureAquaDefaults()
            .ConfigureAquaValueWrappingTypes()
            .ConfigureAquaTypeSystemTypes()
            .ConfigureAquaDynamicTypes();

        private static RuntimeTypeModel ConfigureAquaDefaults(this RuntimeTypeModel typeModel)
        {
            typeModel.AutoAddMissingTypes = true;
            typeModel.AllowParseableTypes = false;
            typeModel.AutoCompile = true;
            return typeModel;
        }

        private static RuntimeTypeModel ConfigureAquaValueWrappingTypes(this RuntimeTypeModel typeModel)
        {
            _ = typeModel.GetType<Value>();
            _ = typeModel.GetType<Values>();
            return typeModel;
        }
    }
}
