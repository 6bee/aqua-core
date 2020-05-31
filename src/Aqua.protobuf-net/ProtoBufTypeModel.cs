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
        private static readonly ImmutableArray<Type> _systemTypes = new[]
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
        /// <param name="addNullableSupport">Indicates whether protobuf-net should suport null values for the specified <see cref="Type"/>.</param>
        /// <returns>The <see cref="AquaTypeModel"/> under configuration.</returns>
        public static AquaTypeModel AddDynamicPropertyType<T>(this AquaTypeModel typeModel, bool addSingleValueSuppoort = true, bool addCollectionSupport = true, bool addNullableSupport = true)
            => AddDynamicPropertyType(true, typeModel, typeof(T), addSingleValueSuppoort, addCollectionSupport, addNullableSupport);

        /// <summary>
        /// Add type configuration for the given <see cref="Type"/> to be supported as payload within a <see cref="DynamicObject"/> and/or <see cref="Property"/>.
        /// </summary>
        /// <param name="typeModel">The <see cref="AquaTypeModel"/> used for model configutation.</param>
        /// <param name="propertyType">The <see cref="Type"/> to be configured as dynamic payload.</param>
        /// <param name="addSingleValueSuppoort">Indicated whether the specified <see cref="Type"/> should be supposted as single value.</param>
        /// <param name="addCollectionSupport">Indicated whether the specified <see cref="Type"/> should be supposted as collection.</param>
        /// <param name="addNullableSupport">Indicates whether protobuf-net should support null values for the specified <see cref="Type"/>.</param>
        /// <returns>The <see cref="AquaTypeModel"/> under configuration.</returns>
        public static AquaTypeModel AddDynamicPropertyType(this AquaTypeModel typeModel, Type propertyType, bool addSingleValueSuppoort = true, bool addCollectionSupport = true, bool addNullableSupport = true)
            => AddDynamicPropertyType(true, typeModel, propertyType, addSingleValueSuppoort, addCollectionSupport, addNullableSupport);

        public static AquaTypeModel AddDynamicPropertyType(bool flag, AquaTypeModel typeModel, Type propertyType, bool addSingleValueSuppoort = true, bool addCollectionSupport = true, bool addNullableSupport = true)
        {
            if (typeModel is null)
            {
                throw new ArgumentNullException(nameof(typeModel));
            }

            if (propertyType is null)
            {
                throw new ArgumentNullException(nameof(propertyType));
            }

            var isNullable = propertyType.IsNullable();
            if (addSingleValueSuppoort)
            {
                var singleValueType = typeof(Value<>).MakeGenericType(propertyType);
                typeModel.AddSubType<Value>(singleValueType);
                if (isNullable && addNullableSupport)
                {
                    typeModel.GetType(singleValueType)[1].SupportNull = true;
                }
            }

            if (addCollectionSupport)
            {
                var collectionType = typeof(Values<>).MakeGenericType(propertyType);
                typeModel.AddSubType<Values>(collectionType);
                if (isNullable && addNullableSupport)
                {
                    typeModel.GetType(collectionType)[1].SupportNull = true;
                }
            }

            if (flag && propertyType.IsValueType && addNullableSupport)
            {
                var invertedNullableType = isNullable
                    ? propertyType.GetGenericArguments()[0]
                    : typeof(Nullable<>).MakeGenericType(propertyType);
                AddDynamicPropertyType(false, typeModel, invertedNullableType, addSingleValueSuppoort, addCollectionSupport, addNullableSupport);
            }

            return typeModel;
        }

        private static void ConfigureDefaultSystemTypes(AquaTypeModel typeModel)
            => _systemTypes.ForEach(t => typeModel.AddDynamicPropertyType(t));
    }
}
