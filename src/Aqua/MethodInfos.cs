// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua
{
    using Aqua.TypeSystem.Extensions;
    using System.Collections.Generic;
    using System.Linq;
    using BindingFlags = System.Reflection.BindingFlags;
    using MethodInfo = System.Reflection.MethodInfo;

    internal static class MethodInfos
    {
        internal static class Enumerable
        {
            internal static readonly MethodInfo Cast = typeof(System.Linq.Enumerable)
                .GetMethod(nameof(System.Linq.Enumerable.Cast), BindingFlags.Public | BindingFlags.Static);

            internal static readonly MethodInfo OfType = typeof(System.Linq.Enumerable)
                .GetMethod(nameof(System.Linq.Enumerable.OfType), BindingFlags.Public | BindingFlags.Static);

            internal static readonly MethodInfo ToArray = typeof(System.Linq.Enumerable)
                .GetMethod(nameof(System.Linq.Enumerable.ToArray), BindingFlags.Public | BindingFlags.Static);

            internal static readonly MethodInfo ToList = typeof(System.Linq.Enumerable)
                .GetMethod(nameof(System.Linq.Enumerable.ToList), BindingFlags.Public | BindingFlags.Static);

            internal static readonly MethodInfo Contains = typeof(System.Linq.Enumerable)
                .GetMethods(BindingFlags.Public | BindingFlags.Static)
                .Single(m => m.Name == nameof(System.Linq.Enumerable.Contains) && m.GetParameters().Length == 2);

            internal static readonly MethodInfo Single = typeof(System.Linq.Enumerable)
                .GetMethods(BindingFlags.Static | BindingFlags.Public)
                .Where(x => x.Name == nameof(System.Linq.Enumerable.Single))
                .Where(x =>
                {
                    var parameters = x.GetParameters();
                    return parameters.Length == 1
                        && parameters[0].ParameterType.IsGenericType()
                        && parameters[0].ParameterType.GetGenericTypeDefinition() == typeof(IEnumerable<>);
                })
                .Single();

            internal static readonly MethodInfo SingleOrDefault = typeof(System.Linq.Enumerable)
                .GetMethods(BindingFlags.Static | BindingFlags.Public)
                .Where(x => x.Name == nameof(System.Linq.Enumerable.SingleOrDefault))
                .Where(x =>
                {
                    var parameters = x.GetParameters();
                    return parameters.Length == 1
                        && parameters[0].ParameterType.IsGenericType()
                        && parameters[0].ParameterType.GetGenericTypeDefinition() == typeof(IEnumerable<>);
                })
                .Single();
        }

        internal static class Expression
        {
            internal static readonly MethodInfo Lambda = typeof(System.Linq.Expressions.Expression)
                .GetMethods(BindingFlags.Public | BindingFlags.Static)
                .Single(m =>
                    m.Name == nameof(System.Linq.Expressions.Expression.Lambda) &&
                    m.IsGenericMethod &&
                    m.GetParameters().Length == 2 &&
                    m.GetParameters()[1].ParameterType == typeof(System.Linq.Expressions.ParameterExpression[]));
        }

        internal static class Queryable
        {
            internal static readonly MethodInfo OrderBy = typeof(System.Linq.Queryable)
                .GetMethods(BindingFlags.Public | BindingFlags.Static)
                .Single(m =>
                    m.Name == nameof(System.Linq.Queryable.OrderBy) &&
                    m.IsGenericMethod &&
                    m.GetParameters().Length == 2);

            internal static readonly MethodInfo OrderByDescending = typeof(System.Linq.Queryable)
                .GetMethods(BindingFlags.Public | BindingFlags.Static)
                .Single(m =>
                    m.Name == nameof(System.Linq.Queryable.OrderByDescending) &&
                    m.IsGenericMethod &&
                    m.GetParameters().Length == 2);

            internal static readonly MethodInfo ThenBy = typeof(System.Linq.Queryable)
                .GetMethods(BindingFlags.Public | BindingFlags.Static)
                .Single(m =>
                    m.Name == nameof(System.Linq.Queryable.ThenBy) &&
                    m.IsGenericMethod &&
                    m.GetParameters().Length == 2);

            internal static readonly MethodInfo ThenByDescending = typeof(System.Linq.Queryable)
                .GetMethods(BindingFlags.Public | BindingFlags.Static)
                .Single(m =>
                    m.Name == nameof(System.Linq.Queryable.ThenByDescending) &&
                    m.IsGenericMethod &&
                    m.GetParameters().Length == 2);

            internal static readonly MethodInfo Select = typeof(System.Linq.Queryable)
                .GetMethods(BindingFlags.Public | BindingFlags.Static)
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
                    if (!expressionParamType.IsGenericType())
                    {
                        return false;
                    }

                    var genericArguments = expressionParamType.GetGenericArguments().ToArray();
                    if (genericArguments.Count() != 1)
                    {
                        return false;
                    }

                    if (!genericArguments.Single().IsGenericType())
                    {
                        return false;
                    }

                    if (genericArguments.Single().GetGenericArguments().Count() != 2)
                    {
                        return false;
                    }

                    return true;
                });
        }

        internal static class String
        {
            internal static readonly MethodInfo StartsWith = typeof(string)
                .GetMethod(nameof(string.StartsWith), new[] { typeof(string) });

            internal static readonly MethodInfo EndsWith = typeof(string)
                .GetMethod(nameof(string.EndsWith), new[] { typeof(string) });

            internal static readonly MethodInfo Contains = typeof(string)
                .GetMethod(nameof(string.Contains), new[] { typeof(string) });
        }
    }
}
