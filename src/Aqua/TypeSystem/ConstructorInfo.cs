// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.TypeSystem
{
    using Extensions;
    using System;
    using System.Linq;
    using System.Runtime.Serialization;
    using BindingFlags = System.Reflection.BindingFlags;

    [Serializable]
    [DataContract(Name = "Constructor", IsReference = true)]
    public class ConstructorInfo : MethodBaseInfo
    {
        [NonSerialized]
        private System.Reflection.ConstructorInfo _constructor;

        public ConstructorInfo()
        {
        }

        public ConstructorInfo(System.Reflection.ConstructorInfo constructorInfo)
            : base(constructorInfo, TypeInfo.CreateReferenceTracker<Type>())
        {
            _constructor = constructorInfo;
        }

        // TODO: replace binding flags by bool flags
        public ConstructorInfo(string name, Type declaringType, BindingFlags bindingFlags, Type[] genericArguments, Type[] parameterTypes)
            : base(name, declaringType, bindingFlags, genericArguments, parameterTypes, TypeInfo.CreateReferenceTracker<Type>())
        {
        }

        protected ConstructorInfo(ConstructorInfo constructorInfo)
            : base(constructorInfo, TypeInfo.CreateReferenceTracker<TypeInfo>())
        {
        }

        public override MemberTypes MemberType { get { return TypeSystem.MemberTypes.Constructor; } }

        internal System.Reflection.ConstructorInfo Constructor
        {
            get
            {
                if (ReferenceEquals(null, _constructor))
                {
                    _constructor = ResolveConstructor(TypeResolver.Instance);
                }
                return _constructor;
            }
        }

        public System.Reflection.ConstructorInfo ResolveConstructor(ITypeResolver typeResolver)
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

            var constructorInfo = declaringType.GetConstructor(BindingFlags, null, parameterTypes, null);
            if (ReferenceEquals(null, constructorInfo))
            {
                constructorInfo = declaringType.GetConstructors(BindingFlags)
                    .Where(m => m.Name == Name)
                    .Where(m => !m.IsGenericMethod || m.GetGenericArguments().Length == genericArguments.Length)
                    .Where(m => m.GetParameters().Length == parameterTypes.Length)
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
            }

            return constructorInfo;
        }

        public override string ToString()
        {
            return $".ctor {base.ToString()}";
        }

        public static explicit operator System.Reflection.ConstructorInfo(ConstructorInfo c)
        {
            return c.Constructor;
        }
    }
}
