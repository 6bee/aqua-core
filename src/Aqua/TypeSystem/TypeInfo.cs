// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.TypeSystem
{
    using Aqua.TypeSystem.Extensions;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.Serialization;
    using System.Text.RegularExpressions;

    [Serializable]
    [DataContract(Name = "Type", IsReference = true)]
    [DebuggerDisplay("{FullName}")]
    public class TypeInfo
    {
        private static readonly Regex _arrayNameRegex = new Regex(@"^.*\[,*\]$");

        [NonSerialized]
        [Dynamic.Unmapped]
        private Type _type;

        public TypeInfo()
        {
        }

        public TypeInfo(Type type, bool includePropertyInfos = true)
            : this(type, includePropertyInfos, true)
        {
        }

        public TypeInfo(Type type, bool includePropertyInfos, bool setMemberDeclaringTypes)
            : this(type, new TypeInfoProvider(includePropertyInfos, setMemberDeclaringTypes))
        {
        }

        internal TypeInfo(Type type, TypeInfoProvider typeInfoProvider)
        {
            if (!(type is null))
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

                    type = type.GetElementType();
                }

                if (type.IsNested && !type.IsGenericParameter)
                {
                    DeclaringType = typeInfoProvider.Get(type.DeclaringType, false, false);
                }

                IsGenericType = type.IsGenericType();

                if (IsGenericType && !type.GetTypeInfo().IsGenericTypeDefinition)
                {
                    GenericArguments = type
                        .GetGenericArguments()
                        .Select(x => typeInfoProvider.Get(x))
                        .ToList();
                }

                IsAnonymousType = type.IsAnonymousType();

                if (IsAnonymousType || typeInfoProvider.IncludePropertyInfos)
                {
                    Properties = type
                        .GetProperties()
                        .OrderBy(x => x.MetadataToken)
                        .Select(x => new PropertyInfo(x.Name, typeInfoProvider.Get(x.PropertyType), typeInfoProvider.SetMemberDeclaringTypes ? this : null))
                        .ToList();
                }
            }
        }

        public TypeInfo(TypeInfo typeInfo)
            : this(typeInfo, new TypeInfoProvider())
        {
        }

        internal TypeInfo(TypeInfo typeInfo, TypeInfoProvider typeInfoProvider)
        {
            if (typeInfo is null)
            {
                throw new ArgumentNullException(nameof(typeInfo));
            }

            typeInfoProvider.RegisterReference(typeInfo, this);

            Name = typeInfo.Name;
            Namespace = typeInfo.Namespace;
            DeclaringType = typeInfo.DeclaringType is null ? null : typeInfoProvider.Get(typeInfo.DeclaringType);
            GenericArguments = typeInfo.GenericArguments?.Select(typeInfoProvider.Get).ToList();
            IsGenericType = typeInfo.IsGenericType;
            IsAnonymousType = typeInfo.IsAnonymousType;
            Properties = typeInfo.Properties?.Select(x => new PropertyInfo(x, typeInfoProvider)).ToList();
            _type = typeInfo._type;
        }

        [DataMember(Order = 1, EmitDefaultValue = false)]
        public string Name { get; set; }

        [DataMember(Order = 2, IsRequired = false, EmitDefaultValue = false)]
        public string Namespace { get; set; }

        [DataMember(Order = 3, IsRequired = false, EmitDefaultValue = false)]
        public TypeInfo DeclaringType { get; set; }

        [DataMember(Order = 4, IsRequired = false, EmitDefaultValue = false)]
        public List<TypeInfo> GenericArguments { get; set; }

        [DataMember(Order = 5, IsRequired = false, EmitDefaultValue = false)]
        public bool IsAnonymousType { get; set; }

        [DataMember(Order = 6, IsRequired = false, EmitDefaultValue = false)]
        public bool IsGenericType { get; set; }

        [DataMember(Order = 7, IsRequired = false, EmitDefaultValue = false)]
        public List<PropertyInfo> Properties { get; set; }

        public bool IsNested => !(DeclaringType is null);

        public bool IsGenericTypeDefinition => !GenericArguments?.Any() ?? true;

        public bool IsArray
        {
            get
            {
                var name = Name;
                return !(name is null) && _arrayNameRegex.IsMatch(name);
            }
        }

        [Dynamic.Unmapped]
        public string FullName
            => IsNested
                ? $"{DeclaringType.FullName}+{Name}"
                : $"{Namespace}{(string.IsNullOrEmpty(Namespace) ? null : ".")}{Name}";

        /// <summary>
        /// Gets <see cref="Type"/> by resolving this <see cref="TypeInfo"/> instance using the default <see cref="TypeResolver"/>.
        /// </summary>
        [Dynamic.Unmapped]
        public Type Type
        {
            get
            {
                if (_type is null)
                {
                    _type = TypeResolver.Instance.ResolveType(this);
                }

                return _type;
            }
        }

        public static explicit operator Type(TypeInfo t)
            => t.Type;

        public override string ToString()
            => $"{FullName}{GetGenericArgumentsString()}";

        private string GetGenericArgumentsString()
        {
            var genericArguments = GenericArguments;
            var genericArgumentsString = IsGenericType && (genericArguments?.Any() ?? false)
                ? string.Format("[{0}]", string.Join(",", genericArguments.Select(x => x.ToString()).ToArray()))
                : null;
            return genericArgumentsString;
        }
    }
}
