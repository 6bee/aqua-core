// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Protobuf.Mappers;

using Aqua.Dynamic;
using Aqua.TypeExtensions;
using Aqua.TypeSystem;
using System.Buffers;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using Proto = Aqua.Protobuf.Schema;

public class ValueMapper : ProtoMapper<object?, Proto.Value>
{
    public static readonly ValueMapper Instance = new();

    public override object? FromProto(Proto.Value proto, ProtoContext context)
        => proto is null ? null : proto.KindCase switch
        {
            Proto.Value.KindOneofCase.Null => null,
            Proto.Value.KindOneofCase.String => proto.String,
            Proto.Value.KindOneofCase.Scalar => FromScalarProto(proto.Scalar, context.Options),
            Proto.Value.KindOneofCase.PackedArray => FromPackedArray(proto.PackedArray),
            Proto.Value.KindOneofCase.Collection => FromCollection(proto.Collection, context),

            Proto.Value.KindOneofCase.DynamicObject => DynamicObjectMapper.Instance.FromProto(proto.DynamicObject, context),

            Proto.Value.KindOneofCase.TypeInfo => TypeInfoMapper.Instance.FromProto(proto.TypeInfo, context),
            Proto.Value.KindOneofCase.MemberInfo => MemberInfoMapper.Instance.FromProto(proto.MemberInfo, context),
            Proto.Value.KindOneofCase.PropertyInfo => PropertyInfoMapper.Instance.FromProto(proto.PropertyInfo, context),
            Proto.Value.KindOneofCase.FieldInfo => FieldInfoMapper.Instance.FromProto(proto.FieldInfo, context),
            Proto.Value.KindOneofCase.ConstructorInfo => ConstructorInfoMapper.Instance.FromProto(proto.ConstructorInfo, context),
            Proto.Value.KindOneofCase.MethodInfo => MethodInfoMapper.Instance.FromProto(proto.MethodInfo, context),

            _ => null, // not reachable
        };

    public override Proto.Value ToProto(object? value, ProtoContext context)
        => value switch
        {
            null => new() { Null = new Proto.NullValue() },
            string s => new() { String = s },
            Array array => FromArray(array, context),
            IEnumerable enumerable when value is not IFormattable => new() { Collection = ToCollection(enumerable, context) },

            DynamicObject dynamicObject => new() { DynamicObject = DynamicObjectMapper.Instance.ToProto(dynamicObject, context) },

            TypeInfo typeInfo => new() { TypeInfo = TypeInfoMapper.Instance.ToProto(typeInfo, context) },
            PropertyInfo propertyInfo => new() { PropertyInfo = PropertyInfoMapper.Instance.ToProto(propertyInfo, context) },
            FieldInfo fieldInfo => new() { FieldInfo = FieldInfoMapper.Instance.ToProto(fieldInfo, context) },
            ConstructorInfo constructorInfo => new() { ConstructorInfo = ConstructorInfoMapper.Instance.ToProto(constructorInfo, context) },
            MethodInfo methodInfo => new() { MethodInfo = MethodInfoMapper.Instance.ToProto(methodInfo, context) },
            MemberInfo memberInfo => new() { MemberInfo = MemberInfoMapper.Instance.ToProto(memberInfo, context) },

            _ => FromScalarValue(value, context.Options),
        };

    protected virtual Proto.Value FromScalarValue(object value, ProtoOptions options)
    {
        var type = value.GetType();
        if (type.IsEnum)
        {
            type = Enum.GetUnderlyingType(type);
            value = Convert.ChangeType(value, Enum.GetUnderlyingType(type));
        }

        var typeCode = Proto.Scalar.Types.DataType.FromType(type) ?? throw SerializationException($"Value of type {value.GetType().GetFriendlyName()} is not supported.");

        var buffer = new ArrayBufferWriter<byte>();

        AquaScalarCodec.Encode(buffer, value, options);

        return new Proto.Value
        {
            Scalar = new Proto.Scalar
            {
                Type = typeCode,
                Data = ByteString.CopyFrom(buffer.WrittenSpan),
            },
        };
    }

    protected virtual object FromScalarProto(Proto.Scalar scalar, ProtoOptions options)
    {
        var type = Proto.Scalar.Types.DataType.ToType(scalar.Type)
            ?? throw SerializationException($"Unsupported data type {scalar.Type}.");
        return AquaScalarCodec.Decode(scalar.Data.Span, type, options);
    }

    private Proto.Value FromArray(Array array, ProtoContext context)
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

        return new Proto.Value { Collection = ToCollection(array, context) };

        static bool TryPackArray(Array array, [NotNullWhen(true)] out Proto.PackedArray.Types.ElementType? elementTypeKey, [NotNullWhen(true)] out byte[]? bytes)
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

            elementTypeKey = (Proto.PackedArray.Types.ElementType?)(int?)Proto.Scalar.Types.DataType.FromType(elementType);
            if (elementTypeKey is null)
            {
                return false;
            }

            bytes = PackedArraySerializer.Pack(array);
            return true;
        }
    }

    private Proto.Collection ToCollection(IEnumerable items, ProtoContext context)
    {
        var result = new Proto.Collection();
        this.ToProto(result.Items, items, context);
        return result;
    }

    private static object FromPackedArray(Proto.PackedArray packedArray)
    {
        var elementType = Proto.Scalar.Types.DataType.ToType((Proto.Scalar.Types.DataType)packedArray.ElementType)
            ?? throw SerializationException($"Unsupported data type {packedArray.ElementType}.");
        if (!PackedArraySerializer.IsEligibleElementType(elementType))
        {
            throw SerializationException($"{packedArray.ElementType} does not denote a packed-array-eligible element type.");
        }

        return PackedArraySerializer.Unpack(packedArray.Data.ToByteArray(), elementType);
    }

    private object FromCollection(Proto.Collection collection, ProtoContext context)
    {
        var result = this.FromProto(collection.Items, context);

        if (result.Length > 0 && Array.TrueForAll(result, static x => x is DynamicObject))
        {
            return result.OfType<DynamicObject>().ToArray();
        }

        return result;
    }

    private static ProtobufSerializationException SerializationException(string message) => new(message);
}
