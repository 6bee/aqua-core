// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.TypeSystem
{
    using System;
    using System.Collections.Generic;

    public class TypeInfoProvider : ITypeInfoProvider
    {
        private readonly Dictionary<Type, TypeInfo> _referenceTracker;
        private readonly Dictionary<TypeInfo, TypeInfo> _typeInfoReferenceTracker;
        private readonly ITypeInfoProvider _parent;

        public TypeInfoProvider(bool includePropertyInfos = false, bool setMemberDeclaringTypes = false)
            : this(includePropertyInfos, setMemberDeclaringTypes, null, null)
        {
        }

        internal TypeInfoProvider(bool includePropertyInfos, bool setMemberDeclaringTypes, ITypeInfoProvider parent)
            : this(includePropertyInfos, setMemberDeclaringTypes, (parent as TypeInfoProvider)?._referenceTracker, (parent as TypeInfoProvider)?._typeInfoReferenceTracker)
        {
            _parent = parent;
        }

        private TypeInfoProvider(bool includePropertyInfos, bool setMemberDeclaringTypes, Dictionary<Type, TypeInfo> referenceTracker, Dictionary<TypeInfo, TypeInfo> typeInfoReferenceTracker)
        {
            IncludePropertyInfos = includePropertyInfos;
            SetMemberDeclaringTypes = setMemberDeclaringTypes;
            _referenceTracker = referenceTracker ?? CreateReferenceTracker<Type>();
            _typeInfoReferenceTracker = typeInfoReferenceTracker ?? CreateReferenceTracker<TypeInfo>();
        }

        public bool IncludePropertyInfos { get; }

        public bool SetMemberDeclaringTypes { get; }

        internal void RegisterReference(Type type, TypeInfo typeInfo)
            => _referenceTracker.Add(type, typeInfo);

        internal void RegisterReference(TypeInfo type, TypeInfo typeInfo)
            => _typeInfoReferenceTracker.Add(type, typeInfo);

        public virtual TypeInfo Get(Type type, bool? includePropertyInfosOverride = null, bool? setMemberDeclaringTypesOverride = null)
        {
            if (type is null)
            {
                return null;
            }

            if (_referenceTracker.TryGetValue(type, out TypeInfo typeInfo))
            {
                return typeInfo;
            }

            if (!(_parent is null))
            {
                return _parent.Get(type, includePropertyInfosOverride, setMemberDeclaringTypesOverride);
            }

            var context = this;
            if ((includePropertyInfosOverride.HasValue && includePropertyInfosOverride != IncludePropertyInfos) ||
                (setMemberDeclaringTypesOverride.HasValue && setMemberDeclaringTypesOverride != SetMemberDeclaringTypes))
            {
                context = new TypeInfoProvider(
                    includePropertyInfosOverride ?? IncludePropertyInfos,
                    setMemberDeclaringTypesOverride ?? SetMemberDeclaringTypes,
                    this);
            }

            return new TypeInfo(type, context);
        }

        internal TypeInfo Get(TypeInfo type)
        {
            if (type is null)
            {
                return null;
            }

            return _typeInfoReferenceTracker.TryGetValue(type, out TypeInfo typeInfo)
                ? typeInfo
                : new TypeInfo(type, this);
        }

        private static Dictionary<T, TypeInfo> CreateReferenceTracker<T>()
            => new Dictionary<T, TypeInfo>(ReferenceEqualityComparer<T>.Default);
    }
}
