// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Protobuf;

using Aqua.Dynamic;
using Aqua.TypeExtensions;
using Google.Protobuf.Collections;
using System.Buffers;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using Proto = Aqua.Protobuf.Schema;

/// <summary>
/// Maps Aqua object graphs to and from the generated protobuf wire messages.
/// </summary>
internal sealed class ProtobufValueMapper
{
    public static readonly ProtobufValueMapper Instance = new();

    public Proto.Value ToValue(object? value)
    {
        switch (value)
        {
            case null:
                return new Proto.Value { Null = new Proto.NullValue() };

            case string s:
                return new Proto.Value { String = s };

            case DynamicObject dynamicObject:
                return new Proto.Value { DynamicObject = ToDynamicObject(dynamicObject) };

            case Aqua.TypeSystem.TypeInfo typeInfo:
                return new Proto.Value { TypeInfo = ToTypeInfo(typeInfo) };

            case Enum enumValue:
                var underlyingEnumType = Enum.GetUnderlyingType(value.GetType());
                object numericEnumValue = Convert.ChangeType(enumValue, underlyingEnumType);
                return ToValue(numericEnumValue);

            case Array array:
                return ToArrayValue(array);

            case IEnumerable enumerable when value is not IFormattable:
                return new Proto.Value { Collection = ToCollection(enumerable) };

            default:
                return ToScalarValue(value);
        }
    }

    public object? FromValue(Proto.Value value)
    {
        value.AssertNotNull();
        return value.KindCase switch
        {
            Proto.Value.KindOneofCase.Null => null,
            Proto.Value.KindOneofCase.String => value.String,
            Proto.Value.KindOneofCase.Scalar => FromScalar(value.Scalar),
            Proto.Value.KindOneofCase.PackedArray => FromPackedArray(value.PackedArray),
            Proto.Value.KindOneofCase.Collection => FromCollection(value.Collection),
            Proto.Value.KindOneofCase.DynamicObject => FromDynamicObject(value.DynamicObject),
            Proto.Value.KindOneofCase.TypeInfo => FromTypeInfo(value.TypeInfo),
            _ => null,
        };
    }

    private Proto.DynamicObject ToDynamicObject(DynamicObject value)
    {
        var result = new Proto.DynamicObject
        {
            Type = value.Type is null ? null : ToTypeInfo(value.Type),
            HasProperties = value.Properties is not null,
        };

        if (value.Properties is { } properties)
        {
            foreach (var property in properties)
            {
                result.Properties.Add(property.Name, ToValue(property.Value));
            }
        }

        return result;
    }

    private DynamicObject FromDynamicObject(Proto.DynamicObject value)
    {
        var type = value.Type is null ? null : FromTypeInfo(value.Type);
        var properties = value.HasProperties ? FromPropertySet(value.Properties) : null;
        return new DynamicObject(type, properties);
    }

    private Proto.Value ToArrayValue(Array array)
    {
        if (TryPackArray(array, out var elementType, out var bytes))
        {
            return new Proto.Value
            {
                PackedArray = new Proto.PackedArray
                {
                    ElementType = elementType.Value,
                    Data = ByteString.CopyFrom(bytes),
                },
            };
        }

        return new Proto.Value { Collection = ToCollection(array) };

        static bool TryPackArray(Array array, [NotNullWhen(true)] out Proto.DataType? elementTypeKey, [NotNullWhen(true)] out byte[]? bytes)
        {
            elementTypeKey = null;
            bytes = null;

            if (array is null || array.Rank != 1)
            {
                return false;
            }

            var elementType = array.GetType().GetElementType()!;
            if (elementType.IsEnum)
            {
                elementType = Enum.GetUnderlyingType(elementType);
            }

            if (!PackedArraySerializer.IsEligibleElementType(elementType))
            {
                return false;
            }

            elementTypeKey = Proto.DataType.FromType(elementType);
            if (elementTypeKey is null)
            {
                return false;
            }

            bytes = PackedArraySerializer.Pack(array);
            return true;
        }
    }

    private Proto.Collection ToCollection(IEnumerable items)
    {
        var collection = new Proto.Collection();
        foreach (var item in items)
        {
            collection.Items.Add(ToValue(item));
        }

        return collection;
    }

    private object FromCollection(Proto.Collection collection)
    {
        var result = new object?[collection.Items.Count];
        for (var i = 0; i < collection.Items.Count; i++)
        {
            result[i] = FromValue(collection.Items[i]);
        }

        if (result.Length > 0 && Array.TrueForAll(result, static x => x is DynamicObject))
        {
            return result.OfType<DynamicObject>().ToArray();
        }

        return result;
    }

