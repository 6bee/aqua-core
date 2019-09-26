// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.TypeSystem
{
    using Aqua.TypeSystem.Extensions;
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
            => typeInfo is null ? null : typeResolver.ResolveType(typeInfo);

        public static System.Reflection.MemberInfo ResolveMemberInfo(this MemberInfo memberInfo, ITypeResolver typeResolver)
        {
            if (memberInfo is null)
            {
                return null;
            }

            switch (memberInfo.MemberType)
            {
                case MemberTypes.Field:
                    return ((FieldInfo)memberInfo).ResolveField(typeResolver, MemberInfo.Scope.Any);

                case MemberTypes.Constructor:
                    return ((ConstructorInfo)memberInfo).ResolveConstructor(typeResolver);

                case MemberTypes.Property:
                    return ((PropertyInfo)memberInfo).ResolveProperty(typeResolver, MemberInfo.Scope.Any);

                case MemberTypes.Method:
                    return ((MethodInfo)memberInfo).ResolveMethod(typeResolver);

                default:
                    throw new ArgumentException($"Unknown member type: {memberInfo.MemberType}");
            }
        }

        public static System.Reflection.ConstructorInfo ResolveConstructor(this ConstructorInfo constructorInfo, ITypeResolver typeResolver)
        {
            if (constructorInfo is null)
            {
                return null;
            }

            var declaringType = constructorInfo.ResolveDeclaringType(typeResolver);

            var genericArguments = constructorInfo.GenericArgumentTypes is null ? Array.Empty<Type>() : constructorInfo.GenericArgumentTypes
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

            var parameterTypes = constructorInfo.ParameterTypes is null ? Array.Empty<Type>() : constructorInfo.ParameterTypes
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
            if (result is null)
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
            => fieldInfo?.ResolveDeclaringType(typeResolver).GetField(fieldInfo.Name);

        public static System.Reflection.FieldInfo ResolveField(this FieldInfo fieldInfo, ITypeResolver typeResolver, BindingFlags bindingAttr)
            => fieldInfo?.ResolveDeclaringType(typeResolver).GetField(fieldInfo.Name, bindingAttr);

        public static System.Reflection.MethodInfo ResolveMethod(this MethodInfo methodInfo, ITypeResolver typeResolver)
        {
            if (methodInfo is null)
            {
                return null;
            }

            var declaringType = methodInfo.ResolveDeclaringType(typeResolver);

            var genericArguments = methodInfo.GenericArgumentTypes is null ? Array.Empty<Type>() : methodInfo.GenericArgumentTypes
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

            var parameterTypes = methodInfo.ParameterTypes is null ? Array.Empty<Type>() : methodInfo.ParameterTypes
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

            int CountDeclarationDepth(System.Reflection.TypeInfo type, System.Reflection.TypeInfo methodDeclaringType, int i)
                => type == methodDeclaringType
                ? i
                : CountDeclarationDepth(type.BaseType.GetTypeInfo(), methodDeclaringType, i + 1);

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
                .OrderBy(m => CountDeclarationDepth(declaringType.GetTypeInfo(), m.DeclaringType.GetTypeInfo(), 0))
                .First();

            return result;
        }

        public static System.Reflection.PropertyInfo ResolveProperty(this PropertyInfo propertyInfo, ITypeResolver typeResolver)
            => propertyInfo?.ResolveDeclaringType(typeResolver).GetTypeInfo().GetDeclaredProperty(propertyInfo.Name);

        public static System.Reflection.PropertyInfo ResolveProperty(this PropertyInfo propertyInfo, ITypeResolver typeResolver, BindingFlags bindingAttr)
            => propertyInfo?.ResolveDeclaringType(typeResolver).GetProperty(propertyInfo.Name, bindingAttr);

        private static Type ResolveDeclaringType(this MemberInfo memberInfo, ITypeResolver typeResolver)
        {
            try
            {
                return typeResolver.ResolveType(memberInfo.DeclaringType);
            }
            catch (Exception ex)
            {
                throw new Exception($"Declaring type '{memberInfo.DeclaringType}' could not be reconstructed", ex);
            }
        }
    }
}
