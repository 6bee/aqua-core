// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.ProtoBuf;

using Aqua.Dynamic;
using Aqua.TypeExtensions;
using global::ProtoBuf.Meta;
using System;
using System.Diagnostics.CodeAnalysis;

public class AquaTypeModel
{
    internal AquaTypeModel(RuntimeTypeModel typeModel)
    {
        typeModel.AssertNotNull();

        Model = typeModel;

        typeModel.AutoAddMissingTypes = true;
        typeModel.AllowParseableTypes = false;
        typeModel.AutoCompile = true;
    }

    public RuntimeTypeModel Model { get; }

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

    public MetaType GetType<T>() => GetType(typeof(T));

    public MetaType GetType(Type type) => Model[type];

    public TypeModel Compile() => Model.Compile();

    /// <summary>
    /// Add type configuration for the given <see cref="Type"/> to be supported as payload within a <see cref="DynamicObject"/> and/or <see cref="Property"/>.
    /// </summary>
    /// <typeparam name="T">The <see cref="Type"/> to be configured as dynamic payload.</typeparam>
    /// <param name="addSingleValueSuppoort">Indicated whether the specified <see cref="Type"/> should be supposted as single value.</param>
    /// <param name="addCollectionSupport">Indicated whether the specified <see cref="Type"/> should be supposted as collection.</param>
    /// <param name="addNullableSupport">Indicates whether protobuf-net should suport <see langword="null"/> values for the specified <see cref="Type"/>.</param>
    /// <returns>The <see cref="AquaTypeModel"/> under configuration.</returns>
    public AquaTypeModel AddDynamicPropertyType<T>(bool addSingleValueSuppoort = true, bool addCollectionSupport = true, bool addNullableSupport = true)
        => AddDynamicPropertyType(true, typeof(T), addSingleValueSuppoort, addCollectionSupport, addNullableSupport);

    /// <summary>
    /// Add type configuration for the given <see cref="Type"/> to be supported as payload within a <see cref="DynamicObject"/> and/or <see cref="Property"/>.
    /// </summary>
    /// <param name="propertyType">The <see cref="Type"/> to be configured as dynamic payload.</param>
    /// <param name="addSingleValueSuppoort">Indicated whether the specified <see cref="Type"/> should be supposted as single value.</param>
    /// <param name="addCollectionSupport">Indicated whether the specified <see cref="Type"/> should be supposted as collection.</param>
    /// <param name="addNullableSupport">Indicates whether protobuf-net should support <see langword="null"/> values for the specified <see cref="Type"/>.</param>
    /// <returns>The <see cref="AquaTypeModel"/> under configuration.</returns>
    public AquaTypeModel AddDynamicPropertyType(Type propertyType, bool addSingleValueSuppoort = true, bool addCollectionSupport = true, bool addNullableSupport = true)
        => AddDynamicPropertyType(true, propertyType, addSingleValueSuppoort, addCollectionSupport, addNullableSupport);

    private AquaTypeModel AddDynamicPropertyType(bool flag, Type propertyType, bool addSingleValueSuppoort, bool addCollectionSupport, bool addNullableSupport)
    {
        if (propertyType is null)
        {
            throw new ArgumentNullException(nameof(propertyType));
        }

        if (!Model.CanSerialize(propertyType))
        {
            AddType(propertyType);
        }

        var isNullable = propertyType.IsNullableType();
        if (addSingleValueSuppoort)
        {
            var singleValueType = typeof(Value<>).MakeGenericType(propertyType);
            AddSubType<Value>(singleValueType);
        }

        if (addCollectionSupport)
        {
            var collectionType = typeof(Values<>).MakeGenericType(propertyType);
            AddSubType<Values>(collectionType);
            if (isNullable && addNullableSupport)
            {
                var nullableCollectionType = typeof(NullableValues<>).MakeGenericType(propertyType);
                AddSubType<Values>(nullableCollectionType);
            }
        }

        if (flag && propertyType.IsValueType && addNullableSupport)
        {
            var invertedNullableType = isNullable
                ? propertyType.GetGenericArguments()[0]
                : typeof(Nullable<>).MakeGenericType(propertyType);
            AddDynamicPropertyType(false, invertedNullableType, addSingleValueSuppoort, addCollectionSupport, addNullableSupport);
        }

        return this;
    }

    [SuppressMessage("Usage", "CA2225:Operator overloads have named alternates", Justification = "Model property may be used")]
    public static implicit operator RuntimeTypeModel(AquaTypeModel model) => model.CheckNotNull().Model;
}