// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Tests;

using System.Globalization;
using System.Numerics;
using System.Reflection;

public static class TestData
{
    // TODO: Extend test data set with custom types
    public class GenericReferenceType<T>
    {
        public T Value { get; set; }
    }

    public class ReferenceType
    {
        public string Value { get; set; }
    }

    public sealed class ImmutableReferenceType
    {
        public string Value { get; init; }
    }

    public struct ValueType
    {
        public string Value { get; set; }
    }

    public readonly struct ImmutableValueType
    {
        public string Value { get; init; }
    }

    public record RecordType
    {
        public string Value { get; set; }
    }

    public sealed record ImmutableRecordType
    {
        public string Value { get; init; }
    }

    public class EmptyType
    {
        public override int GetHashCode() => 0;

        public override bool Equals(object obj) => obj is EmptyType;
    }

    public enum TestEnum
    {
        Foo,
        Bar,
    }

    private const BindingFlags PublicStatic = BindingFlags.Static | BindingFlags.Public;
    private const BindingFlags PrivateStatic = BindingFlags.NonPublic | BindingFlags.Static;

    private static IEnumerable<(Type Type, object Value, CultureInfo Culture)> GenerateTestValueSet()
        => new object[]
        {
            "literal string",
            byte.MinValue,
            byte.MaxValue,
            sbyte.MinValue,
            sbyte.MaxValue,
            (sbyte)0,
            short.MinValue,
            short.MaxValue,
            (short)0,
            ushort.MinValue,
            ushort.MaxValue,
            (ushort)0,
            int.MinValue,
            int.MaxValue,
            0,
            uint.MinValue,
            uint.MaxValue,
            0u,
            long.MinValue,
            long.MaxValue,
            0L,
            ulong.MinValue,
            ulong.MaxValue,
            0ul,
            float.MinValue,
            float.MaxValue,
            0f,
            .1f,
            1f / 3f,
            double.MinValue,
            double.MaxValue,
            0d,
            .1d,
            1d / 3d,
            decimal.MinValue,
            decimal.MaxValue,
            new decimal(Math.E),
            new decimal(Math.PI),
            0m,
            .1m,
            1m / 3m,
            char.MinValue,
            char.MaxValue,
            'à',
            true,
            false,
            Guid.Empty,
            Guid.Parse("0c67c9c9-245d-4d5b-ac7e-0803ca118f4c"),
            default(DateTime),
            DateTime.MinValue.ToUniversalTime(),
            DateTime.MaxValue.ToUniversalTime(),
            new DateTime(1009, 9, 7, 6, 5, 4, 321, DateTimeKind.Utc),
            new DateTime(1, DateTimeKind.Utc),
            default(TimeSpan),
            TimeSpan.MinValue,
            TimeSpan.MaxValue,
            default(DateTimeOffset),
            DateTimeOffset.MinValue,
            DateTimeOffset.MaxValue,
            new DateTimeOffset(new DateTime(1009, 9, 7, 6, 5, 4, 321), new TimeSpan(1, 15, 0)),
            default(BigInteger),
            new BigInteger(ulong.MinValue) - 1,
            new BigInteger(ulong.MaxValue) + 1,
            default(Complex),
            new Complex(32, -87654),
            new Complex(-87654, 234),
            new Complex(double.MinValue, double.MinValue),
            new Complex(double.MaxValue, double.MaxValue),
            (TestEnum)(-1),
            TestEnum.Foo,
            TestEnum.Bar,
            new { Text = string.Empty, Timestamp = default(DateTime?) },
            new EmptyType(),
#if NET5_0_OR_GREATER
            Half.MaxValue,
            Half.Epsilon,
            (Half)0f,
            (Half).1f,
            (Half)(1f / 3f),
#endif // NET5_0_OR_GREATER
#if NET6_0_OR_GREATER
            DateOnly.MinValue,
            DateOnly.MaxValue,
            DateOnly.FromDateTime(new DateTime(1009, 9, 7)),
            TimeOnly.MinValue,
            TimeOnly.MaxValue,
            TimeOnly.FromDateTime(new DateTime(1009, 9, 7, 6, 5, 4, 321)),
#endif // NET6_0_OR_GREATER
#if NET7_0_OR_GREATER
            Int128.MinValue,
            Int128.MaxValue,
            UInt128.MinValue,
            UInt128.MaxValue,
#endif // NET7_0_OR_GREATER

            // TODO: consider support for custom tuples
            // (Name: "NegativePi", Value: -Math.PI),
        }
        .SelectMany(
            static x => new (Type Type, object Value)[]
            {
                (x.GetType(), x),
                (x.GetType(), CreateDefault(x.GetType())),
                (x.GetType().IsClass ? x.GetType() : typeof(Nullable<>).MakeGenericType(x.GetType()), x),
                (x.GetType().IsClass ? x.GetType() : typeof(Nullable<>).MakeGenericType(x.GetType()), null),
            })
        .Distinct()
        .SelectMany(
            static _ => new[]
            {
                CultureInfo.InvariantCulture,
                CultureInfo.GetCultureInfo("de"),
            },
            static (x, c) => (x.Type, x.Value, c));

    private static object CreateDefault(Type t)
        => typeof(TestData).GetMethods(PrivateStatic)
        .Single(x => string.Equals(x.Name, nameof(CreateDefault), StringComparison.Ordinal) && x.IsGenericMethodDefinition)
        .MakeGenericMethod(t)
        .Invoke(null, null);

    private static object CreateDefault<T>()
        => default(T);

    public static IEnumerable<object[]> TestTypes
        => GenerateTestValueSet()
        .Select(x => x.Type)
        .Distinct()
        .Select(x => new[] { x });

    public static IEnumerable<object[]> TestValues
        => GenerateTestValueSet()
        .Select(x => new object[] { x.Type, x.Value, x.Culture });

    public static IEnumerable<object[]> TestValueArrays
        => GenerateTestValueSet()
        .Select(x => new[]
        {
            x.Type.MakeArrayType(),
            CreateArray(x.Type, x.Value),
            x.Culture,
        });

    public static IEnumerable<object[]> TestValueLists
        => GenerateTestValueSet()
        .Select(x => new[]
        {
            typeof(List<>).MakeGenericType(x.Type),
            CreateList(x.Type, x.Value),
            x.Culture,
        });

    private static object CreateArray(Type type, object item)
    {
        var toArrayMethod = typeof(Enumerable).GetMethod(nameof(Enumerable.ToArray), PublicStatic).MakeGenericMethod(type);
        return toArrayMethod.Invoke(null, [CreateEnumerable(type, item)]);
    }

    private static object CreateList(Type type, object item)
    {
        var toListMethod = typeof(Enumerable).GetMethod(nameof(Enumerable.ToList), PublicStatic).MakeGenericMethod(type);
        return toListMethod.Invoke(null, [CreateEnumerable(type, item)]);
    }

    private static object CreateEnumerable(Type type, object item)
    {
        var array = new[] { item, item }.AsEnumerable();
        var castMethod = typeof(Enumerable).GetMethod(nameof(Enumerable.Cast), PublicStatic).MakeGenericMethod(type);
        return castMethod.Invoke(null, [array]);
    }
}
