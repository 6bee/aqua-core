// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.TypeSystem;

using Aqua.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

public class TypeInfoProvider : ITypeInfoProvider
{
    private readonly Dictionary<Type, TypeInfo> _referenceTracker;
    private readonly Dictionary<TypeInfo, TypeInfo> _typeInfoReferenceTracker;
    private readonly ITypeInfoProvider? _parent;

    public object SyncRoot => this;

    public TypeInfoProvider(bool includePropertyInfos = false, bool setMemberDeclaringTypes = false)
        : this(includePropertyInfos, setMemberDeclaringTypes, null, null)
    {
    }

    internal TypeInfoProvider(bool includePropertyInfos, bool setMemberDeclaringTypes, ITypeInfoProvider parent)
        : this(includePropertyInfos, setMemberDeclaringTypes, (parent as TypeInfoProvider)?._referenceTracker, (parent as TypeInfoProvider)?._typeInfoReferenceTracker)
    {
        _parent = parent;
    }

    private TypeInfoProvider(bool includePropertyInfos, bool setMemberDeclaringTypes, Dictionary<Type, TypeInfo>? referenceTracker, Dictionary<TypeInfo, TypeInfo>? typeInfoReferenceTracker)
    {
        IncludePropertyInfos = includePropertyInfos;
        SetMemberDeclaringTypes = setMemberDeclaringTypes;
        _referenceTracker = referenceTracker ?? CreateReferenceTracker<Type>();
        _typeInfoReferenceTracker = typeInfoReferenceTracker ?? CreateReferenceTracker<TypeInfo>();
    }

    public bool IncludePropertyInfos { get; }

    public bool SetMemberDeclaringTypes { get; }

    internal void RegisterReference(Type type, TypeInfo typeInfo) => _referenceTracker.Add(type, typeInfo);

    internal void RegisterReference(TypeInfo type, TypeInfo typeInfo) => _typeInfoReferenceTracker.Add(type, typeInfo);

    /// <summary>
    /// Returns a <see cref="TypeInfo"/> representing the specified <see cref="Type"/>.
    /// </summary>
    /// <param name="type">The <see cref="Type"/> to be represented.</param>
    /// <param name="includePropertyInfos">If provided overrules <seealso cref="IncludePropertyInfos"/> property set on class level.</param>
    /// <param name="setMemberDeclaringTypes">If provided overrules <seealso cref="SetMemberDeclaringTypes"/> property set on class level.</param>
    /// <returns>Returns a <see cref="TypeInfo"/> representing the specified <see cref="Type"/> or null if the type parameter is null.</returns>
    [return: NotNullIfNotNull(nameof(type))]
    public virtual TypeInfo? GetTypeInfo(Type? type, bool? includePropertyInfos = null, bool? setMemberDeclaringTypes = null)
    {
        if (type is null)
        {
            return null;
        }

        lock (SyncRoot)
        {
            if (_referenceTracker.TryGetValue(type, out var typeInfo))
            {
                return typeInfo;
            }

            if (_parent is not null)
            {
                return _parent.GetTypeInfo(type, includePropertyInfos, setMemberDeclaringTypes);
            }

            var context = this;
            if ((includePropertyInfos.HasValue && includePropertyInfos != IncludePropertyInfos) ||
                (setMemberDeclaringTypes.HasValue && setMemberDeclaringTypes != SetMemberDeclaringTypes))
            {
                context = new TypeInfoProvider(
                    includePropertyInfos ?? IncludePropertyInfos,
                    setMemberDeclaringTypes ?? SetMemberDeclaringTypes,
                    this);
            }

            return new TypeInfo(type, context);
        }
    }

    [return: NotNullIfNotNull(nameof(type))]
    internal TypeInfo? Get(TypeInfo? type)
    {
        lock (SyncRoot)
        {
            return type is null
                ? null
                : _typeInfoReferenceTracker.TryGetValue(type, out var typeInfo)
                ? typeInfo
                : new TypeInfo(type, this);
        }
    }

    private static Dictionary<T, TypeInfo> CreateReferenceTracker<T>()
        where T : notnull
        => new Dictionary<T, TypeInfo>(ReferenceEqualityComparer<T>.Default);
}
