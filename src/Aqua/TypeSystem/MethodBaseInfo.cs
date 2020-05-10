// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.TypeSystem
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.Serialization;
    using System.Xml.Serialization;

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
            var genericArguments = methodInfo.IsGenericMethod ? methodInfo.GetGenericArguments() : null;
            GenericArgumentTypes = genericArguments is null || genericArguments.Length == 0
                ? null
                : genericArguments.Select(x => typeInfoProvider.Get(x, false, false)).ToList();

            var parameters = methodInfo.GetParameters();
            ParameterTypes = parameters.Length == 0
                ? null
                : parameters.Select(x => typeInfoProvider.Get(x.ParameterType, false, false)).ToList();
        }

        internal MethodBaseInfo(string name, Type declaringType, IEnumerable<Type>? genericArguments, IEnumerable<Type>? parameterTypes, TypeInfoProvider typeInfoProvider)
            : this(
            name,
            typeInfoProvider.Get(declaringType, includePropertyInfosOverride: false, setMemberDeclaringTypesOverride: false),
            genericArguments?.Select(x => typeInfoProvider.Get(x, false, false)),
            parameterTypes?.Select(x => typeInfoProvider.Get(x, false, false)))
        {
        }

        protected MethodBaseInfo(string name, TypeInfo declaringType, IEnumerable<TypeInfo>? genericArguments, IEnumerable<TypeInfo>? parameterTypes)
            : base(name, declaringType)
        {
            GenericArgumentTypes = genericArguments?.ToList();
            ParameterTypes = parameterTypes?.ToList();
        }

        internal MethodBaseInfo(MethodBaseInfo methodBaseInfo, TypeInfoProvider typeInfoProvider)
            : base(methodBaseInfo, typeInfoProvider)
        {
            // TODO: why is the dammit operator required!?
            GenericArgumentTypes = methodBaseInfo.GenericArgumentTypes?.Select(typeInfoProvider.Get).ToList() !;
            ParameterTypes = methodBaseInfo.ParameterTypes?.Select(typeInfoProvider.Get).ToList() !;
        }

        [DataMember(Order = 5, IsRequired = false, EmitDefaultValue = false)]
        public List<TypeInfo>? GenericArgumentTypes { get; set; }

        [DataMember(Order = 6, IsRequired = false, EmitDefaultValue = false)]
        public List<TypeInfo>? ParameterTypes { get; set; }

        public bool IsGenericMethod => GenericArgumentTypes?.Any() ?? false;

        public override string ToString()
        {
            var hasGenericArguments = IsGenericMethod;
            return string.Format(
                "{0}.{1}{3}{4}{5}({2})",
                DeclaringType,
                Name,
                string.Join(", ", ParameterTypes ?? Enumerable.Empty<TypeInfo>()),
                hasGenericArguments ? "<" : null,
                hasGenericArguments ? string.Join(", ", GenericArgumentTypes) : null,
                hasGenericArguments ? ">" : null);
        }
    }
}
