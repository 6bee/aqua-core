// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.TypeSystem
{
    using Aqua.Dynamic;
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
        [Unmapped]
        private System.Reflection.MethodInfo? _method;

        public MethodInfo()
        {
        }

        public MethodInfo(System.Reflection.MethodInfo method)
            : this(method, new TypeInfoProvider())
        {
        }

        public MethodInfo(System.Reflection.MethodInfo method, TypeInfoProvider typeInfoProvider)
            : base(method, typeInfoProvider)
        {
            _method = method;
            ReturnType = typeInfoProvider.CheckNotNull(nameof(typeInfoProvider)).GetTypeInfo(method?.ReturnType, false, false);
        }

        public MethodInfo(string name, Type declaringType, IEnumerable<Type>? parameterTypes = null, Type? returnType = null)
            : this(name, declaringType, null, parameterTypes, returnType, new TypeInfoProvider())
        {
        }

        public MethodInfo(string name, Type declaringType, IEnumerable<Type> genericArguments, IEnumerable<Type> parameterTypes, Type? returnType = null)
            : this(name, declaringType, genericArguments, parameterTypes, returnType, new TypeInfoProvider())
        {
        }

        public MethodInfo(string name, Type declaringType, IEnumerable<Type>? genericArguments, IEnumerable<Type>? parameterTypes, Type? returnType, TypeInfoProvider typeInfoProvider)
            : base(name, declaringType, genericArguments, parameterTypes, typeInfoProvider)
        {
            ReturnType = typeInfoProvider.CheckNotNull(nameof(typeInfoProvider)).GetTypeInfo(returnType, false, false);
        }

        public MethodInfo(string name, TypeInfo declaringType, IEnumerable<TypeInfo>? genericArguments = null, IEnumerable<TypeInfo>? parameterTypes = null, TypeInfo? returnType = null)
            : base(name, declaringType, genericArguments, parameterTypes)
        {
            ReturnType = returnType;
        }

        protected MethodInfo(MethodInfo method)
            : base(method, new TypeInfoProvider())
        {
        }

        public override MemberTypes MemberType => MemberTypes.Method;

        [DataMember(Order = 7, IsRequired = false, EmitDefaultValue = false)]
        public TypeInfo? ReturnType { get; set; }

        [Obsolete("Use method ToMethodInfo() instead", true)]
        public System.Reflection.MethodInfo Method => ToMethodInfo();

        public override string ToString() => $"{ReturnType} {base.ToString()}".Trim();

        public static explicit operator System.Reflection.MethodInfo(MethodInfo method)
            => method.CheckNotNull(nameof(method)).ToMethodInfo();

        public System.Reflection.MethodInfo ToMethodInfo()
            => _method ?? (_method = this.ResolveMethod(TypeResolver.Instance))
            ?? throw new TypeResolverException($"Failed to resolve method, consider using extension method to specify {nameof(ITypeResolver)}.");
    }
}
