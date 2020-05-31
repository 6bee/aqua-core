// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.ProtoBuf
{
    using global::ProtoBuf.Meta;
    using System;

    public class AquaTypeModel
    {
        public AquaTypeModel(RuntimeTypeModel typeModel)
        {
            Model = typeModel ?? throw new ArgumentNullException(nameof(typeModel));

            typeModel.AutoAddMissingTypes = true;
            typeModel.AllowParseableTypes = false;
            typeModel.AutoCompile = true;
        }

        public RuntimeTypeModel Model { get; }

        public AquaTypeModel AddTypeSurrogate<T, TSurrogate>()
        {
            GetType<T>().SetSurrogate(typeof(TSurrogate));
            return this;
        }

        public AquaTypeModel AddType<T>()
        {
            _ = GetType<T>();
            return this;
        }

        public AquaTypeModel AddSubType<TBase, T>() => AddSubType<TBase>(typeof(T));

        public AquaTypeModel AddSubType<TBase>(Type subtype)
        {
            var baseType = GetType<TBase>();
            var n = baseType.GetFields().Length + baseType.GetSubtypes().Length + 1;
            baseType.AddSubType(n, subtype);
            return this;
        }

        private MetaType GetType<T>() => Model[typeof(T)];

        public TypeModel Compile() => Model.Compile();

        public static implicit operator RuntimeTypeModel(AquaTypeModel model) => model.Model;
    }
}
