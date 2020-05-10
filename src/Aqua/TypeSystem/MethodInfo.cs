// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.TypeSystem
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime.Serialization;

    [Serializable]
    [DataContract(Name = "Method", IsReference = true)]
    [DebuggerDisplay("Method: {Name,nq}")]
    public class MethodInfo : MethodBaseInfo
    {
        [NonSerialized]
        [Dynamic.Unmapped]
        private System.Reflection.MethodInfo? _method;

        public MethodInfo()
        {
        }

        public MethodInfo(System.Reflection.MethodInfo methodInfo)
            : this(methodInfo, new TypeInfoProvider())
        {
        }

        internal MethodInfo(System.Reflection.MethodInfo methodInfo, TypeInfoProvider typeInfoProvider)
            : base(methodInfo, typeInfoProvider)
        {
            _method = methodInfo;
            ReturnType = typeInfoProvider.Get(methodInfo?.ReturnType, false, false);
        }

        public MethodInfo(string name, Type declaringType, IEnumerable<Type>? genericArguments = null, IEnumerable<Type>? parameterTypes = null, Type? returnType = null)
            : this(name, declaringType, genericArguments, parameterTypes, returnType, new TypeInfoProvider())
        {
        }

        internal MethodInfo(string name, Type declaringType, IEnumerable<Type>? genericArguments, IEnumerable<Type>? parameterTypes, Type? returnType, TypeInfoProvider typeInfoProvider)
            : base(name, declaringType, genericArguments, parameterTypes, typeInfoProvider)
        {
            ReturnType = typeInfoProvider.Get(returnType, false, false);
        }

        public MethodInfo(string name, TypeInfo declaringType, IEnumerable<TypeInfo>? genericArguments = null, IEnumerable<TypeInfo>? parameterTypes = null, TypeInfo? returnType = null)
            : base(name, declaringType, genericArguments, parameterTypes)
        {
            ReturnType = returnType;
        }

        protected MethodInfo(MethodInfo methodInfo)
            : base(methodInfo, new TypeInfoProvider())
        {
        }

        public override MemberTypes MemberType => MemberTypes.Method;

        [DataMember(Order = 7, IsRequired = false, EmitDefaultValue = false)]
        public TypeInfo? ReturnType { get; set; }

        [Dynamic.Unmapped]
        public System.Reflection.MethodInfo Method
            => _method ?? (_method = this.ResolveMethod(TypeResolver.Instance))
            ?? throw new TypeResolverException($"Failed to resolve method, consider using extension method to specify {nameof(ITypeResolver)}.");

        public override string ToString() => $"{ReturnType} {base.ToString()}".Trim();

        public static explicit operator System.Reflection.MethodInfo(MethodInfo m) => m.Method;
    }
}
