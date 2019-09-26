// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.TypeSystem
{
    using Aqua.TypeSystem.Extensions;
    using System;
    using System.Reflection;
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
        internal static class Scope
        {
            public const BindingFlags PublicInstance = BindingFlags.Public | BindingFlags.Instance;
            public const BindingFlags Any = PublicInstance | BindingFlags.NonPublic | BindingFlags.Static;
        }

        protected MemberInfo()
        {
        }

        internal MemberInfo(System.Reflection.MemberInfo memberInfo, TypeInfoProvider typeInfoProvider)
        {
            if (!(memberInfo is null))
            {
                Name = memberInfo.Name;
                DeclaringType = typeInfoProvider.Get(memberInfo.DeclaringType, false, false);
            }
        }

        protected MemberInfo(string name, TypeInfo declaringType)
        {
            Name = name;
            DeclaringType = declaringType;
        }

        internal MemberInfo(MemberInfo memberInfo, TypeInfoProvider typeInfoProvider)
        {
            if (!(memberInfo is null))
            {
                Name = memberInfo.Name;
                DeclaringType = typeInfoProvider.Get(memberInfo.DeclaringType);
            }
        }

        [Dynamic.Unmapped]
        public abstract MemberTypes MemberType { get; }

        [DataMember(Order = 1, EmitDefaultValue = false)]
        public string Name { get; set; }

        [DataMember(Order = 2, EmitDefaultValue = false)]
        public TypeInfo DeclaringType { get; set; }

        public static explicit operator System.Reflection.MemberInfo(MemberInfo memberInfo)
            => memberInfo.ResolveMemberInfo(TypeResolver.Instance);

        public override string ToString()
            => $"{DeclaringType}.{Name}";

        public static MemberInfo Create(System.Reflection.MemberInfo memberInfo)
            => Create(memberInfo, new TypeInfoProvider());

        internal static MemberInfo Create(System.Reflection.MemberInfo memberInfo, TypeInfoProvider typeInfoProvider)
        {
            switch (memberInfo.GetMemberType())
            {
                case MemberTypes.Field:
                    return new FieldInfo((System.Reflection.FieldInfo)memberInfo, typeInfoProvider);

                case MemberTypes.Constructor:
                    return new ConstructorInfo((System.Reflection.ConstructorInfo)memberInfo, typeInfoProvider);

                case MemberTypes.Property:
                    return new PropertyInfo((System.Reflection.PropertyInfo)memberInfo, typeInfoProvider);

                case MemberTypes.Method:
                    return new MethodInfo((System.Reflection.MethodInfo)memberInfo, typeInfoProvider);

                default:
                    throw new Exception($"Not supported member type: {memberInfo.GetMemberType()}");
            }
        }
    }
}
