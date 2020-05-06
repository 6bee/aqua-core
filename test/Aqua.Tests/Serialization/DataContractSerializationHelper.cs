// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Tests.Serialization
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Runtime.Serialization;
    using System.Xml;

    public static class DataContractSerializationHelper
    {
        private sealed class CustomDataContractResolver : DataContractResolver
        {
            public override Type ResolveName(string typeName, string typeNamespace, Type declaredType, DataContractResolver knownTypeResolver)
            {
                var type = knownTypeResolver.ResolveName(typeName, typeNamespace, declaredType, knownTypeResolver);
                if (type != null)
                {
                    return type;
                }

                var isArray = 0;
                var isNullable = false;
                if (typeName.StartsWith("ArrayOf_"))
                {
                    typeName = typeName.Substring(8);
                    isArray++;
                }

                if (typeName.StartsWith("ListOf_"))
                {
                    typeName = typeName.Substring(7);
                    isArray++;
                }

                if (typeName.StartsWith("ArrayOf"))
                {
                    typeName = typeName.Substring(7);
                    isArray++;
                }

                if (typeName.StartsWith("ListOf"))
                {
                    typeName = typeName.Substring(6);
                    isArray++;
                }

                if (typeName.StartsWith("NullableOf"))
                {
                    typeName = typeName.Substring(10);
                    isNullable = true;
                }

                var typeMap = new Dictionary<string, string>
                {
                    { "decimal", typeof(decimal).FullName },
                    { "string", typeof(string).FullName },
                    { "object", typeof(object).FullName },
                };

                if (typeMap.TryGetValue(typeName, out string t))
                {
                    typeName = t;
                }

                type = AppDomain.CurrentDomain
                    .GetAssemblies()
                    .Where(x => !x.IsDynamic)
                    .Select(x =>
                    {
                        try
                        {
                            return x.GetType(typeName);
                        }
                        catch (Exception ex)
                        {
                            _ = ex;
                            return null;
                        }
                    })
                    .Where(x => x != null)
                    .FirstOrDefault();
                if (isNullable)
                {
                    type = typeof(Nullable<>).MakeGenericType(type);
                }

                while (isArray-- > 0)
                {
                    type = type.MakeArrayType();
                }

                return type;
            }

            public override bool TryResolveType(Type type, Type declaredType, DataContractResolver knownTypeResolver, out XmlDictionaryString typeName, out XmlDictionaryString typeNamespace)
            {
                string name = null;
                if (type.IsArray && type.GetElementType() != typeof(object))
                {
                    name = $"ArrayOf{type.GetElementType().FullName}";
                }
                else if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
                {
                    name = $"ListOf{type.GetGenericArguments().Single().FullName}";
                }

                if (name != null)
                {
                    var dictionary = new XmlDictionary();
                    typeName = dictionary.Add(name);
                    typeNamespace = dictionary.Add("http://schemas.microsoft.com/2003/10/Serialization/Arrays");
                    return true;
                }

                return knownTypeResolver.TryResolveType(type, declaredType, knownTypeResolver, out typeName, out typeNamespace);
            }
        }

        public static T Serialize<T>(this T graph) => Serialize(graph, null);

        public static T Serialize<T>(this T graph, Type[] knownTypes)
        {
            var serializer = new DataContractSerializer(typeof(T), new DataContractSerializerSettings
            {
                DataContractResolver = new CustomDataContractResolver(),
                KnownTypes = knownTypes,
            });

            using var stream = new MemoryStream();

            serializer.WriteObject(stream, graph);
            stream.Seek(0, SeekOrigin.Begin);
            try
            {
                return (T)serializer.ReadObject(stream);
            }
            catch
            {
                // var filename = $"Dump-{graph?.GetType().Name}-DataContractSerializer-{Guid.NewGuid()}.xml";
                // stream.Dump(filename);
                throw;
            }
        }
    }
}
