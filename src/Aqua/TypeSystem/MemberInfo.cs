// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.TypeSystem
{
    using Aqua.Dynamic;
    using Aqua.TypeSystem.Extensions;
    using System;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.Serialization;
    using System.Xml.Serialization;

    [Serializable]
    [DataContract(Name = "Member", IsReference = true)]
    [KnownType(typeof(ConstructorInfo)), XmlInclude(typeof(ConstructorInfo))]
    [KnownType(typeof(FieldInfo)), XmlInclude(typeof(FieldInfo))]
    [KnownType(typeof(MethodInfo)), XmlInclude(typeof(MethodInfo))]
    [KnownType(typeof(PropertyInfo)), XmlInclude(typeof(PropertyInfo))]
    public abstract class MemberInfo
    {
        [IgnoreDataMember]
        [Unmapped]
        [NonSerialized]
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private System.Reflection.MemberInfo? _member;

        protected MemberInfo()
        {
        }

        protected MemberInfo(System.Reflection.MemberInfo member, TypeInfoProvider typeInfoProvider)
        {
            _member = member;
            Name = member.CheckNotNull().Name;
            DeclaringType = typeInfoProvider.CheckNotNull().GetTypeInfo(member.DeclaringType, false, false);
            var isStatic = member switch
            {
                System.Reflection.MethodBase method => method.IsStatic,
                System.Reflection.PropertyInfo property => (property.GetMethod ?? property.SetMethod)?.IsStatic,
                System.Reflection.FieldInfo field => field.IsStatic,
                _ => member.GetBindingFlags().Contains(System.Reflection.BindingFlags.Static),
            };
            IsStatic = isStatic is true
                ? true
                : default(bool?);
        }

        protected MemberInfo(string name, TypeInfo? declaringType)
        {
            Name = name;
            DeclaringType = declaringType;
        }

        protected MemberInfo(MemberInfo member, TypeInfoProvider typeInfoProvider)
        {
            Name = member.CheckNotNull().Name;
            DeclaringType = typeInfoProvider.CheckNotNull().Get(member.DeclaringType);
        }

        [IgnoreDataMember]
        [Unmapped]
        public abstract MemberTypes MemberType { get; }

        [DataMember(Order = 1, EmitDefaultValue = false)]
        public string? Name { get; set; }

        [DataMember(Order = 2, EmitDefaultValue = false)]
        public TypeInfo? DeclaringType { get; set; }

        [DataMember(Order = 3, EmitDefaultValue = false)]
        public bool? IsStatic { get; set; }

        public static explicit operator System.Reflection.MemberInfo?(MemberInfo? member)
            => member?.ToMemberInfo();

        public System.Reflection.MemberInfo ToMemberInfo()
            => _member ??= this.ResolveMemberInfo(TypeResolver.Instance)
            ?? throw new TypeResolverException($"Failed to resolve member, consider using extension method to specify {nameof(ITypeResolver)}.");

        public override string ToString() => $"{DeclaringType}.{Name}";

        [return: NotNullIfNotNull("member")]
        public static MemberInfo? Create(System.Reflection.MemberInfo? member) => Create(member, new TypeInfoProvider());

        [return: NotNullIfNotNull("member")]
        internal static MemberInfo? Create(System.Reflection.MemberInfo? member, TypeInfoProvider typeInfoProvider)
            => member.GetMemberType() switch
            {
                null => null,
                MemberTypes.Field => new FieldInfo((System.Reflection.FieldInfo)member!, typeInfoProvider),
                MemberTypes.Constructor => new ConstructorInfo((System.Reflection.ConstructorInfo)member!, typeInfoProvider),
                MemberTypes.Property => new PropertyInfo((System.Reflection.PropertyInfo)member!, typeInfoProvider),
                MemberTypes.Method => new MethodInfo((System.Reflection.MethodInfo)member!, typeInfoProvider),
                _ => throw new Exception($"Unsupported member type: {member.GetMemberType()}"),
            };
    }
}
