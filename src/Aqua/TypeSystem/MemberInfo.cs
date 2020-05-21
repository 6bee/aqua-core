// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.TypeSystem
{
    using Aqua.Dynamic;
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

        internal MemberInfo(System.Reflection.MemberInfo member, TypeInfoProvider typeInfoProvider)
        {
            if (member is null)
            {
                throw new ArgumentNullException(nameof(member));
            }

            Name = member.Name;
            DeclaringType = typeInfoProvider.Get(member.DeclaringType, false, false);
            if (member.GetBindingFlags().Contains(System.Reflection.BindingFlags.Static))
            {
                IsStatic = true;
            }
        }

        protected MemberInfo(string name, TypeInfo? declaringType)
        {
            Name = name;
            DeclaringType = declaringType;
        }

        internal MemberInfo(MemberInfo member, TypeInfoProvider typeInfoProvider)
        {
            Name = member.Name;
            DeclaringType = typeInfoProvider.Get(member.DeclaringType);
        }

        [Unmapped]
        public abstract MemberTypes MemberType { get; }

        [DataMember(Order = 1, EmitDefaultValue = false)]
        public string? Name { get; set; }

        [DataMember(Order = 2, EmitDefaultValue = false)]
        public TypeInfo? DeclaringType { get; set; }

        [DataMember(Order = 3, EmitDefaultValue = false)]
        public bool? IsStatic { get; set; }

        public static explicit operator System.Reflection.MemberInfo?(MemberInfo? member) => member.ResolveMemberInfo(TypeResolver.Instance);

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
