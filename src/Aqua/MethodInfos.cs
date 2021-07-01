// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Linq.Expressions;
    using BindingFlags = System.Reflection.BindingFlags;
    using MethodInfo = System.Reflection.MethodInfo;

    internal static class MethodInfos
    {
        internal static class Enumerable
        {
            /// <summary>
            /// Type definition used in generic type filters.
            /// </summary>
            [SuppressMessage("Major Bug", "S3453:Classes should not have only \"private\" constructors", Justification = "For reflection only")]
            private sealed class TSource
            {
                private TSource()
                {
                }
            }

            /// <summary>
            /// Type definition used in generic type filters.
            /// </summary>
            [SuppressMessage("Major Bug", "S3453:Classes should not have only \"private\" constructors", Justification = "For reflection only")]
            private sealed class TResult
            {
                private TResult()
                {
                }
            }

            internal static readonly MethodInfo Cast = GetStaticMethod(
                typeof(System.Linq.Enumerable),
                nameof(System.Linq.Enumerable.Cast),
                new[] { typeof(TResult) },
                typeof(IEnumerable));

            internal static readonly MethodInfo OfType = GetStaticMethod(
                typeof(System.Linq.Enumerable),
                nameof(System.Linq.Enumerable.OfType),
                new[] { typeof(TResult) },
                typeof(IEnumerable));

            internal static readonly MethodInfo ToArray = GetStaticMethod(
                typeof(System.Linq.Enumerable),
                nameof(System.Linq.Enumerable.ToArray),
                new[] { typeof(TSource) },
                typeof(IEnumerable<TSource>));

            internal static readonly MethodInfo ToList = GetStaticMethod(
                typeof(System.Linq.Enumerable),
                nameof(System.Linq.Enumerable.ToList),
                new[] { typeof(TSource) },
                typeof(IEnumerable<TSource>));

            internal static readonly MethodInfo Contains = GetStaticMethod(
                typeof(System.Linq.Enumerable),
                nameof(System.Linq.Enumerable.Contains),
                new[] { typeof(TSource) },
                typeof(IEnumerable<TSource>),
                typeof(TSource));

            internal static readonly MethodInfo Single = GetStaticMethod(
                typeof(System.Linq.Enumerable),
                nameof(System.Linq.Enumerable.Single),
                new[] { typeof(TSource) },
                typeof(IEnumerable<TSource>));

            internal static readonly MethodInfo SingleOrDefault = GetStaticMethod(
                typeof(System.Linq.Enumerable),
                nameof(System.Linq.Enumerable.SingleOrDefault),
                new[] { typeof(TSource) },
                typeof(IEnumerable<TSource>));
        }

        internal static class Expression
        {
            /// <summary>
            /// Type definition used in generic type filters.
            /// </summary>
            [SuppressMessage("Major Bug", "S3453:Classes should not have only \"private\" constructors", Justification = "For reflection only")]
            private sealed class TDelegate
            {
                private TDelegate()
                {
                }
            }

            internal static readonly MethodInfo Lambda = GetStaticMethod(
                typeof(System.Linq.Expressions.Expression),
                nameof(System.Linq.Expressions.Expression.Lambda),
                new[] { typeof(TDelegate) },
                typeof(Expression),
                typeof(ParameterExpression[]));
        }

        internal static class Queryable
        {
            /// <summary>
            /// Type definition used in generic type filters.
            /// </summary>
            [SuppressMessage("Major Bug", "S3453:Classes should not have only \"private\" constructors", Justification = "For reflection only")]
            private sealed class TElement
            {
                private TElement()
                {
                }
            }

            /// <summary>
            /// Type definition used in generic type filters.
            /// </summary>
            [SuppressMessage("Major Bug", "S3453:Classes should not have only \"private\" constructors", Justification = "For reflection only")]
            private sealed class TSource
            {
                private TSource()
                {
                }
            }

            /// <summary>
            /// Type definition used in generic type filters.
            /// </summary>
            [SuppressMessage("Major Bug", "S3453:Classes should not have only \"private\" constructors", Justification = "For reflection only")]
            private sealed class TKey
            {
                private TKey()
                {
                }
            }

            /// <summary>
            /// Type definition used in generic type filters.
            /// </summary>
            [SuppressMessage("Major Bug", "S3453:Classes should not have only \"private\" constructors", Justification = "For reflection only")]
            private sealed class TResult
            {
                private TResult()
                {
                }
            }

            internal static readonly MethodInfo AsQueryable = GetStaticMethod(
                typeof(System.Linq.Queryable),
                nameof(System.Linq.Queryable.AsQueryable),
                new[] { typeof(TElement) },
                typeof(IEnumerable<TElement>));

            internal static readonly MethodInfo OrderBy = GetStaticMethod(
                typeof(System.Linq.Queryable),
                nameof(System.Linq.Queryable.OrderBy),
                new[] { typeof(TSource), typeof(TKey) },
                typeof(IQueryable<TSource>),
                typeof(Expression<Func<TSource, TKey>>));

            internal static readonly MethodInfo OrderByDescending = GetStaticMethod(
                typeof(System.Linq.Queryable),
                nameof(System.Linq.Queryable.OrderByDescending),
                new[] { typeof(TSource), typeof(TKey) },
                typeof(IQueryable<TSource>),
                typeof(Expression<Func<TSource, TKey>>));

            internal static readonly MethodInfo ThenBy = GetStaticMethod(
                typeof(System.Linq.Queryable),
                nameof(System.Linq.Queryable.ThenBy),
                new[] { typeof(TSource), typeof(TKey) },
                typeof(IOrderedQueryable<TSource>),
                typeof(Expression<Func<TSource, TKey>>));

            internal static readonly MethodInfo ThenByDescending = GetStaticMethod(
                typeof(System.Linq.Queryable),
                nameof(System.Linq.Queryable.ThenByDescending),
                new[] { typeof(TSource), typeof(TKey) },
                typeof(IOrderedQueryable<TSource>),
                typeof(Expression<Func<TSource, TKey>>));

            internal static readonly MethodInfo Select = GetStaticMethod(
                typeof(System.Linq.Queryable),
                nameof(System.Linq.Queryable.Select),
                new[] { typeof(TSource), typeof(TResult) },
                typeof(IOrderedQueryable<TSource>),
                typeof(Expression<Func<TSource, TResult>>));
        }

        internal static class String
        {
            internal static readonly MethodInfo StartsWith = GetMethod(
                typeof(string),
                nameof(string.StartsWith),
                typeof(string));

            internal static readonly MethodInfo EndsWith = GetMethod(
                typeof(string),
                nameof(string.EndsWith),
                typeof(string));

            internal static readonly MethodInfo Contains = GetMethod(
                typeof(string),
                nameof(string.Contains),
                typeof(string));
        }

        private const BindingFlags PublicStatic = BindingFlags.Public | BindingFlags.Static;
        private const BindingFlags PublicInstance = BindingFlags.Public | BindingFlags.Instance;

        /// <summary>
        /// Get <see cref="MethodInfo"/> using reflection.
        /// </summary>
        internal static MethodInfo GetMethod(Type declaringType, string name, params Type[] parameters)
            => GetMethodCore(declaringType, name, x => ParametersMatch(x, Array.Empty<Type>(), parameters), PublicInstance);

        /// <summary>
        /// Get <see cref="MethodInfo"/> using reflection.
        /// </summary>
        internal static MethodInfo GetMethod(Type declaringType, string name, Type[] genericArguments, params Type[] parameters)
            => GetMethodCore(declaringType, name, x => ParametersMatch(x, genericArguments, parameters), PublicInstance);

        /// <summary>
        /// Get <see cref="MethodInfo"/> using reflection.
        /// </summary>
        internal static MethodInfo GetStaticMethod(Type declaringType, string name, params Type[] parameters)
            => GetMethodCore(declaringType, name, x => ParametersMatch(x, Array.Empty<Type>(), parameters), PublicStatic);

        /// <summary>
        /// Get <see cref="MethodInfo"/> using reflection.
        /// </summary>
        internal static MethodInfo GetStaticMethod(Type declaringType, string name, Type[] genericArguments, params Type[] parameters)
            => GetMethodCore(declaringType, name, x => ParametersMatch(x, genericArguments, parameters), PublicStatic);

        private static MethodInfo GetMethodCore(Type declaringType, string name, Func<MethodInfo, bool> filter, BindingFlags bindingFlags)
        {
            try
            {
                return GetMethodsCore(declaringType, name, filter, bindingFlags).Single();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to get MethodInfo '{bindingFlags} {declaringType}.{name}()'", ex);
            }
        }

        private static IEnumerable<MethodInfo> GetMethodsCore(Type declaringType, string name, Func<MethodInfo, bool> filter, BindingFlags bindingFlags)
            => declaringType
            .GetMethods(bindingFlags)
            .Where(x => string.Equals(x.Name, name, StringComparison.Ordinal) && filter(x));

        private static bool ParametersMatch(MethodInfo method, Type[] genericArgumentTypes, Type[] parameterTypes)
        {
            method.AssertNotNull(nameof(method));
            genericArgumentTypes ??= Array.Empty<Type>();
            parameterTypes ??= Array.Empty<Type>();

            genericArgumentTypes.AssertItemsNotNull(nameof(genericArgumentTypes));
            parameterTypes.AssertItemsNotNull(nameof(parameterTypes));

            if (method.IsGenericMethod)
            {
                if (method.GetGenericArguments().Length != genericArgumentTypes.Length)
                {
                    return false;
                }

                method = method.MakeGenericMethod(genericArgumentTypes);
            }
            else if (genericArgumentTypes.Length > 0)
            {
                return false;
            }

            var parameters = method.GetParameters();
            if (parameters?.Length != parameterTypes.Length)
            {
                return false;
            }

            for (int i = 0; i < parameters.Length; i++)
            {
                var parameterType = parameters[i].ParameterType;
                var expectedType = parameterTypes[i];
                if (parameterType != expectedType)
                {
                    return false;
                }
            }

            return true;
        }
    }
}