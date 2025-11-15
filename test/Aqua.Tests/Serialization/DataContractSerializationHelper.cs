// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Tests.Serialization;

using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.Serialization;
using System.Xml;
using Xunit;

public static class DataContractSerializationHelper
{
    private sealed class CustomDataContractResolver : DataContractResolver
    {
        public override Type ResolveName(string typeName, string typeNamespace, Type declaredType, DataContractResolver knownTypeResolver)
        {
            var type = knownTypeResolver.ResolveName(typeName, typeNamespace, declaredType, knownTypeResolver);
            if (type is not null)
            {
                return type;
            }

            var isArray = 0;
            var isNullable = false;
            if (typeName.StartsWith("ArrayOf_", StringComparison.Ordinal))
            {
                typeName = typeName[8..];
                isArray++;
            }

            if (typeName.StartsWith("ListOf_", StringComparison.Ordinal))
            {
                typeName = typeName[7..];
                isArray++;
            }

            if (typeName.StartsWith("ArrayOf", StringComparison.Ordinal))
            {
                typeName = typeName[7..];
                isArray++;
            }

            if (typeName.StartsWith("ListOf", StringComparison.Ordinal))
            {
                typeName = typeName[6..];
                isArray++;
            }

            if (typeName.StartsWith("NullableOf", StringComparison.Ordinal))
            {
                typeName = typeName[10..];
                isNullable = true;
            }

            var typeMap = new Dictionary<string, string>
            {
                { "decimal", typeof(decimal).FullName },
                { "string", typeof(string).FullName },
                { "object", typeof(object).FullName },
            };

            if (typeMap.TryGetValue(typeName, out var t))
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
                    catch
                    {
                        return null;
                    }
                })
                .FirstOrDefault(x => x is not null);
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

            if (name is not null)
            {
                var dictionary = new XmlDictionary();
                typeName = dictionary.Add(name);
                typeNamespace = dictionary.Add("http://schemas.microsoft.com/2003/10/Serialization/Arrays");
                return true;
            }

            return knownTypeResolver.TryResolveType(type, declaredType, knownTypeResolver, out typeName, out typeNamespace);
        }
    }

    public static T Clone<T>(this T graph) => Clone(graph, null);

    [SuppressMessage("Major Code Smell", "S125:Sections of code should not be commented out", Justification = "Debugging purpose")]
    public static T Clone<T>(this T graph, Type[] knownTypes)
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

    public static void SkipUnsupportedDataType(Type type, object value)
    {
#if NET5_0_OR_GREATER
        Skip.If(type.Is<Half>(), $"{type} serialization is not supported.");
#endif // NET5_0_OR_GREATER
#if NET6_0_OR_GREATER
        Skip.If(type.Is<DateOnly>(), $"{type} serialization is not supported.");
        Skip.If(type.Is<TimeOnly>(), $"{type} serialization is not supported.");
#endif // NET6_0_OR_GREATER
#if NET7_0_OR_GREATER
        Skip.If(type.Is<Int128>(), $"{type} serialization is not supported.");
        Skip.If(type.Is<UInt128>(), $"{type} serialization is not supported.");
#endif // NET7_0_OR_GREATER
    }
}