// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.TypeSystem;

using System.Diagnostics.CodeAnalysis;

public interface ITypeResolver
{
    [return: NotNullIfNotNull(nameof(type))]
    Type? ResolveType(TypeInfo? type);
}