// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.TypeSystem;

using System.Diagnostics.CodeAnalysis;

public interface ITypeInfoProvider
{
    [return: NotNullIfNotNull(nameof(type))]
    TypeInfo? GetTypeInfo(Type? type, bool? includePropertyInfos = null, bool? setMemberDeclaringTypes = null);
}