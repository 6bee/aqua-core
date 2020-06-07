// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.TypeSystem
{
    using Aqua.Dynamic;
    using System;
    using System.Diagnostics;
    using System.Runtime.Serialization;

    [Serializable]
    [DataContract(Name = "Field", IsReference = true)]
    [DebuggerDisplay("Field: {Name,nq}")]
    public class FieldInfo : MemberInfo
    {
        [NonSerialized]
        [Unmapped]
        private System.Reflection.FieldInfo? _field;

        public FieldInfo()
        {
        }

        public FieldInfo(System.Reflection.FieldInfo field)
            : this(field, new TypeInfoProvider())
        {
        }

        public FieldInfo(System.Reflection.FieldInfo field, TypeInfoProvider typeInfoProvider)
            : base(field, typeInfoProvider)
        {
            _field = field;
        }

        public FieldInfo(string fieldName, Type declaringType)
            : this(fieldName, new TypeInfoProvider().GetTypeInfo(declaringType, false, false))
        {
        }

        public FieldInfo(string fieldName, TypeInfo declaringType)
            : base(fieldName, declaringType)
        {
        }

        protected FieldInfo(FieldInfo field)
            : base(field, new TypeInfoProvider())
        {
        }

        public override MemberTypes MemberType => MemberTypes.Field;

        [Unmapped]
        [Obsolete("Use method ToFieldInfo() instead", true)]
        public System.Reflection.FieldInfo Field => ToFieldInfo();

        public static explicit operator System.Reflection.FieldInfo(FieldInfo field)
            => field.CheckNotNull(nameof(field)).ToFieldInfo();

        public System.Reflection.FieldInfo ToFieldInfo()
            => _field ?? (_field = this.ResolveField(TypeResolver.Instance))
            ?? throw new TypeResolverException($"Failed to resolve field, consider using extension method to specify {nameof(ITypeResolver)}.");
    }
}
