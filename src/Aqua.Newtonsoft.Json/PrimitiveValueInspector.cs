// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua
{
    using Aqua.Dynamic;
    using Aqua.TypeSystem;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.Serialization;
    using System.Text.RegularExpressions;

    public static class PrimitiveValueInspector
    {
        private static readonly Regex _arrayNameRegex = new Regex(@"^(.*)\[\,*\]$");

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
                { typeof(ulong), x => Convert.ToUInt64(x) },
                { typeof(float), x => Convert.ToSingle(x) },
                //{ typeof(double), x => Convert.ToDouble(x) },
                { typeof(decimal), x => Convert.ToDecimal(x) },
                { typeof(char), x => Convert.ToChar(x) },
                //{ typeof(bool), x => Convert.ToBoolean(x) },
            }
            .ToDictionary(k => k.Key.FullName, v => v.Value);

        internal static void DynamicObjectSerializationCallback(object o, StreamingContext context)
        {
            var dynamicObject = o as DynamicObject;
            if (!ReferenceEquals(null, dynamicObject))
            {
                var type = dynamicObject.Type;
                if (!ReferenceEquals(null, type))
                {
                    if (dynamicObject.Values?.Count() == 1)
                    {
                        var enumerable = dynamicObject.Values.Single() as IEnumerable;
                        if (!ReferenceEquals(null, enumerable))
                        {
                            TypeInfo elementType = null;
                            var array = _arrayNameRegex.Match(type.Name);
                            if (array.Success)
                            {
                                elementType = new TypeInfo(type);
                                elementType.Name = array.Groups[1].Value;
                            }
                            else if (type.GenericArguments?.Count == 1)
                            {
                                elementType = type.GenericArguments.Single();
                            }

                            if (!ReferenceEquals(null, elementType))
                            {
                                var converter = GetConverter(elementType);
                                if (!ReferenceEquals(null, converter))
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

                    var properties = type.Properties;
                    if (!ReferenceEquals(null, properties))
                    {
                        foreach (var property in properties)
                        {
                            var converter = GetConverter(property.PropertyType);
                            if (!ReferenceEquals(null, converter))
                            {
                                var dynamicProperty = dynamicObject.Properties
                                    .SingleOrDefault(x => string.Equals(x.Name, property.Name));
                                if (!ReferenceEquals(null, dynamicProperty))
                                {
                                    dynamicProperty.Value = converter(dynamicProperty.Value);
                                }
                            }
                        }
                    }
                }
            }
        }

        public static Func<object, object> GetConverter(Aqua.TypeSystem.TypeInfo typeInfo)
        {
            if (ReferenceEquals(null, typeInfo))
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
                if (!ReferenceEquals(null, converter))
                {
                    return x => ReferenceEquals(null, x) ? null : converter(x);
                }
            }

            return null;
        }
    }
}
