// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua
{
    using Aqua.Dynamic;
    using Aqua.TypeSystem;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Numerics;
    using System.Runtime.Serialization;
    using System.Text.RegularExpressions;

    public static class NativeValueInspector
    {
        private static readonly Regex _arrayNameRegex = new Regex(@"^(.+)\[\,*\]$");

        private static readonly Dictionary<string, Func<object, object>> _converterMap =
            new Dictionary<Type, Func<object, object>>
            {
                { typeof(int), x => Convert.ToInt32(x) },
                { typeof(uint), x => Convert.ToUInt32(x) },
                { typeof(byte), x => Convert.ToByte(x) },
                { typeof(sbyte), x => Convert.ToSByte(x) },
                { typeof(short), x => Convert.ToInt16(x) },
                { typeof(ushort), x => Convert.ToUInt16(x) },
                { typeof(long), x => Convert.ToInt64(x) },
                { typeof(ulong), x => x is BigInteger ? (ulong)(BigInteger)x : Convert.ToUInt64(x) },
                { typeof(float), x => Convert.ToSingle(x) },
                { typeof(decimal), x => Convert.ToDecimal(x) },
                { typeof(char), x => Convert.ToChar(x) },
                { typeof(Guid), x => x is string ? Guid.Parse((string)x) : x },
                { typeof(TimeSpan), x => x is string ? TimeSpan.Parse((string)x) : x },
                { typeof(DateTimeOffset), x => x is DateTime ? new DateTimeOffset(((DateTime)x).ToLocalTime()) : x },
                { typeof(BigInteger), x => x is long ? new BigInteger((long)x) : x },

                // { typeof(double), x => Convert.ToDouble(x) },
                // { typeof(bool), x => Convert.ToBoolean(x) },
                // { typeof(Complex), x => x },
            }
            .ToDictionary(k => k.Key.FullName, v => v.Value);

        internal static void DynamicObjectSerializationCallback(object o, StreamingContext context)
        {
            if (o is DynamicObject dynamicObject)
            {
                var type = dynamicObject.Type;
                if (!(type is null))
                {
                    if (dynamicObject.Values?.Count() == 1)
                    {
                        var value = dynamicObject.Values.Single();
                        if (value is IEnumerable enumerable && !(value is string))
                        {
                            if (value is byte[])
                            {
                                return;
                            }

                            ConvertArrayProperty(dynamicObject, type, enumerable);
                        }
                        else
                        {
                            ConvertProperty(dynamicObject, type, value);
                        }
                    }
                    else
                    {
                        ConvertProperties(dynamicObject, type);
                    }
                }
            }
        }

        private static void ConvertProperty(DynamicObject dynamicObject, TypeInfo type, object value)
        {
            var converter = GetConverter(type);
            if (!(converter is null))
            {
                dynamicObject.Properties.Single().Value = converter(value);
            }
        }

        public static Func<object, object> GetConverter(TypeInfo typeInfo)
        {
            if (typeInfo is null)
            {
                return null;
            }

            Func<object, object> converter;
            if (_converterMap.TryGetValue(typeInfo.FullName, out converter))
            {
                return converter;
            }

            if (string.Equals(typeInfo.FullName, typeof(Nullable<>).FullName) && typeInfo.GenericArguments?.Count == 1)
            {
                converter = GetConverter(typeInfo.GenericArguments.Single());
                if (!(converter is null))
                {
                    return x => x is null ? null : converter(x);
                }
            }

            return null;
        }

        private static TypeInfo GetArrayElementType(TypeInfo type)
        {
            TypeInfo elementType;
            var array = _arrayNameRegex.Match(type.Name);
            if (array.Success)
            {
                elementType = new TypeInfo(type) { Name = array.Groups[1].Value };
            }
            else if (type.GenericArguments?.Count == 1)
            {
                elementType = type.GenericArguments.Single();
                array = _arrayNameRegex.Match(elementType.Name);
                if (array.Success)
                {
                    elementType.Name = array.Groups[1].Value;
                }

                // would be nice to be able to detect collection types... to retrieve the element type.
                // i.e. else (elementType.Implements(typeof(IEnumerable<>))) { ... }
            }
            else
            {
                elementType = null;
            }

            return elementType;
        }

        private static void ConvertProperties(DynamicObject dynamicObject, TypeInfo type)
        {
            var properties = type.Properties;
            if (!(properties is null))
            {
                foreach (var property in properties)
                {
                    var converter = GetConverter(property.PropertyType);
                    if (!(converter is null))
                    {
                        var dynamicProperty = dynamicObject.Properties.SingleOrDefault(x => string.Equals(x.Name, property.Name));
                        if (!(dynamicProperty is null))
                        {
                            dynamicProperty.Value = converter(dynamicProperty.Value);
                        }
                    }
                }
            }
        }

        private static void ConvertArrayProperty(DynamicObject dynamicObject, TypeInfo type, IEnumerable enumerable)
        {
            var elementType = GetArrayElementType(type);
            if (!(elementType is null))
            {
                var converter = GetConverter(elementType);
                if (!(converter is null))
                {
                    var convertedValues = enumerable
                        .Cast<object>()
                        .Select(converter)
                        .ToArray();
                    dynamicObject.Properties.Single().Value = convertedValues;
                }
            }
        }
    }
}
