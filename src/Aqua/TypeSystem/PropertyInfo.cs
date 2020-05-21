﻿// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.TypeSystem
{
    using Aqua.Dynamic;
    using System;
    using System.Diagnostics;
    using System.Runtime.Serialization;

    [Serializable]
    [DataContract(Name = "Property", IsReference = true)]
    [DebuggerDisplay("Property: {Name,nq}")]
    public class PropertyInfo : MemberInfo
    {
        [NonSerialized]
        [Unmapped]
        private System.Reflection.PropertyInfo? _property;

        public PropertyInfo()
        {
        }

        public PropertyInfo(System.Reflection.PropertyInfo property)
            : this(property, new TypeInfoProvider())
        {
        }

        public PropertyInfo(string propertyName, Type propertyType, Type declaringType)
            : this(propertyName, propertyType, declaringType, new TypeInfoProvider())
        {
        }

        public PropertyInfo(string propertyName, TypeInfo propertyType, TypeInfo? declaringType)
            : base(propertyName, declaringType)
        {
            PropertyType = propertyType;
        }

        protected PropertyInfo(PropertyInfo property)
            : base(property, new TypeInfoProvider())
        {
        }

        internal PropertyInfo(PropertyInfo property, TypeInfoProvider typeInfoProvider)
            : base(property, typeInfoProvider)
        {
            PropertyType = typeInfoProvider.Get(property.PropertyType);
            _property = property._property;
        }

        internal PropertyInfo(System.Reflection.PropertyInfo property, TypeInfoProvider typeInfoProvider)
            : base(property, typeInfoProvider)
        {
            _property = property;
            PropertyType = typeInfoProvider.Get(property.PropertyType, false, false);
        }

        private PropertyInfo(string propertyName, Type propertyType, Type declaringType, TypeInfoProvider typeInfoProvider)
            : this(propertyName, typeInfoProvider.Get(propertyType, false, false), typeInfoProvider.Get(declaringType, false, false))
        {
        }

        public override MemberTypes MemberType => MemberTypes.Property;

        [DataMember(Order = 4, IsRequired = false, EmitDefaultValue = false)]
        public TypeInfo? PropertyType { get; set; }

        [Unmapped]
        public System.Reflection.PropertyInfo Property
            => _property ?? (_property = this.ResolveProperty(TypeResolver.Instance))
            ?? throw new TypeResolverException($"Failed to resolve property, consider using extension method to specify {nameof(ITypeResolver)}.");

        public static explicit operator System.Reflection.PropertyInfo(PropertyInfo p) => p.Property;
    }
}
