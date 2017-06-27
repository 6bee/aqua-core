// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.TypeSystem
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Reflection;
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
            : this(propertyInfo, TypeInfo.CreateReferenceTracker<Type>())
        {
        }
        
        private PropertyInfo(System.Reflection.PropertyInfo propertyInfo, Dictionary<Type, TypeInfo> referenceTracker)
            : base(propertyInfo, referenceTracker)
        {
            _property = propertyInfo;
            PropertyType = TypeInfo.Create(referenceTracker, propertyInfo.PropertyType, false, false);
        }

        public PropertyInfo(string propertyName, Type propertyType, Type declaringType)
            : this(propertyName, propertyType, declaringType, TypeInfo.CreateReferenceTracker<Type>())
        {
        }

        private PropertyInfo(string propertyName, Type propertyType, Type declaringType, Dictionary<Type, TypeInfo> referenceTracker)
            : this(propertyName, TypeInfo.Create(referenceTracker, propertyType, false, false), TypeInfo.Create(referenceTracker, declaringType, false, false))
        {
        }

        public PropertyInfo(string propertyName, TypeInfo propertyType, TypeInfo declaringType)
            : base(propertyName, declaringType)
        {
            PropertyType = propertyType;
        }

        protected PropertyInfo(PropertyInfo propertyInfo)
            : base(propertyInfo, TypeInfo.CreateReferenceTracker<TypeInfo>())
        {
        }

        internal PropertyInfo(PropertyInfo propertyInfo, Dictionary<TypeInfo, TypeInfo> referenceTracker)
            : base(propertyInfo, referenceTracker)
        {
            if (ReferenceEquals(null, propertyInfo))
            {
                throw new ArgumentNullException(nameof(propertyInfo));
            }
            
            PropertyType = TypeInfo.Create(referenceTracker, propertyInfo.PropertyType);
            _property = propertyInfo._property;
        }

        public override MemberTypes MemberType => MemberTypes.Property;

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public TypeInfo PropertyType { get; set; }

        [Dynamic.Unmapped]
        internal System.Reflection.PropertyInfo Property
        {
            get
            {
                if (ReferenceEquals(null, _property))
                {
                    _property = this.ResolveProperty(TypeResolver.Instance);
                }

                return _property;
            }
        }

        public static explicit operator System.Reflection.PropertyInfo(PropertyInfo p)
        {
            return p.Property;
        }
    }
}
