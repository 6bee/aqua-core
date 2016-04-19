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

    [Serializable]
    [DataContract(Name = "Type", IsReference = true)]
    [DebuggerDisplay("{FullName}")]
    public class TypeInfo
    {
        [NonSerialized]
        private Type _type;

        public TypeInfo()
        {
        }

        public TypeInfo(Type type, bool includePropertyInfos = true)
            : this(type, includePropertyInfos, true, TypeInfo.CreateReferenceTracker<Type>())
        {
        }

        public TypeInfo(Type type, bool includePropertyInfos, bool setMemberDeclaringTypes)
            : this(type, includePropertyInfos, setMemberDeclaringTypes, TypeInfo.CreateReferenceTracker<Type>())
        {
        }

        private TypeInfo(Type type, bool includePropertyInfos, bool setMemberDeclaringTypes, Dictionary<Type, TypeInfo> referenceTracker)
        {
            if (!ReferenceEquals(null, type))
            {
                referenceTracker.Add(type, this);

                _type = type;

                Name = type.Name;

                Namespace = type.Namespace;

                if (type.IsArray)
                {
                    if (!IsArray)
                    {
                        throw new Exception("Name is not in expected format for array type");
                    }

                    type = type.GetElementType();
                }

                if (type.IsNested && !type.IsGenericParameter)
                {
                    DeclaringType = TypeInfo.Create(referenceTracker, type.DeclaringType, false, false);
                }

                if (type.IsGenericType())
                {
                    GenericArguments = type
                        .GetGenericArguments()
                        .Select(x => TypeInfo.Create(referenceTracker, x, includePropertyInfos, setMemberDeclaringTypes))
                        .ToList();
                }

                IsAnonymousType = type.IsAnonymousType();

                if (IsAnonymousType || includePropertyInfos)
                {
                    Properties = type
                        .GetProperties()
                        .Select(x => new PropertyInfo(x.Name, TypeInfo.Create(referenceTracker, x.PropertyType, includePropertyInfos, setMemberDeclaringTypes), setMemberDeclaringTypes ? this : null))
                        .ToList();
                }
            }
        }

        internal protected TypeInfo(TypeInfo typeInfo)
            : this(typeInfo, new Dictionary<TypeInfo, TypeInfo>(ReferenceEqualityComparer<TypeInfo>.Default))
        {
        }

        private TypeInfo(TypeInfo typeInfo, Dictionary<TypeInfo, TypeInfo> referenceTracker)
        {
            if (ReferenceEquals(null, typeInfo))
            {
                throw new ArgumentNullException(nameof(typeInfo));
            }

            referenceTracker.Add(typeInfo, this);

            Name = typeInfo.Name;
            Namespace = typeInfo.Namespace;
            DeclaringType = ReferenceEquals(null, typeInfo.DeclaringType) ? null : Create(referenceTracker, typeInfo.DeclaringType);
            GenericArguments = ReferenceEquals(null, typeInfo.GenericArguments) ? null : typeInfo.GenericArguments.Select(x => Create(referenceTracker, x)).ToList();
            IsAnonymousType = typeInfo.IsAnonymousType;
            Properties = ReferenceEquals(null, typeInfo.Properties) ? null : typeInfo.Properties.Select(x => new PropertyInfo(x, referenceTracker)).ToList();
            _type = typeInfo._type;
        }

        internal static Dictionary<T, TypeInfo> CreateReferenceTracker<T>()
        {
            return new Dictionary<T, TypeInfo>(ReferenceEqualityComparer<T>.Default);
        }

        internal static TypeInfo Create(Dictionary<Type, TypeInfo> referenceTracker, Type type, bool includePropertyInfos, bool setMemberDeclaringTypes)
        {
            TypeInfo typeInfo;
            if (!referenceTracker.TryGetValue(type, out typeInfo))
            {
                typeInfo = new TypeInfo(type, includePropertyInfos, setMemberDeclaringTypes, referenceTracker);
            }

            return typeInfo;
        }

        internal static TypeInfo Create(Dictionary<TypeInfo, TypeInfo> referenceTracker, TypeInfo type)
        {
            if (ReferenceEquals(null, type))
            {
                return null;
            }

            TypeInfo typeInfo;
            if (!referenceTracker.TryGetValue(type, out typeInfo))
            {
                typeInfo = new TypeInfo(type, referenceTracker);
            }

            return typeInfo;
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
        public List<PropertyInfo> Properties { get; set; }

        public bool IsNested { get { return !ReferenceEquals(null, DeclaringType); } }

        public bool IsGenericType { get { return !ReferenceEquals(null, GenericArguments) && GenericArguments.Any(); } }

        public bool IsArray
        {
            get
            {
                var name = Name;
                return !ReferenceEquals(null, name) && name.EndsWith("[]");
            }
        }

        public string FullName
        {
            get
            {
                if (IsNested)
                {
                    return $"{DeclaringType.FullName}+{Name}";
                }
                else
                {
                    return $"{Namespace}{(string.IsNullOrEmpty(Namespace) ? null : ".")}{Name}";
                }
            }
        }

        /// <summary>
        /// Resolves this type info instance to it's type using the default type resolver instance.
        /// </summary>
        public Type Type
        {
            get
            {
                if (ReferenceEquals(null, _type))
                {
                    _type = TypeResolver.Instance.ResolveType(this);
                }
                return _type;
            }
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return Equals(obj as TypeInfo);
        }

        public bool Equals(TypeInfo typeInfo)
        {
            if (ReferenceEquals(null, typeInfo)) return false;
            if (ReferenceEquals(this, typeInfo)) return true;
            var s0 = ToString();
            var s1 = typeInfo.ToString();
            return string.Equals(s0, s1, StringComparison.Ordinal);
        }

        public override int GetHashCode()
        {
            var s = ToString();
            return s.GetHashCode();
        }

        public override string ToString()
        {
            var genericArguments = IsGenericType
                ? string.Format("[{0}]", string.Join(",", GenericArguments.Select(x => x.ToString()).ToArray()))
                : null;
            return $"{FullName}{genericArguments}";
        }

        public static explicit operator Type(TypeInfo t)
        {
            return t.Type;
        }
    }
}
