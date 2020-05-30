// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.ProtoBuf
{
    using Aqua.Extensions;
    using Aqua.TypeSystem;
    using global::ProtoBuf.Meta;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    internal static class ProtoBufMetaExtensions
    {
        public static MetaType AddSubType<T>(this MetaType type, ref int fieldNumber, Action<MetaType>? configure = null)
            => AddSubType(type, typeof(T), ref fieldNumber, configure);

        public static MetaType AddSubType(this MetaType type, Type subType, ref int fieldNumber, Action<MetaType>? configure = null)
        {
            var n = fieldNumber++;
            type.AddSubType(n, subType);
            configure?.Invoke(type.GetSubtypes().Single(x => x.FieldNumber == n).DerivedType);
            return type;
        }

        public static MetaType GetType<T>(this RuntimeTypeModel typeModel)
            => typeModel[typeof(T)];

        public static ValueMember GetMember(this MetaType type, string propertyName)
            => type.GetFields().Single(x => string.Equals(x.Name, propertyName, StringComparison.Ordinal));

        public static void SetSurrogate<T>(this MetaType type)
            => type.SetSurrogate(typeof(T));

        ////public static ValueMember MakeDynamicType(this ValueMember member)
        ////{
        ////    member.DynamicType = true;
        ////    return member;
        ////}

        /// <summary>Register a base class and it's subtypes from the same assebly, uncluding all their serializable fields and proeprties.</summary>
        public static RuntimeTypeModel RegisterBaseAndSubtypes<T>(this RuntimeTypeModel typeModel, ref int fieldNumber)
        {
            var resiteredTypes = new HashSet<Type>();
            var pendingTypes = new HashSet<Type>();

            void CollectType(Type t)
            {
                t = TypeHelper.GetElementType(t) ?? t;

                if (resiteredTypes.Contains(t) || pendingTypes.Contains(t))
                {
                    return;
                }

                pendingTypes.Add(t);
                t.GetDefaultPropertiesForSerialization().Select(x => x.PropertyType).ForEach(CollectType);
                t.GetDefaultFieldsForSerialization().Select(x => x.FieldType).ForEach(CollectType);
            }

            void RegisterBaseAndSubtypes(Type baseType, ref int fnum)
            {
                if (resiteredTypes.Contains(baseType))
                {
                    return;
                }

                var baseModel = typeModel[baseType];
                resiteredTypes.Add(baseType);

                var expressionTypes = baseType.Assembly
                    .GetTypes()
                    .Where(x => x.BaseType == baseType)
                    .OrderBy(x => x.FullName)
                    .ToArray();

                foreach (var t in expressionTypes)
                {
                    baseModel.AddSubType(++fnum, t);
                    RegisterBaseAndSubtypes(t, ref fnum);
                    CollectType(t);
                }
            }

            RegisterBaseAndSubtypes(typeof(T), ref fieldNumber);

            pendingTypes.ForEach(x => _ = typeModel[x]);

            return typeModel;
        }
    }
}
