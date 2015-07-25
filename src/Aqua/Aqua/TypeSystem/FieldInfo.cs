// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.TypeSystem
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    [DataContract(Name = "Field", IsReference = true)]
    public sealed class FieldInfo : MemberInfo
    {
        [NonSerialized]
        private System.Reflection.FieldInfo _field;

        public FieldInfo()
        {
        }

        public FieldInfo(System.Reflection.FieldInfo fieldInfo)
            : base(fieldInfo, TypeInfo.CreateReferenceTracker<Type>())
        {
            _field = fieldInfo;
        }

        public FieldInfo(string fieldName, Type declaringType)
            : this(fieldName, TypeInfo.Create(TypeInfo.CreateReferenceTracker<Type>(), declaringType, includePropertyInfos: false))
        {
        }

        public FieldInfo(string fieldName, TypeInfo declaringType)
            : base(fieldName, declaringType)
        {
        }

        protected FieldInfo(FieldInfo fieldInfo)
            : base(fieldInfo, TypeInfo.CreateReferenceTracker<TypeInfo>())
        {
        }

        public override MemberTypes MemberType { get { return Aqua.TypeSystem.MemberTypes.Field; } }

        internal System.Reflection.FieldInfo Field
        {
            get
            {
                if (ReferenceEquals(null, _field))
                {
                    _field = ResolveField(TypeResolver.Instance);
                }

                return _field;
            }
        }

        public System.Reflection.FieldInfo ResolveField(ITypeResolver typeResolver)
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

            var fieldInfo = declaringType.GetField(Name);
            return fieldInfo;
        }

        public static explicit operator System.Reflection.FieldInfo(FieldInfo f)
        {
            return f.Field;
        }
    }
}
