// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.TypeSystem
{
    using Extensions;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    public static class TypeHelper
    {
        public static Type GetElementType(Type type)
        {
            var enumerableType = FindIEnumerable(type);
            if (enumerableType == null)
            {
                return type;
            }

            return enumerableType.GetGenericArguments().First();
        }

        private static Type FindIEnumerable(Type type)
        {
            if (type == null || type == typeof(string))
            {
                return null;
            }

            if (type.IsArray)
            {
                return typeof(IEnumerable<>).MakeGenericType(type.GetElementType());
            }

            if (type.IsGenericType())
            {
                foreach (var arg in type.GetGenericArguments())
                {
                    var enumerableType = typeof(IEnumerable<>).MakeGenericType(arg);
                    if (enumerableType.IsAssignableFrom(type))
                    {
                        return enumerableType;
                    }
                }
            }

            var interfaces = type.GetInterfaces();
            if (interfaces != null && interfaces.Any())
            {
                foreach (var interfaceType in interfaces)
                {
                    var enumerableType = FindIEnumerable(interfaceType);
                    if (enumerableType != null)
                    {
                        return enumerableType;
                    }
                }
            }

            var baseType = type.GetBaseType();
            if (baseType != null && baseType != typeof(object))
            {
                return FindIEnumerable(baseType);
            }

            return null;
        }

        public static Type ResolveType(this TypeInfo typeInfo, ITypeResolver typeResolver)
        {
            if (ReferenceEquals(null, typeInfo))
            {
                return null;
            }

            var result = typeResolver.ResolveType(typeInfo);
            return result;
        }

        public static System.Reflection.MemberInfo ResolveMemberInfo(this MemberInfo memberInfo, ITypeResolver typeResolver)
        {
            if (ReferenceEquals(null, memberInfo))
            {
                return null;
            }

            switch (memberInfo.MemberType)
            {
                case MemberTypes.Field:
                    return ((FieldInfo)memberInfo).ResolveField(typeResolver);

                case MemberTypes.Constructor:
                    return ((ConstructorInfo)memberInfo).ResolveConstructor(typeResolver);

                case MemberTypes.Property:
                    return ((PropertyInfo)memberInfo).ResolveProperty(typeResolver);

                case MemberTypes.Method:
                    return ((MethodInfo)memberInfo).ResolveMethod(typeResolver);

                default:
                    throw new NotImplementedException($"Implementation missing for conversion of member type: {memberInfo.MemberType}");
            }
        }

        public static System.Reflection.ConstructorInfo ResolveConstructor(this ConstructorInfo constructorInfo, ITypeResolver typeResolver)
        {
            if (ReferenceEquals(null, constructorInfo))
            {
                return null;
            }

            Type declaringType;
            try
            {
                declaringType = typeResolver.ResolveType(constructorInfo.DeclaringType);
            }
            catch (Exception ex)
            {
                throw new Exception($"Declaring type '{constructorInfo.DeclaringType}' could not be reconstructed", ex);
            }

            var genericArguments = ReferenceEquals(null, constructorInfo.GenericArgumentTypes) ? new Type[0] : constructorInfo.GenericArgumentTypes
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

            var parameterTypes = ReferenceEquals(null, constructorInfo.ParameterTypes) ? new Type[0] : constructorInfo.ParameterTypes
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

            var result = declaringType.GetConstructor(constructorInfo.BindingFlags, null, parameterTypes, null);
            if (ReferenceEquals(null, result))
            {
                result = declaringType.GetConstructors(constructorInfo.BindingFlags)
                    .Where(m => m.Name == constructorInfo.Name)
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

            return result;
        }

        public static System.Reflection.FieldInfo ResolveField(this FieldInfo fieldInfo, ITypeResolver typeResolver)
        {
            if (ReferenceEquals(null, fieldInfo))
            {
                return null;
            }

            Type declaringType;
            try
            {
                declaringType = typeResolver.ResolveType(fieldInfo.DeclaringType);
            }
            catch (Exception ex)
            {
                throw new Exception($"Declaring type '{fieldInfo.DeclaringType}' could not be reconstructed", ex);
            }

            var result = declaringType.GetField(fieldInfo.Name);
            return result;
        }

        public static System.Reflection.MethodInfo ResolveMethod(this MethodInfo methodInfo, ITypeResolver typeResolver)
        {
            if (ReferenceEquals(null, methodInfo))
            {
                return null;
            }

            Type declaringType;
            try
            {
                declaringType = typeResolver.ResolveType(methodInfo.DeclaringType);
            }
            catch (Exception ex)
            {
                throw new Exception($"Declaring type '{methodInfo.DeclaringType}' could not be reconstructed", ex);
            }

            var genericArguments = ReferenceEquals(null, methodInfo.GenericArgumentTypes) ? new Type[0] : methodInfo.GenericArgumentTypes
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

            var parameterTypes = ReferenceEquals(null, methodInfo.ParameterTypes) ? new Type[0] : methodInfo.ParameterTypes
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


            var result = declaringType.GetMethods(methodInfo.BindingFlags)
                .Where(m => m.Name == methodInfo.Name)
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

            return result;
        }

        public static System.Reflection.PropertyInfo ResolveProperty(this PropertyInfo propertyInfo, ITypeResolver typeResolver)
        {
            if (ReferenceEquals(null, propertyInfo))
            {
                return null;
            }

            Type declaringType;
            try
            {
                declaringType = typeResolver.ResolveType(propertyInfo.DeclaringType);
            }
            catch (Exception ex)
            {
                throw new Exception($"Declaring type '{propertyInfo.DeclaringType}' could not be reconstructed", ex);
            }

            var result = declaringType.GetProperty(propertyInfo.Name);
            return result;
        }
    }
}
