// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.TypeSystem
{
    using Aqua.TypeSystem.Extensions;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Reflection;
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
            : base(methodInfo, TypeInfo.CreateReferenceTracker<Type>())
        {
            _method = methodInfo;
        }

        // TODO: replace binding flags by bool flags
        public MethodInfo(string name, Type declaringType, BindingFlags bindingFlags, Type[] genericArguments, Type[] parameterTypes)
            : base(name, declaringType, bindingFlags, genericArguments, parameterTypes, TypeInfo.CreateReferenceTracker<Type>())
        {
        }

        public MethodInfo(string name, TypeInfo declaringType, BindingFlags bindingFlags, IEnumerable<TypeInfo> genericArguments, IEnumerable<TypeInfo> parameterTypes)
            : base(name, declaringType, bindingFlags, genericArguments, parameterTypes)
        {
        }

        protected MethodInfo(MethodInfo methodInfo)
            : base(methodInfo, TypeInfo.CreateReferenceTracker<TypeInfo>())
        {
        }

        public override MemberTypes MemberType => MemberTypes.Method;

        [Dynamic.Unmapped]
        public System.Reflection.MethodInfo Method
        {
            get
            {
                if (ReferenceEquals(null, _method))
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
                returnType = new TypeInfo(Method.ReturnType, includePropertyInfos: false).ToString();
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
