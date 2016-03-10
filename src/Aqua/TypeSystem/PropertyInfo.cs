// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.TypeSystem
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;

    [Serializable]
    [DataContract(Name = "Property", IsReference = true)]
    public class PropertyInfo : MemberInfo
    {
        [NonSerialized]
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
                throw new ArgumentNullException("propertyInfo");
            }
            
            PropertyType = TypeInfo.Create(referenceTracker, propertyInfo.PropertyType);
            _property = propertyInfo._property;
        }

        public override MemberTypes MemberType { get { return Aqua.TypeSystem.MemberTypes.Property; } }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public TypeInfo PropertyType { get; set; }

        internal System.Reflection.PropertyInfo Property
        {
            get
            {
                if (ReferenceEquals(null, _property))
                {
                    _property = ResolveProperty(TypeResolver.Instance);
                }
                return _property;
            }
        }

        public System.Reflection.PropertyInfo ResolveProperty(ITypeResolver typeResolver)
        {
            Type declaringType;
            try
            {
                declaringType = typeResolver.ResolveType(DeclaringType);
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("Declaring type '{0}' could not be reconstructed", DeclaringType), ex);
            }

            var propertyInfo = declaringType.GetProperty(Name);
            return propertyInfo;
        }

        public static explicit operator System.Reflection.PropertyInfo(PropertyInfo p)
        {
            return p.Property;
        }
    }
}
