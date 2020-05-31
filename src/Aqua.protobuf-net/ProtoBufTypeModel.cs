// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua
{
    using Aqua.ProtoBuf;
    using Aqua.ProtoBuf.Dynamic;
    using Aqua.ProtoBuf.TypeSystem;
    using global::ProtoBuf.Meta;
    using System;
    using System.Collections.Immutable;
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
            var n = 10;
            return typeModel
              .ConfigureTypedValueWrapper<Value>(typeof(Value<>), false, ref n)
              .ConfigureTypedValueWrapper<Values>(typeof(Values<>), true, ref n);
        }

        private static RuntimeTypeModel ConfigureTypedValueWrapper<TBase>(this RuntimeTypeModel typeModel, Type genericWrapperType, bool addNullableSupport, ref int n)
        {
            var wrapperBaseType = typeModel.GetType<TBase>();

            if (typeof(TBase) == typeof(Values))
            {
                wrapperBaseType.AddSubType<Values<Value>>(ref n);
            }
            else if (typeof(TBase).IsAssignableFrom(typeof(Values)))
            {
                wrapperBaseType.AddSubType<Values>(ref n);
            }

            if (typeof(TBase).IsAssignableFrom(typeof(NullValue)))
            {
                wrapperBaseType.AddSubType<NullValue>(ref n);
            }
            else
            {
                wrapperBaseType.AddSubType(genericWrapperType.MakeGenericType(typeof(NullValue)), ref n);
            }

            if (typeof(TBase).IsAssignableFrom(typeof(DynamicObjectSurrogate)))
            {
                wrapperBaseType.AddSubType<DynamicObjectSurrogate>(ref n);
            }
            else
            {
                wrapperBaseType.AddSubType(genericWrapperType.MakeGenericType(typeof(DynamicObjectSurrogate)), ref n);
            }

            foreach (var t in WrappedTypes)
            {
                wrapperBaseType.AddSubType(genericWrapperType.MakeGenericType(t), ref n);

                if (addNullableSupport && t.IsValueType)
                {
                    var tNullable = typeof(Nullable<>).MakeGenericType(t);
                    wrapperBaseType.AddSubType(genericWrapperType.MakeGenericType(tNullable), ref n);
                }
            }

            return typeModel;
        }

        internal static ImmutableArray<Type> WrappedTypes { get; } = new[]
        {
            typeof(string),
            typeof(int),
            typeof(byte),
            typeof(bool),
            typeof(double),
            typeof(char),
            typeof(Guid),
            typeof(long),
            typeof(float),
            typeof(decimal),
            typeof(sbyte),
            typeof(uint),
            typeof(ulong),
            typeof(short),
            typeof(ushort),
            typeof(DateTime),
            typeof(TimeSpan),
        }.ToImmutableArray();
    }
}
