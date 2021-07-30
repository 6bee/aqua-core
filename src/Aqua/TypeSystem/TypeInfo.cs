// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.TypeSystem
{
    using Aqua.Dynamic;
    using Aqua.EnumerableExtensions;
    using Aqua.TypeExtensions;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.Serialization;
    using System.Text.RegularExpressions;
    using System.Xml.Serialization;

    [Serializable]
    [DataContract(Name = "Type", IsReference = true)]
    [DebuggerDisplay("Type: {FullName,nq}")]
    [KnownType(typeof(TypeInfo[])), XmlInclude(typeof(TypeInfo[]))]
    public class TypeInfo
    {
        private static readonly Regex _arrayNameRegex = new Regex(@"^.*\[,*\]$");

        [IgnoreDataMember]
        [Unmapped]
        [NonSerialized]
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private Type? _type;

        public TypeInfo()
        {
        }

        public TypeInfo(Type type, bool includePropertyInfos = true)
            : this(type, includePropertyInfos, true)
        {
        }

        public TypeInfo(Type type, bool includePropertyInfos, bool setMemberDeclaringTypes)
            : this(type.CheckNotNull(nameof(type)), new TypeInfoProvider(includePropertyInfos, setMemberDeclaringTypes))
        {
        }

        internal TypeInfo(Type type, TypeInfoProvider typeInfoProvider)
        {
            lock (typeInfoProvider.SyncRoot)
            {
                typeInfoProvider.RegisterReference(type, this);

                _type = type;

                Name = type.Name;

                Namespace = type.Namespace;

                if (type.IsArray)
                {
                    if (!IsArray)
                    {
                        throw new ArgumentException("Type name is not in expected format for array type");
                    }

                    type = type.GetElementType() !;
                }

                if (type.IsNested && !type.IsGenericParameter)
                {
                    DeclaringType = typeInfoProvider.GetTypeInfo(type.DeclaringType, false, false);
                }

                IsGenericType = type.IsGenericType;

                if (IsGenericType && !type.GetTypeInfo().IsGenericTypeDefinition)
                {
                    GenericArguments = type
                        .GetGenericArguments()
                        .Select(x => typeInfoProvider.GetTypeInfo(x))
                        .ToList();
                }

                IsAnonymousType = type.IsAnonymousType();

                if (IsAnonymousType || typeInfoProvider.IncludePropertyInfos)
                {
                    Properties = type
                        .GetProperties()
                        .OrderBy(x => x.MetadataToken)
                        .Select(x => new PropertyInfo(x.Name, typeInfoProvider.GetTypeInfo(x.PropertyType), typeInfoProvider.SetMemberDeclaringTypes ? this : null))
                        .ToList();
                }
            }
        }

        public TypeInfo(TypeInfo type)
            : this(type.CheckNotNull(nameof(type)), new TypeInfoProvider())
        {
        }

        internal TypeInfo(TypeInfo typeInfo, TypeInfoProvider typeInfoProvider)
        {
            lock (typeInfoProvider.SyncRoot)
            {
                typeInfoProvider.RegisterReference(typeInfo, this);

                Name = typeInfo.Name;
                Namespace = typeInfo.Namespace;
                DeclaringType = typeInfo.DeclaringType is null ? null : typeInfoProvider.Get(typeInfo.DeclaringType);
                GenericArguments = typeInfo.GenericArguments?.Select(x => typeInfoProvider.Get(x)).ToList();
                IsGenericType = typeInfo.IsGenericType;
                IsAnonymousType = typeInfo.IsAnonymousType;
                Properties = typeInfo.Properties?.Select(x => new PropertyInfo(x, typeInfoProvider)).ToList();
                _type = typeInfo._type;
            }
        }

        [DataMember(Order = 1, EmitDefaultValue = false)]
        public string? Name { get; set; }

        [DataMember(Order = 2, IsRequired = false, EmitDefaultValue = false)]
        public string? Namespace { get; set; }

        [DataMember(Order = 3, IsRequired = false, EmitDefaultValue = false)]
        public TypeInfo? DeclaringType { get; set; }

        [DataMember(Order = 4, IsRequired = false, EmitDefaultValue = false)]
        public List<TypeInfo>? GenericArguments { get; set; }

        [DataMember(Order = 5, IsRequired = false, EmitDefaultValue = false)]
        public bool IsAnonymousType { get; set; }

        [DataMember(Order = 6, IsRequired = false, EmitDefaultValue = false)]
        public bool IsGenericType { get; set; }

        [DataMember(Order = 7, IsRequired = false, EmitDefaultValue = false)]
        public List<PropertyInfo>? Properties { get; set; }

        [IgnoreDataMember]
        [Unmapped]
        public bool IsNested => DeclaringType is not null;

        [IgnoreDataMember]
        [Unmapped]
        public bool IsGenericTypeDefinition => IsGenericType && (!GenericArguments?.Any() ?? true);

        [IgnoreDataMember]
        [Unmapped]
        public bool IsArray
        {
            get
            {
                var name = Name;
                return name is not null && _arrayNameRegex.IsMatch(name);
            }
        }

        [IgnoreDataMember]
        [Unmapped]
        public string FullName
            => IsNested
                ? $"{DeclaringType!.FullName}+{Name}"
                : $"{Namespace}{(string.IsNullOrEmpty(Namespace) ? null : ".")}{Name}";

        [IgnoreDataMember]
        [Unmapped]
        internal string NameWithoutNameSpace
            => IsNested
                ? $"{DeclaringType!.NameWithoutNameSpace}+{Name}"
                : Name ?? string.Empty;

        /// <summary>
        /// Gets <see cref="Type"/> by resolving this <see cref="TypeInfo"/> instance using the default <see cref="TypeResolver"/>.
        /// </summary>
        [IgnoreDataMember]
        [Unmapped]
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Use method ToType() instead, this property is being removed in a future version.", false)]
        public Type Type => ToType();

        /// <summary>
        /// Returns the <see cref="Type"/> represented by this <see cref="TypeInfo"/> instance by resolving it using the default <see cref="TypeResolver"/>.
        /// </summary>
        public Type ToType()
            => _type ??= this.ResolveType(TypeResolver.Instance)
            ?? throw new TypeResolverException($"Failed to resolve type, consider using extension method to specify {nameof(ITypeResolver)}.");

        public static explicit operator Type?(TypeInfo? type)
            => type?.ToType();

        public override string ToString()
            => GetFriendlyName();

        /// <summary>
        /// Returns a formatted string for the given <see cref="TypeInfo"/>.
        /// </summary>
        /// <param name="includeNamespance"><see langword="true"/> is fullname should be included, <see langword="false"/> otherwise.</param>
        /// <param name="includeDeclaringType">Can be set <see langword="false"/> for nested types to supress name of declaring type.
        /// This has no effect for non-nested types or if <paramref name="includeNamespance"/> is <see langword="true"/>.</param>
        /// <returns>Formatted string for the given <see cref="TypeInfo"/>.</returns>
        public string GetFriendlyName(bool includeNamespance = true, bool includeDeclaringType = true)
        {
            var genericArgumentsString = GetGenericArgumentsString();
            var typeName = includeNamespance
                ? FullName
                : includeDeclaringType
                ? NameWithoutNameSpace
                : Name;

            if (typeName?.Length > 2 && IsArray)
            {
                typeName = typeName.Substring(0, typeName.Length - 2);
            }

            return $"{typeName}{genericArgumentsString}{(IsArray ? "[]" : null)}";

            string? GetGenericArgumentsString()
            {
                var genericArguments = GenericArguments;
                var genericArgumentsString = IsGenericType && (genericArguments?.Any() ?? false)
                    ? $"[{genericArguments.Select(x => x.GetFriendlyName(includeNamespance, includeDeclaringType)).StringJoin(",")}]"
                    : null;
                return genericArgumentsString;
            }
        }
    }
}
