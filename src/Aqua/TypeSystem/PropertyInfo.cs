// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.TypeSystem
{
    using System;
    using System.Diagnostics;
    using System.Runtime.Serialization;

    [Serializable]
    [DataContract(Name = "Property", IsReference = true)]
    [DebuggerDisplay("{Name}")]
    public class PropertyInfo : MemberInfo
    {
        [NonSerialized]
        [Dynamic.Unmapped]
        private System.Reflection.PropertyInfo _property;

        public PropertyInfo()
        {
        }

        public PropertyInfo(System.Reflection.PropertyInfo propertyInfo)
            : this(propertyInfo, new TypeInfoProvider())
        {
        }

        public PropertyInfo(string propertyName, Type propertyType, Type declaringType)
            : this(propertyName, propertyType, declaringType, new TypeInfoProvider())
        {
        }

        public PropertyInfo(string propertyName, TypeInfo propertyType, TypeInfo declaringType)
            : base(propertyName, declaringType)
        {
            PropertyType = propertyType;
        }

        protected PropertyInfo(PropertyInfo propertyInfo)
            : base(propertyInfo, new TypeInfoProvider())
        {
        }

        internal PropertyInfo(PropertyInfo propertyInfo, TypeInfoProvider typeInfoProvider)
            : base(propertyInfo, typeInfoProvider)
        {
            if (propertyInfo is null)
            {
                throw new ArgumentNullException(nameof(propertyInfo));
            }

            PropertyType = typeInfoProvider.Get(propertyInfo.PropertyType);
            _property = propertyInfo._property;
        }

        internal PropertyInfo(System.Reflection.PropertyInfo propertyInfo, TypeInfoProvider typeInfoProvider)
            : base(propertyInfo, typeInfoProvider)
        {
            _property = propertyInfo;
            PropertyType = typeInfoProvider.Get(propertyInfo.PropertyType, false, false);
        }

        private PropertyInfo(string propertyName, Type propertyType, Type declaringType, TypeInfoProvider typeInfoProvider)
            : this(propertyName, typeInfoProvider.Get(propertyType, false, false), typeInfoProvider.Get(declaringType, false, false))
        {
        }

        public override MemberTypes MemberType => MemberTypes.Property;

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public TypeInfo PropertyType { get; set; }

        [Dynamic.Unmapped]
        internal System.Reflection.PropertyInfo Property
        {
            get
            {
                if (_property is null)
                {
                    _property = this.ResolveProperty(TypeResolver.Instance);
                }

                return _property;
            }
        }

        public static explicit operator System.Reflection.PropertyInfo(PropertyInfo p)
            => p.Property;
    }
}
