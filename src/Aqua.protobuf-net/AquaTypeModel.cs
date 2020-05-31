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

        public AquaTypeModel AddTypeSurrogate<T, TSurrogate>(Action<MetaType>? config = null)
            => AddTypeSurrogate<T>(typeof(TSurrogate), config);

        public AquaTypeModel AddTypeSurrogate<T>(Type surrogateType, Action<MetaType>? config = null)
            => AddTypeSurrogate(typeof(T), surrogateType, config);

        public AquaTypeModel AddTypeSurrogate(Type type, Type surrogateType, Action<MetaType>? config = null)
        {
            GetType(type).SetSurrogate(surrogateType);
            config?.Invoke(GetType(surrogateType));
            return this;
        }

        public AquaTypeModel AddType<T>(Action<MetaType>? config = null)
            => AddType(typeof(T), config);

        public AquaTypeModel AddType(Type type, Action<MetaType>? config = null)
        {
            var metaType = GetType(type);
            config?.Invoke(metaType);
            return this;
        }

        public AquaTypeModel AddSubType<TBase, T>(Action<MetaType>? config = null)
            => AddSubType<TBase>(typeof(T), config);

        public AquaTypeModel AddSubType<TBase>(Type subtype, Action<MetaType>? config = null)
            => AddSubType(typeof(TBase), subtype, config);

        public AquaTypeModel AddSubType(Type baseType, Type subtype, Action<MetaType>? config = null)
        {
            var metaBase = GetType(baseType);
            var n = metaBase.GetFields().Length + metaBase.GetSubtypes().Length + 1;
            var metaSub = metaBase.AddSubType(n, subtype);
            config?.Invoke(metaSub);
            return this;
        }

        public MetaType GetType<T>() => GetType(typeof(T));

        public MetaType GetType(Type type) => Model[type];

        public TypeModel Compile() => Model.Compile();

        public static implicit operator RuntimeTypeModel(AquaTypeModel model) => model.Model;
    }
}
