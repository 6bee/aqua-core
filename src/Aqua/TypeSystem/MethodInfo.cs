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
    [DebuggerDisplay("{Name}")]
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
        }

        // TODO: replace binding flags by bool flags
        public MethodInfo(string name, Type declaringType, BindingFlags bindingFlags, Type[] genericArguments, Type[] parameterTypes)
            : base(name, declaringType, bindingFlags, genericArguments, parameterTypes, new TypeInfoProvider())
        {
        }

        public MethodInfo(string name, TypeInfo declaringType, BindingFlags bindingFlags, IEnumerable<TypeInfo> genericArguments, IEnumerable<TypeInfo> parameterTypes)
            : base(name, declaringType, bindingFlags, genericArguments, parameterTypes)
        {
        }

        protected MethodInfo(MethodInfo methodInfo)
            : base(methodInfo, new TypeInfoProvider())
        {
        }

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
