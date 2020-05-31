// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua
{
    using Aqua.Dynamic;
    using Aqua.Extensions;
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
        public static AquaTypeModel ConfigureAquaTypes(string? name = null, bool configureDefaultSystemTypes = true)
            => ConfigureAquaTypes(RuntimeTypeModel.Create(name), configureDefaultSystemTypes);

        public static AquaTypeModel ConfigureAquaTypes(this RuntimeTypeModel typeModel, bool configureDefaultSystemTypes = true)
        {
            var model = new AquaTypeModel(typeModel);

            if (configureDefaultSystemTypes)
            {
                ConfigureDefaultSystemTypes(model);
            }

            return model
                .ConfigureAquaTypeSystemTypes()
                .ConfigureAquaDynamicTypes();
        }

        /// <summary>
        /// Add type configuration for the given <see cref="Type"/> to be supported as payload within a <see cref="DynamicObject"/> and/or <see cref="Property"/>.
        /// </summary>
        /// <typeparam name="T">The <see cref="Type"/> to be configured as dynamic payload.</typeparam>
        /// <param name="typeModel">The <see cref="AquaTypeModel"/> used for model configutation.</param>
        /// <param name="addSingleValueSuppoort">Indicated whether the specified <see cref="Type"/> should be supposted as single value.</param>
        /// <param name="addCollectionSupport">Indicated whether the specified <see cref="Type"/> should be supposted as collection.</param>
        /// <param name="addNullableSupport">Indicates whether the specified <see cref="Type"/> should be supported as both, <see cref="Nullable{T}"/> as well as <typeparamref name="T"/>. <br/>
        /// This argument is is relevant for value types only and is ignored for reference types.</param>
        /// <returns>The <see cref="AquaTypeModel"/> under configuration.</returns>
        public static AquaTypeModel AddDynamicPropertyType<T>(this AquaTypeModel typeModel, bool addSingleValueSuppoort = true, bool addCollectionSupport = true, bool addNullableSupport = true)
            => typeModel.AddDynamicPropertyType(typeof(T), addSingleValueSuppoort, addCollectionSupport, addNullableSupport);

#pragma warning disable SA1629 // Documentation text should end with a period
        /// <summary>
        /// Add type configuration for the given <see cref="Type"/> to be supported as payload within a <see cref="DynamicObject"/> and/or <see cref="Property"/>.
        /// </summary>
        /// <param name="typeModel">The <see cref="AquaTypeModel"/> used for model configutation.</param>
        /// <param name="propertyType">The <see cref="Type"/> to be configured as dynamic payload.</param>
        /// <param name="addSingleValueSuppoort">Indicated whether the specified <see cref="Type"/> should be supposted as single value.</param>
        /// <param name="addCollectionSupport">Indicated whether the specified <see cref="Type"/> should be supposted as collection.</param>
        /// <param name="addNullableSupport">Indicates whether the specified <see cref="Type"/> should be supported as both, <see cref="Nullable{T}"/> as well as <code>T</code>. <br/>
        /// This argument is is relevant for value types only and is ignored for reference types.</param>
        /// <returns>The <see cref="AquaTypeModel"/> under configuration.</returns>
#pragma warning restore SA1629 // Documentation text should end with a period
        public static AquaTypeModel AddDynamicPropertyType(this AquaTypeModel typeModel, Type propertyType, bool addSingleValueSuppoort = true, bool addCollectionSupport = true, bool addNullableSupport = true)
        {
            if (addSingleValueSuppoort)
            {
                typeModel.AddSubType<Value>(typeof(Value<>).MakeGenericType(propertyType));
            }

            if (addCollectionSupport)
            {
                typeModel.AddSubType<Values>(typeof(Values<>).MakeGenericType(propertyType));
            }

            if (propertyType.IsValueType && addNullableSupport)
            {
                var invertedNullableType = propertyType.IsNullable()
                    ? propertyType.GetGenericArguments()[0]
                    : typeof(Nullable<>).MakeGenericType(propertyType);
                typeModel.AddDynamicPropertyType(invertedNullableType, addSingleValueSuppoort, addCollectionSupport, false);
            }

            return typeModel;
        }

        private static bool IsNullable(this Type type) => type.IsGenericType && typeof(Nullable<>) == type.GetGenericTypeDefinition();

        private static void ConfigureDefaultSystemTypes(AquaTypeModel typeModel)
            => SystemTypes.ForEach(t => typeModel.AddDynamicPropertyType(t));

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
            .ToImmutableArray();
    }
}