    private Proto.Value ToScalarValue(object value)
    {
        var type = value.GetType();
        var typeCode = Proto.DataType.FromType(type) ?? throw SerializationException($"Value of type {type.GetFriendlyName()} is not supported.");

        var buffer = new ArrayBufferWriter<byte>();

        AquaScalarCodec.Encode(buffer, value);

        return new Proto.Value
        {
            Scalar = new Proto.Scalar
            {
                Type = typeCode,
                Data = ByteString.CopyFrom(buffer.WrittenSpan),
            },
        };
    }

    private static object FromScalar(Proto.Scalar scalar)
    {
        var type = Proto.DataType.ToType(scalar.Type) ?? throw SerializationException($"Unsupported data type code {scalar.Type}.");
        return AquaScalarCodec.Decode(scalar.Data.Span, type);
    }

    private static object FromPackedArray(Proto.PackedArray packedArray)
    {
        var elementType = Proto.DataType.ToType(packedArray.ElementType) ?? throw SerializationException($"Unsupported data type {packedArray.ElementType}.");
        if (!PackedArraySerializer.IsEligibleElementType(elementType))
        {
            throw SerializationException($"Type key '{packedArray.ElementType}' does not denote a packed-array-eligible element type.");
        }

        return PackedArraySerializer.Unpack(packedArray.Data.ToByteArray(), elementType);
    }

    private PropertySet FromPropertySet(MapField<string, Proto.Value> value)
    {
        var properties = new List<Property>(value.Count);
        foreach (var item in value)
        {
            properties.Add(new(item.Key, item.Value is null ? null : FromValue(item.Value)));
        }

        return new(properties);
    }

    private Proto.TypeInfo ToTypeInfo(Aqua.TypeSystem.TypeInfo value)
    {
        var result = new Proto.TypeInfo
        {
            Name = value.Name ?? string.Empty,
            Namespace = value.Namespace ?? string.Empty,
            DeclaringType = value.DeclaringType is null ? null : ToTypeInfo(value.DeclaringType),
            IsAnonymousType = value.IsAnonymousType,
            IsGenericType = value.IsGenericType,
        };

        if (value.GenericArguments is not null)
        {
            foreach (var argument in value.GenericArguments)
            {
                result.GenericArguments.Add(ToTypeInfo(argument));
            }
        }

        if (value.Properties is not null)
        {
            foreach (var property in value.Properties)
            {
                result.Properties.Add(ToPropertyInfo(property));
            }
        }

        return result;
    }

    private Aqua.TypeSystem.TypeInfo FromTypeInfo(Proto.TypeInfo value)
    {
        var result = new Aqua.TypeSystem.TypeInfo
        {
            Name = value.Name.Length == 0 ? null : value.Name,
            Namespace = value.Namespace.Length == 0 ? null : value.Namespace,
            DeclaringType = value.DeclaringType is null ? null : FromTypeInfo(value.DeclaringType),
            IsAnonymousType = value.IsAnonymousType,
            IsGenericType = value.IsGenericType,
        };

        if (value.GenericArguments.Count > 0)
        {
            var arguments = new List<Aqua.TypeSystem.TypeInfo>(value.GenericArguments.Count);
            foreach (var argument in value.GenericArguments)
            {
                arguments.Add(FromTypeInfo(argument));
            }

            result.GenericArguments = arguments;
        }

        if (value.Properties.Count > 0)
        {
            var properties = new List<Aqua.TypeSystem.PropertyInfo>(value.Properties.Count);
            foreach (var property in value.Properties)
            {
                properties.Add(FromPropertyInfo(property));
            }

            result.Properties = properties;
        }

        return result;
    }

    private Proto.PropertyInfo ToPropertyInfo(Aqua.TypeSystem.PropertyInfo value)
    {
        var result = new Proto.PropertyInfo
        {
            Name = value.Name ?? string.Empty,
            DeclaringType = value.DeclaringType is null ? null : ToTypeInfo(value.DeclaringType),
            PropertyType = value.PropertyType is null ? null : ToTypeInfo(value.PropertyType),
        };

        if (value.IsStatic.HasValue)
        {
            result.IsStatic = value.IsStatic.Value;
        }

        return result;
    }

    private Aqua.TypeSystem.PropertyInfo FromPropertyInfo(Proto.PropertyInfo value)
    {
        var name = value.Name.Length == 0 ? null : value.Name;
        var declaringType = value.DeclaringType is null ? null : FromTypeInfo(value.DeclaringType);
        var propertyType = value.PropertyType is null ? null : FromTypeInfo(value.PropertyType);
        return new Aqua.TypeSystem.PropertyInfo(name!, propertyType!, declaringType)
        {
            IsStatic = value.HasIsStatic ? value.IsStatic : null,
        };
    }

    private static ProtobufSerializationException SerializationException(string message) => new(message);
}
