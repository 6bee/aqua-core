// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.TypeSystem
{
    using Aqua.TypeSystem.Extensions;
    using System;
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
        protected MemberInfo()
        {
        }

        internal MemberInfo(System.Reflection.MemberInfo memberInfo, TypeInfoProvider typeInfoProvider)
        {
            if (memberInfo is null)
            {
                throw new ArgumentNullException(nameof(memberInfo));
            }

            Name = memberInfo.Name;
            DeclaringType = typeInfoProvider.Get(memberInfo.DeclaringType, false, false);
            if (memberInfo.GetBindingFlags().Contains(System.Reflection.BindingFlags.Static))
            {
                IsStatic = true;
            }
        }

        protected MemberInfo(string name, TypeInfo? declaringType)
        {
            Name = name;
            DeclaringType = declaringType;
        }

        internal MemberInfo(MemberInfo memberInfo, TypeInfoProvider typeInfoProvider)
        {
            Name = memberInfo.Name;
            DeclaringType = typeInfoProvider.Get(memberInfo.DeclaringType);
        }

        [Dynamic.Unmapped]
        public abstract MemberTypes MemberType { get; }

        [DataMember(Order = 1, EmitDefaultValue = false)]
        public string? Name { get; set; }

        [DataMember(Order = 2, EmitDefaultValue = false)]
        public TypeInfo? DeclaringType { get; set; }

        [DataMember(Order = 3, EmitDefaultValue = false)]
        public bool? IsStatic { get; set; }

        public static explicit operator System.Reflection.MemberInfo?(MemberInfo? memberInfo) => memberInfo.ResolveMemberInfo(TypeResolver.Instance);

        public override string ToString() => $"{DeclaringType}.{Name}";

        [return: NotNullIfNotNull("memberInfo")]
        public static MemberInfo? Create(System.Reflection.MemberInfo? memberInfo) => Create(memberInfo, new TypeInfoProvider());

        [return: NotNullIfNotNull("memberInfo")]
        internal static MemberInfo? Create(System.Reflection.MemberInfo? memberInfo, TypeInfoProvider typeInfoProvider)
            => memberInfo.GetMemberType() switch
            {
                null => null,
                MemberTypes.Field => new FieldInfo((System.Reflection.FieldInfo)memberInfo!, typeInfoProvider),
                MemberTypes.Constructor => new ConstructorInfo((System.Reflection.ConstructorInfo)memberInfo!, typeInfoProvider),
                MemberTypes.Property => new PropertyInfo((System.Reflection.PropertyInfo)memberInfo!, typeInfoProvider),
                MemberTypes.Method => new MethodInfo((System.Reflection.MethodInfo)memberInfo!, typeInfoProvider),
                _ => throw new Exception($"Unsupported member type: {memberInfo.GetMemberType()}"),
            };
    }
}
