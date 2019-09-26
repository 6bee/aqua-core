// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.TypeSystem
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.Serialization;
    using System.Xml.Serialization;
    using BindingFlags = System.Reflection.BindingFlags;

    [Serializable]
    [DataContract(Name = "MethodBase", IsReference = true)]
    [KnownType(typeof(ConstructorInfo)), XmlInclude(typeof(ConstructorInfo))]
    [KnownType(typeof(MethodInfo)), XmlInclude(typeof(MethodInfo))]
    public abstract class MethodBaseInfo : MemberInfo
    {
        protected MethodBaseInfo()
        {
        }

        internal MethodBaseInfo(System.Reflection.MethodBase methodInfo, TypeInfoProvider typeInfoProvider)
            : base(methodInfo, typeInfoProvider)
        {
            var bindingFlags = methodInfo.IsStatic ? BindingFlags.Static : BindingFlags.Instance;
            bindingFlags |= methodInfo.IsPublic ? BindingFlags.Public : BindingFlags.NonPublic;
            BindingFlags = bindingFlags;

            var genericArguments = methodInfo.IsGenericMethod ? methodInfo.GetGenericArguments() : null;
            GenericArgumentTypes = genericArguments is null || genericArguments.Length == 0
                ? null
                : genericArguments.Select(x => typeInfoProvider.Get(x, false, false)).ToList();

            var parameters = methodInfo.GetParameters();
            ParameterTypes = parameters.Length == 0
                ? null
                : parameters.Select(x => typeInfoProvider.Get(x.ParameterType, false, false)).ToList();
        }

        // TODO: replace binding flags by bool flags
        internal MethodBaseInfo(string name, Type declaringType, BindingFlags bindingFlags, Type[] genericArguments, Type[] parameterTypes, TypeInfoProvider typeInfoProvider)
            : this(
            name,
            typeInfoProvider.Get(declaringType, includePropertyInfosOverride: false, setMemberDeclaringTypesOverride: false),
            bindingFlags,
            genericArguments?.Select(x => typeInfoProvider.Get(x, false, false)),
            parameterTypes?.Select(x => typeInfoProvider.Get(x, false, false)))
        {
        }

        protected MethodBaseInfo(string name, TypeInfo declaringType, BindingFlags bindingFlags, IEnumerable<TypeInfo> genericArguments, IEnumerable<TypeInfo> parameterTypes)
            : base(name, declaringType)
        {
            BindingFlags = bindingFlags;

            GenericArgumentTypes = genericArguments is null || !genericArguments.Any()
                ? null
                : genericArguments.ToList();

            ParameterTypes = parameterTypes is null || !parameterTypes.Any()
                ? null
                : parameterTypes.ToList();
        }

        internal MethodBaseInfo(MethodBaseInfo methodBaseInfo, TypeInfoProvider typeInfoProvider)
            : base(methodBaseInfo, typeInfoProvider)
        {
            BindingFlags = methodBaseInfo.BindingFlags;
            GenericArgumentTypes = methodBaseInfo.GenericArgumentTypes?.Select(typeInfoProvider.Get).ToList();
            ParameterTypes = methodBaseInfo.ParameterTypes?.Select(typeInfoProvider.Get).ToList();
        }

        [DataMember(Order = 1, IsRequired = false, EmitDefaultValue = false)]
        public BindingFlags BindingFlags { get; set; }

        [DataMember(Order = 2, IsRequired = false, EmitDefaultValue = false)]
        public List<TypeInfo> GenericArgumentTypes { get; set; }

        [DataMember(Order = 3, IsRequired = false, EmitDefaultValue = false)]
        public List<TypeInfo> ParameterTypes { get; set; }

        public bool IsGenericMethod => !(GenericArgumentTypes is null) && GenericArgumentTypes.Any();

        public override string ToString()
        {
            var hasGenericArguments = !(GenericArgumentTypes is null) && GenericArgumentTypes.Any();
            return string.Format(
                "{0}.{1}{3}{4}{5}({2})",
                DeclaringType,
                Name,
                ParameterTypes is null ? null : string.Join(", ", ParameterTypes.Select(x => x.ToString()).ToArray()),
                hasGenericArguments ? "<" : null,
                hasGenericArguments ? string.Join(", ", GenericArgumentTypes.Select(x => x.ToString()).ToArray()) : null,
                hasGenericArguments ? ">" : null);
        }
    }
}
