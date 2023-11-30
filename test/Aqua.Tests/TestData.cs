// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Tests;

using Aqua.Dynamic;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
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

    private static IEnumerable<(Type Type, object Value, CultureInfo Culture)> GenerateTestValueSet() => new object[]
        {
            $"Test values treated as native types in {nameof(DynamicObjectMapper)}",
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
            double.MinValue,
            double.MaxValue,
            0d,
            .1d,
            decimal.MinValue,
            decimal.MaxValue,
            new decimal(Math.E),
            new decimal(Math.PI),
            0m,
            .1m,
            char.MinValue,
            char.MaxValue,
            'à',
            true,
            false,
            Guid.Empty,
            Guid.NewGuid(),
            DateTime.Now,
            default(DateTime),
            default(TimeSpan),
            new TimeSpan(long.MaxValue),
            default(DateTimeOffset),
            DateTimeOffset.MinValue,
            DateTimeOffset.MaxValue,
            new DateTimeOffset(new DateTime(2012, 12, 12), new TimeSpan(12, 12, 0)),
            default(BigInteger),
            new BigInteger(ulong.MinValue) - 1,
            new BigInteger(ulong.MaxValue) + 1,
            default(Complex),
            new Complex(32, -87654),
            new Complex(-87654, 234),
            new Complex(double.MinValue, double.MinValue),
            new Complex(double.MaxValue, double.MaxValue),
#if NET5_0_OR_GREATER
            Half.MaxValue,
            Half.Epsilon,
            (Half)0f,
            (Half).1f,
#endif // NET5_0_OR_GREATER
            (TestEnum)(-1),
            TestEnum.Foo,
            TestEnum.Bar,
            new { Text = string.Empty, Timestamp = default(DateTime?) },
            new EmptyType(),

            // TODO: consider support for custom tuples
            // (Name: "NegativePi", Value: -Math.PI),
        }
        .SelectMany(x => new (Type Type, object Value)[]
        {
            (x.GetType(), x),
            (x.GetType(), CreateDefault(x.GetType())),
            (x.GetType().IsClass ? x.GetType() : typeof(Nullable<>).MakeGenericType(x.GetType()), x),
            (x.GetType().IsClass ? x.GetType() : typeof(Nullable<>).MakeGenericType(x.GetType()), null),
        })
        .Distinct()
        .SelectMany(
            _ => new[]
            {
                CultureInfo.InvariantCulture,
                CultureInfo.GetCultureInfo("de"),
            },
            (x, c) => (x.Type, x.Value, c));

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