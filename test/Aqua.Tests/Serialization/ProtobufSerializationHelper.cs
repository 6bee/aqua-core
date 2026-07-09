// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Tests.Serialization;

using Aqua.Protobuf;
using Aqua.TypeExtensions;

public static class ProtobufSerializationHelper
{
    public static T Clone<T>(this T graph)
    {
        var data = AquaProtobufSerializer.Serialize(graph);
        var copy = AquaProtobufSerializer.Deserialize<T>(data);

        return copy;
    }

    private static readonly Dictionary<Type, Func<object, object>> _transformers = new()
    {
        [typeof(DateTime)] = v => ((DateTime)v).ToMicrosecondPrecision(),
        [typeof(DateTimeOffset)] = v => ((DateTimeOffset)v).ToMicrosecondPrecision(),
        [typeof(TimeSpan)] = v => ((TimeSpan)v).ToMicrosecondPrecision(),
    };

    /// <summary>
    /// <para>
    ///   protobuf encoder transforms dates/times to UTC and uses unix style microseconds.
    /// </para>
    /// <para>
    ///   lossy encoding:
    ///   <list type="bullet">
    ///     <item>getting rid of local date kind</item>
    ///     <item>change precision from 100ns to 1µs</item>
    ///   </list>
    /// </para>
    /// <para>
    ///   therefore, we adjust test data to prevent test failing for lost precision due to our protobuf codec.
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
