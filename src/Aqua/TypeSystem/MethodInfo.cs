// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.TypeSystem
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime.Serialization;
    using BindingFlags = System.Reflection.BindingFlags;

    [Serializable]
    [DataContract(Name = "Method", IsReference = true)]
    [DebuggerDisplay("Method: {Name,nq}")]
    public class MethodInfo : MethodBaseInfo
    {
        [NonSerialized]
        [Dynamic.Unmapped]
        private System.Reflection.MethodInfo _method;

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

        // TODO: replace binding flags by bool flags
        public MethodInfo(string name, Type declaringType, BindingFlags bindingFlags, Type[] genericArguments, Type[] parameterTypes, Type returnType)
            : this(name, declaringType, bindingFlags, genericArguments, parameterTypes, returnType, new TypeInfoProvider())
        {
        }

        internal MethodInfo(string name, Type declaringType, BindingFlags bindingFlags, Type[] genericArguments, Type[] parameterTypes, Type returnType, TypeInfoProvider typeInfoProvider)
            : base(name, declaringType, bindingFlags, genericArguments, parameterTypes, typeInfoProvider)
        {
            ReturnType = typeInfoProvider.Get(returnType, false, false);
        }

        public MethodInfo(string name, TypeInfo declaringType, BindingFlags bindingFlags, IEnumerable<TypeInfo> genericArguments, IEnumerable<TypeInfo> parameterTypes, TypeInfo returnType)
            : base(name, declaringType, bindingFlags, genericArguments, parameterTypes)
        {
            ReturnType = returnType;
        }

        protected MethodInfo(MethodInfo methodInfo)
            : base(methodInfo, new TypeInfoProvider())
        {
        }

        [DataMember(Order = 1, IsRequired = false, EmitDefaultValue = false)]
        public TypeInfo ReturnType { get; set; }

        public override MemberTypes MemberType => MemberTypes.Method;

        [Dynamic.Unmapped]
        public System.Reflection.MethodInfo Method
        {
            get
            {
                if (_method is null)
                {
                    _method = this.ResolveMethod(TypeResolver.Instance);
                }

                return _method;
            }
        }

        public override string ToString()
        {
            string returnType;
            try
            {
                returnType = new TypeInfo(Method.ReturnType, false, false).ToString();
            }
            catch
            {
                returnType = "'failed to resolve return type'";
            }

            return $"{returnType} {base.ToString()}";
        }

        public static explicit operator System.Reflection.MethodInfo(MethodInfo m)
            => m.Method;
    }
}
