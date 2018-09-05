// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.TypeSystem
{
    using System;
    using System.ComponentModel;

    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class TypeInfoProviderExtensions
    {
        public static ConstructorInfo GetConstructorInfo(this ITypeInfoProvider typeInfoProvider, System.Reflection.ConstructorInfo constructorInfo)
            => constructorInfo is null ? null : new ConstructorInfo(constructorInfo, typeInfoProvider.AsNative());

        public static FieldInfo GetFieldInfo(this ITypeInfoProvider typeInfoProvider, System.Reflection.FieldInfo fieldInfo)
            => fieldInfo is null ? null : new FieldInfo(fieldInfo, typeInfoProvider.AsNative());

        public static MethodInfo GetMethodInfo(this ITypeInfoProvider typeInfoProvider, System.Reflection.MethodInfo methodInfo)
            => methodInfo is null ? null : new MethodInfo(methodInfo, typeInfoProvider.AsNative());

        public static MemberInfo GetMemberInfo(this ITypeInfoProvider typeInfoProvider, System.Reflection.MemberInfo memberInfo)
            => memberInfo is null ? null : MemberInfo.Create(memberInfo, typeInfoProvider.AsNative());

        public static PropertyInfo GetPropertyInfo(this ITypeInfoProvider typeInfoProvider, System.Reflection.PropertyInfo propertyInfo)
            => propertyInfo is null ? null : new PropertyInfo(propertyInfo, typeInfoProvider.AsNative());

        public static TypeInfo GetTypeInfo(this ITypeInfoProvider typeInfoProvider, Type type)
            => typeInfoProvider?.Get(type);

        private static TypeInfoProvider AsNative(this ITypeInfoProvider typeInfoProvider)
            => (typeInfoProvider as TypeInfoProvider) ?? new TypeInfoProvider(false, false, typeInfoProvider);
    }
}
