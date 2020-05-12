// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.TypeSystem
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics.CodeAnalysis;

    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class TypeInfoProviderExtensions
    {
        [return: NotNullIfNotNull("constructorInfo")]
        public static ConstructorInfo? GetConstructorInfo(this ITypeInfoProvider typeInfoProvider, System.Reflection.ConstructorInfo? constructorInfo)
            => constructorInfo is null ? null : new ConstructorInfo(constructorInfo, typeInfoProvider.AsNative());

        [return: NotNullIfNotNull("fieldInfo")]
        public static FieldInfo? GetFieldInfo(this ITypeInfoProvider typeInfoProvider, System.Reflection.FieldInfo? fieldInfo)
            => fieldInfo is null ? null : new FieldInfo(fieldInfo, typeInfoProvider.AsNative());

        [return: NotNullIfNotNull("methodInfo")]
        public static MethodInfo? GetMethodInfo(this ITypeInfoProvider typeInfoProvider, System.Reflection.MethodInfo? methodInfo)
            => methodInfo is null ? null : new MethodInfo(methodInfo, typeInfoProvider.AsNative());

        [return: NotNullIfNotNull("memberInfo")]
        public static MemberInfo? GetMemberInfo(this ITypeInfoProvider typeInfoProvider, System.Reflection.MemberInfo? memberInfo)
            => memberInfo is null ? null : MemberInfo.Create(memberInfo, typeInfoProvider.AsNative());

        [return: NotNullIfNotNull("propertyInfo")]
        public static PropertyInfo? GetPropertyInfo(this ITypeInfoProvider typeInfoProvider, System.Reflection.PropertyInfo? propertyInfo)
            => propertyInfo is null ? null : new PropertyInfo(propertyInfo, typeInfoProvider.AsNative());

        [return: NotNullIfNotNull("type")]
        public static TypeInfo? GetTypeInfo(this ITypeInfoProvider typeInfoProvider, Type type)
            => typeInfoProvider?.Get(type);

        private static TypeInfoProvider AsNative(this ITypeInfoProvider typeInfoProvider)
            => (typeInfoProvider as TypeInfoProvider) ?? new TypeInfoProvider(false, false, typeInfoProvider);
    }
}
