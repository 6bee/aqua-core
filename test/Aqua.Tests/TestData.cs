// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Tests
{
    using Aqua.Dynamic;
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Numerics;
    using System.Reflection;

    public static class TestData
    {
        public enum TestEnum
        {
            Foo,
            Bar,
        }

        public static IEnumerable<object[]> Types => new Type[]
            {
                typeof(byte),
                typeof(int),
                typeof(ulong),
                typeof(string),
                typeof(DateTime),
                typeof(TestEnum),
                new { Text = string.Empty, Timestamp = default(DateTime?) }.GetType(),
            }
            .SelectMany(x => new[]
            {
                x,
                x.IsClass ? x : typeof(Nullable<>).MakeGenericType(x),
            })
            .SelectMany(x => new[]
            {
                x,
                typeof(List<>).MakeGenericType(x),
                x.MakeArrayType(),
            })
            .Distinct()
            .Select(x => new[] { x });

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
                default(Guid),
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
            }
            .SelectMany(x => new (Type Type, object Value)[]
            {
                (x.GetType(), x),
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

        public static IEnumerable<object[]> NativeValues
            => GenerateTestValueSet()
            .Select(x => new object[] { x.Type, x.Value, x.Culture });

        public static IEnumerable<object[]> NativeValueArrays
            => GenerateTestValueSet()
            .Select(x => new[]
            {
                x.Type.MakeArrayType(),
                CreateArray(x.Type, x.Value),
                x.Culture,
            });

        // NOTE: NativeValueLists don't work with json.net since list element types don't get corrected by NativeValueInspector
        public static IEnumerable<object[]> NativeValueLists
            => GenerateTestValueSet()
            .Select(x => new[]
            {
                typeof(List<>).MakeGenericType(x.Type),
                CreateList(x.Type, x.Value),
                x.Culture,
            });

        private static object CreateArray(Type type, object item)
        {
            var toArrayMethod = typeof(Enumerable).GetMethod(nameof(Enumerable.ToArray), BindingFlags.Static | BindingFlags.Public).MakeGenericMethod(type);
            return toArrayMethod.Invoke(null, new[] { CreateEnumerable(type, item) });
        }

        private static object CreateList(Type type, object item)
        {
            var toListMethod = typeof(Enumerable).GetMethod(nameof(Enumerable.ToList), BindingFlags.Static | BindingFlags.Public).MakeGenericMethod(type);
            return toListMethod.Invoke(null, new[] { CreateEnumerable(type, item) });
        }

        private static object CreateEnumerable(Type type, object item)
        {
            var array = new[] { item, item }.AsEnumerable();
            var castMethod = typeof(Enumerable).GetMethod(nameof(Enumerable.Cast), BindingFlags.Static | BindingFlags.Public).MakeGenericMethod(type);
            return castMethod.Invoke(null, new object[] { array });
        }
    }
}
