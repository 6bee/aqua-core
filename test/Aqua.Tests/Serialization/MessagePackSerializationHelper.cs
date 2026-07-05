// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Tests.Serialization;

using Aqua.TypeExtensions;
using global::MessagePack;
using System.IO;

public static class MessagePackSerializationHelper
{
    public static T Clone<T>(this T graph)
    {
        var options = MessagePackSerializerOptions.Standard.ConfigureAqua();

        using var stream = new MemoryStream();
        MessagePackSerializer.Serialize(stream, graph, options);

        stream.Position = 0;
        var copy = MessagePackSerializer.Deserialize<T>(stream, options);

        return copy;
    }

    private static readonly Dictionary<Type, Func<object, object>> _transformers = new()
    {
        [typeof(DateTime)] = v => ((DateTime)v).ToUniversalTime(),
    };

    /// <summary>
    /// <para>
    ///   MessagePack encoder transforms DateTime to UTC.
    /// </para>
    /// <para>
    ///   lossy encoding:
    ///   <list type="bullet">
    ///     <item>getting rid of local date kind</item>
    ///   </list>
    /// </para>
    /// <para>
    ///   Therefore, we adjust test data to prevent test failing for lost precision due to our protobuf codec.
    /// </para>
    /// </summary>
    public static void PatchTestData(Type type, ref object value)
    {
        value = Patch(value);

        static object Patch(object value)
        {
            if (value is null)
            {
                return null;
            }

            // unwrap nullable (boxed)
            var type = value.GetType().AsNonNullableType();

            // scalar case
            if (_transformers.TryGetValue(type, out var f))
            {
                return f(value);
            }

            // arrays
            if (value is Array arr)
            {
                var elementType = type.GetElementType()!;
                var result = Array.CreateInstance(elementType, arr.Length);

                for (int i = 0; i < arr.Length; i++)
                {
                    result.SetValue(Patch(arr.GetValue(i)), i);
                }

                return result;
            }

            // generic List<T> or similar
            if (value is System.Collections.IList list && type.IsGenericType)
            {
                var elementType = type.GetGenericArguments()[0];
                var result = (System.Collections.IList)Activator.CreateInstance(type)!;

                foreach (var item in list)
                {
                    result.Add(Patch(item));
                }

                return result;
            }

            return value;
        }
    }
}
