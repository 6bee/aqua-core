// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua
{
    using Aqua.EnumerableExtensions;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using BindingFlags = System.Reflection.BindingFlags;
    using MethodInfo = System.Reflection.MethodInfo;

    internal static class MethodInfos
    {
        internal static class Enumerable
        {
            internal static readonly MethodInfo Cast = GetPublicStaticMethod(typeof(System.Linq.Enumerable), nameof(System.Linq.Enumerable.Cast));

            internal static readonly MethodInfo OfType = GetPublicStaticMethod(typeof(System.Linq.Enumerable), nameof(System.Linq.Enumerable.OfType));

            internal static readonly MethodInfo ToArray = GetPublicStaticMethod(typeof(System.Linq.Enumerable), nameof(System.Linq.Enumerable.ToArray));

            internal static readonly MethodInfo ToList = GetPublicStaticMethod(typeof(System.Linq.Enumerable), nameof(System.Linq.Enumerable.ToList));

            internal static readonly MethodInfo Contains = typeof(System.Linq.Enumerable)
                .GetMethods(PublicStatic)
                .Single(m => m.Name == nameof(System.Linq.Enumerable.Contains) && m.GetParameters().Length == 2);

            internal static readonly MethodInfo Single = typeof(System.Linq.Enumerable)
                .GetMethods(PublicStatic)
                .Where(x => x.Name == nameof(System.Linq.Enumerable.Single))
                .Where(x =>
                {
                    var parameters = x.GetParameters();
                    return parameters.Length == 1
                        && parameters[0].ParameterType.IsGenericType
                        && parameters[0].ParameterType.GetGenericTypeDefinition() == typeof(IEnumerable<>);
                })
                .Single();

            internal static readonly MethodInfo SingleOrDefault = typeof(System.Linq.Enumerable)
                .GetMethods(PublicStatic)
                .Where(x => x.Name == nameof(System.Linq.Enumerable.SingleOrDefault))
                .Where(x =>
                {
                    var parameters = x.GetParameters();
                    return parameters.Length == 1
                        && parameters[0].ParameterType.IsGenericType
                        && parameters[0].ParameterType.GetGenericTypeDefinition() == typeof(IEnumerable<>);
                })
                .Single();
        }

        internal static class Expression
        {
            internal static readonly MethodInfo Lambda = typeof(System.Linq.Expressions.Expression)
                .GetMethods(PublicStatic)
                .Single(m =>
                    m.Name == nameof(System.Linq.Expressions.Expression.Lambda) &&
                    m.IsGenericMethod &&
                    m.GetParameters().Length == 2 &&
                    m.GetParameters()[1].ParameterType == typeof(System.Linq.Expressions.ParameterExpression[]));
        }

        internal static class Queryable
        {
            internal static readonly MethodInfo AsQueryable = typeof(System.Linq.Queryable)
                .GetMethods(PublicStatic)
                .Single(m =>
                    m.Name == nameof(System.Linq.Queryable.AsQueryable) &&
                    m.IsGenericMethod &&
                    m.GetParameters().Length == 1);

            internal static readonly MethodInfo OrderBy = typeof(System.Linq.Queryable)
                .GetMethods(PublicStatic)
                .Single(m =>
                    m.Name == nameof(System.Linq.Queryable.OrderBy) &&
                    m.IsGenericMethod &&
                    m.GetParameters().Length == 2);

            internal static readonly MethodInfo OrderByDescending = typeof(System.Linq.Queryable)
                .GetMethods(PublicStatic)
                .Single(m =>
                    m.Name == nameof(System.Linq.Queryable.OrderByDescending) &&
                    m.IsGenericMethod &&
                    m.GetParameters().Length == 2);

            internal static readonly MethodInfo ThenBy = typeof(System.Linq.Queryable)
                .GetMethods(PublicStatic)
                .Single(m =>
                    m.Name == nameof(System.Linq.Queryable.ThenBy) &&
                    m.IsGenericMethod &&
                    m.GetParameters().Length == 2);

            internal static readonly MethodInfo ThenByDescending = typeof(System.Linq.Queryable)
                .GetMethods(PublicStatic)
                .Single(m =>
                    m.Name == nameof(System.Linq.Queryable.ThenByDescending) &&
                    m.IsGenericMethod &&
                    m.GetParameters().Length == 2);

            internal static readonly MethodInfo Select = typeof(System.Linq.Queryable)
                .GetMethods(PublicStatic)
                .Where(i => i.Name == nameof(System.Linq.Queryable.Select))
                .Where(i => i.IsGenericMethod)
                .Single(i =>
                {
                    var parameters = i.GetParameters();
                    if (parameters.Length != 2)
                    {
                        return false;
                    }

                    var expressionParamType = parameters[1].ParameterType;
                    if (!expressionParamType.IsGenericType)
                    {
                        return false;
                    }

                    var genericArguments = expressionParamType.GetGenericArguments().ToArray();
                    if (genericArguments.Length != 1)
                    {
                        return false;
                    }

                    if (!genericArguments.Single().IsGenericType)
                    {
                        return false;
                    }

                    if (genericArguments.Single().GetGenericArguments().Length != 2)
                    {
                        return false;
                    }

                    return true;
                });
        }

        internal static class String
        {
            internal static readonly MethodInfo StartsWith = GetMethod(typeof(string), nameof(string.StartsWith), new[] { typeof(string) });

            internal static readonly MethodInfo EndsWith = GetMethod(typeof(string), nameof(string.EndsWith), new[] { typeof(string) });

            internal static readonly MethodInfo Contains = GetMethod(typeof(string), nameof(string.Contains), new[] { typeof(string) });
        }

        private const BindingFlags PublicStatic = BindingFlags.Public | BindingFlags.Static;

        private static MethodInfo GetPublicStaticMethod(Type type, string name)
            => type.GetMethod(name, PublicStatic)
            ?? throw new InvalidOperationException($"Type {type} does not declare public static method '{name}'");

        private static MethodInfo GetMethod(Type type, string name, Type[] types)
            => type.GetMethod(name, types)
            ?? throw new InvalidOperationException($"Type {type} does not declare public instance method '{name}({types.StringJoin(", ")})'");
    }
}
