// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Dynamic;

using Aqua.TypeExtensions;
using System.Collections.Generic;

/// <summary>
/// Provides ready-to-use <see cref="ITypeSafetyChecker"/> implementations in reference to
/// <see href="https://owasp.org/www-project-top-ten/2017/A8_2017-Insecure_Deserialization">OWASP A8:2017-Insecure Deserialization</see>,
/// to opt-in to type validation for instance creation
/// when mapping from <see cref="DynamicObject"/>.
/// </summary>
/// <remarks>
/// Type safety checking is opt-in: a <see cref="DynamicObjectMapper"/> created without an
/// <see cref="ITypeSafetyChecker"/> does not restrict types. Supply <see cref="AllowList(Type[])"/>
/// to restrict instance creation to an explicit set of types, or <see cref="AllowAny"/> as an
/// explicit no-op.
/// </remarks>
public static class TypeSafety
{
    /// <summary>
    /// Gets an <see cref="ITypeSafetyChecker"/> that permits instantiation of any type, i.e. performs no checking.
    /// </summary>
    public static ITypeSafetyChecker AllowAny { get; } = new AllowAnyTypeSafetyChecker();

    /// <summary>
    /// Creates an <see cref="ITypeSafetyChecker"/> that permits instantiation of the specified types only,
    /// rejecting any other type with a <see cref="TypeSafetyException"/>.
    /// </summary>
    /// <param name="allowedTypes">The set of types allowed for instance creation. May be empty to reject all types.</param>
    /// <returns>An <see cref="ITypeSafetyChecker"/> restricting instance creation to <paramref name="allowedTypes"/>.</returns>
    public static ITypeSafetyChecker AllowList(params Type[] allowedTypes)
        => AllowList(allowedTypes as IEnumerable<Type>);

    /// <summary>
    /// Creates an <see cref="ITypeSafetyChecker"/> that permits instantiation of the specified types only,
    /// rejecting any other type with a <see cref="TypeSafetyException"/>.
    /// </summary>
    /// <param name="allowedTypes">The set of types allowed for instance creation. May be empty to reject all types.</param>
    /// <returns>An <see cref="ITypeSafetyChecker"/> restricting instance creation to <paramref name="allowedTypes"/>.</returns>
    public static ITypeSafetyChecker AllowList(IEnumerable<Type> allowedTypes)
        => new AllowListTypeSafetyChecker(allowedTypes);

    private sealed class AllowAnyTypeSafetyChecker : ITypeSafetyChecker
    {
        public void AssertTypeSafety(Type type) => type.AssertNotNull();
    }

    private sealed class AllowListTypeSafetyChecker(IEnumerable<Type> allowedTypes) : ITypeSafetyChecker
    {
        private readonly Func<Type, bool> _isAllowedTyped = new HashSet<Type>(allowedTypes.CheckNotNull()).Contains;

        public void AssertTypeSafety(Type type)
        {
            type.AssertNotNull();
            if (!IsAllowed(type))
            {
                throw new TypeSafetyException($"Type '{type.GetFriendlyName()}' is not allowed for instance creation when mapping from {nameof(DynamicObject)}. Add the type to the allow-list or supply {nameof(TypeSafety)}.{nameof(AllowAny)} to permit any type.");
            }
        }

        private bool IsAllowed(Type type)
        {
            if (_isAllowedTyped(type))
            {
                return true;
            }

            if (Nullable.GetUnderlyingType(type) is { } underlyingType)
            {
                return IsAllowed(underlyingType);
            }

            if (type.IsArray && type.GetElementType() is { } elementType)
            {
                return IsAllowed(elementType);
            }

            return false;
        }
    }
}
