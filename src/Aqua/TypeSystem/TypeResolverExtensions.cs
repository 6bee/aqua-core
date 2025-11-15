// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.TypeSystem;

using System.Diagnostics.CodeAnalysis;

public static class TypeResolverExtensions
{
    public static bool TryResolveType(this ITypeResolver typeResolver, TypeInfo type, [NotNullWhen(true)] out Type? resolvedType)
    {
        typeResolver.AssertNotNull();

        try
        {
            resolvedType = typeResolver.ResolveType(type);
            return true;
        }
        catch (TypeResolverException)
        {
            resolvedType = null;
            return false;
        }
    }
}