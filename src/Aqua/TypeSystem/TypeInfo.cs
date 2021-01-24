// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.TypeSystem
{
    using Aqua.Dynamic;
    using Aqua.TypeExtensions;
    using System;
    using System.Collections.Generic;
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

        [NonSerialized]
        [Unmapped]
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

        public TypeInfo(TypeInfo type)
            : this(type.CheckNotNull(nameof(type)), new TypeInfoProvider())
        {
        }

        internal TypeInfo(TypeInfo typeInfo, TypeInfoProvider typeInfoProvider)
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

        [Unmapped]
        public bool IsNested => DeclaringType is not null;

        [Unmapped]
        public bool IsGenericTypeDefinition => IsGenericType && (!GenericArguments?.Any() ?? true);

        [Unmapped]
        public bool IsArray
        {
            get
            {
                var name = Name;
                return name is not null && _arrayNameRegex.IsMatch(name);
            }
        }

        [Unmapped]
        public string FullName
            => IsNested
                ? $"{DeclaringType!.FullName}+{Name}"
                : $"{Namespace}{(string.IsNullOrEmpty(Namespace) ? null : ".")}{Name}";

        [Unmapped]
        internal string NameWithoutNameSpace
            => IsNested
                ? $"{DeclaringType!.NameWithoutNameSpace}+{Name}"
                : Name ?? string.Empty;

        /// <summary>
        /// Gets <see cref="Type"/> by resolving this <see cref="TypeInfo"/> instance using the default <see cref="TypeResolver"/>.
        /// </summary>
        [Unmapped]
        [Obsolete("Use method ToType() instead, this property is being removed in a future version.", false)]
        public Type Type => ToType();

        /// <summary>
        /// Returns the <see cref="Type"/> represented by this <see cref="TypeInfo"/> instance by resolving it using the default <see cref="TypeResolver"/>.
        /// </summary>
        public Type ToType() => _type ?? (_type = this.ResolveType(TypeResolver.Instance));

        public static explicit operator Type?(TypeInfo? type) => type?.ToType();

        public override string ToString() => this.PrintFriendlyName();
    }
}
