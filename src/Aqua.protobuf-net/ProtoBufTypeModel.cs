// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua
{
    using Aqua.Extensions;
    using Aqua.ProtoBuf;
    using Aqua.ProtoBuf.Dynamic;
    using Aqua.ProtoBuf.TypeSystem;
    using global::ProtoBuf.Meta;
    using System;
    using System.Collections.Generic;
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

            if (typeof(TBase).IsAssignableFrom(typeof(NullValue)))
            {
                wrapperBaseType.AddSubType<NullValue>(ref n);
            }

            if (typeof(TBase) != typeof(Values) && typeof(TBase).IsAssignableFrom(typeof(Values)))
            {
                wrapperBaseType.AddSubType<Values>(ref n);
            }

            if (typeof(TBase).IsAssignableFrom(typeof(DynamicObjectSurrogate)))
            {
                wrapperBaseType.AddSubType<DynamicObjectSurrogate>(ref n);
            }

            foreach (var t in GetPrimitiveValueTypes())
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

        private static IEnumerable<Type> GetPrimitiveValueTypes()
        {
            yield return typeof(string);
            yield return typeof(int);
            yield return typeof(byte);
            yield return typeof(bool);
            yield return typeof(char);
            yield return typeof(Guid);
            yield return typeof(long);
            yield return typeof(float);
            yield return typeof(double);
            yield return typeof(decimal);
            yield return typeof(sbyte);
            yield return typeof(uint);
            yield return typeof(ulong);
            yield return typeof(short);
            yield return typeof(ushort);
            yield return typeof(DateTime);
            yield return typeof(TimeSpan);
        }
    }
}
