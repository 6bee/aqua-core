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
    using System.Linq;

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
            return typeModel
              .ConfigureTypedValueWrapper<Value>(typeof(Value<>))
              .ConfigureTypedValueWrapper<Values>(typeof(Values<>));
        }

        private static RuntimeTypeModel ConfigureTypedValueWrapper<TBase>(this RuntimeTypeModel typeModel, Type genericWrapperType)
        {
            var wrapperBaseType = typeModel.GetType<TBase>();
            var n = 10;
            foreach (var t in SystemTypes)
            {
                wrapperBaseType.AddSubType(genericWrapperType.MakeGenericType(t), ref n);
            }

            return typeModel;
        }

        internal static ImmutableArray<Type> SystemTypes { get; } = new[]
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
            }
            .SelectMany(x => x.IsValueType ? new[] { x, typeof(Nullable<>).MakeGenericType(x) } : new[] { x })
            .ToImmutableArray();
    }
}
