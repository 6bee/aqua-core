// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.TypeSystem
{
    using Extensions;
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

        public override MemberTypes MemberType { get { return TypeSystem.MemberTypes.Method; } }

        public System.Reflection.MethodInfo Method
        {
            get
            {
                if (ReferenceEquals(null, _method))
                {
                    _method = ResolveMethod(TypeResolver.Instance);
                }

                return _method;
            }
        }

        public System.Reflection.MethodInfo ResolveMethod(ITypeResolver typeResolver)
        {
            Type declaringType;
            try
            {
                declaringType = typeResolver.ResolveType(DeclaringType);
            }
            catch (Exception ex)
            {
                throw new Exception($"Declaring type '{DeclaringType}' could not be reconstructed", ex);
            }

            var genericArguments = ReferenceEquals(null, GenericArgumentTypes) ? new Type[0] : GenericArgumentTypes
                .Select(typeInfo =>
                {
                    try
                    {
                        var genericArgumentType = typeResolver.ResolveType(typeInfo);
                        return genericArgumentType;
                    }
                    catch (Exception ex)
                    {
                        throw new Exception($"Generic argument type '{typeInfo}' could not be reconstructed", ex);
                    }
                })
                .ToArray();

            var parameterTypes = ReferenceEquals(null, ParameterTypes) ? new Type[0] : ParameterTypes
                .Select(typeInfo =>
                {
                    try
                    {
                        var parameterType = typeResolver.ResolveType(typeInfo);
                        return parameterType;
                    }
                    catch (Exception ex)
                    {
                        throw new Exception($"Parameter type '{typeInfo}' could not be reconstructed", ex);
                    }
                })
                .ToArray();


            var methodInfo = declaringType.GetMethods(BindingFlags)
                .Where(m => m.Name == Name)
                .Where(m => !m.IsGenericMethod || m.GetGenericArguments().Length == genericArguments.Length)
                .Where(m => m.GetParameters().Length == parameterTypes.Length)
                .Select(m => m.IsGenericMethod ? m.MakeGenericMethod(genericArguments) : m)
                .Where(m =>
                {
                    var paramTypes = m.GetParameters();
                    for (int i = 0; i < parameterTypes.Length; i++)
                    {
                        if (paramTypes[i].ParameterType != parameterTypes[i])
                        {
                            return false;
                        }
                    }

                    return true;
                })
                .Single();

            return methodInfo;
        }

        public override string ToString()
        {
            string returnType = null;
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
        {
            return m.Method;
        }
    }
}
