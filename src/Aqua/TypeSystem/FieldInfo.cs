// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.TypeSystem
{
    using System;
    using System.Diagnostics;
    using System.Runtime.Serialization;

    [Serializable]
    [DataContract(Name = "Field", IsReference = true)]
    [DebuggerDisplay("{Name}")]
    public class FieldInfo : MemberInfo
    {
        [NonSerialized]
        [Dynamic.Unmapped]
        private System.Reflection.FieldInfo _field;

        public FieldInfo()
        {
        }

        public FieldInfo(System.Reflection.FieldInfo fieldInfo)
            : this(fieldInfo, new TypeInfoProvider())
        {
        }

        internal FieldInfo(System.Reflection.FieldInfo fieldInfo, TypeInfoProvider typeInfoProvider)
            : base(fieldInfo, typeInfoProvider)
        {
            _field = fieldInfo;
        }

        public FieldInfo(string fieldName, Type declaringType)
            : this(fieldName, new TypeInfoProvider().Get(declaringType, false, false))
        {
        }

        public FieldInfo(string fieldName, TypeInfo declaringType)
            : base(fieldName, declaringType)
        {
        }

        protected FieldInfo(FieldInfo fieldInfo)
            : base(fieldInfo, new TypeInfoProvider())
        {
        }

        public override MemberTypes MemberType => MemberTypes.Field;

        [Dynamic.Unmapped]
        internal System.Reflection.FieldInfo Field
            => _field ?? (_field = this.ResolveField(TypeResolver.Instance));

        public static explicit operator System.Reflection.FieldInfo(FieldInfo f)
            => f.Field;
    }
}
