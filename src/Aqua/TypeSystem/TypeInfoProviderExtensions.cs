// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.TypeSystem
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics.CodeAnalysis;

    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class TypeInfoProviderExtensions
    {
        [return: NotNullIfNotNull("constructor")]
        public static ConstructorInfo? GetConstructorInfo(this ITypeInfoProvider typeInfoProvider, System.Reflection.ConstructorInfo? constructor)
            => constructor is null ? null : new ConstructorInfo(constructor, typeInfoProvider.AsNative());

        [return: NotNullIfNotNull("field")]
        public static FieldInfo? GetFieldInfo(this ITypeInfoProvider typeInfoProvider, System.Reflection.FieldInfo? field)
            => field is null ? null : new FieldInfo(field, typeInfoProvider.AsNative());

        [return: NotNullIfNotNull("method")]
        public static MethodInfo? GetMethodInfo(this ITypeInfoProvider typeInfoProvider, System.Reflection.MethodInfo? method)
            => method is null ? null : new MethodInfo(method, typeInfoProvider.AsNative());

        [return: NotNullIfNotNull("member")]
        public static MemberInfo? GetMemberInfo(this ITypeInfoProvider typeInfoProvider, System.Reflection.MemberInfo? member)
            => member is null ? null : MemberInfo.Create(member, typeInfoProvider.AsNative());

        [return: NotNullIfNotNull("property")]
        public static PropertyInfo? GetPropertyInfo(this ITypeInfoProvider typeInfoProvider, System.Reflection.PropertyInfo? property)
            => property is null ? null : new PropertyInfo(property, typeInfoProvider.AsNative());

        [return: NotNullIfNotNull("type")]
        public static TypeInfo? GetTypeInfo(this ITypeInfoProvider typeInfoProvider, Type type)
            => typeInfoProvider?.GetTypeInfo(type);

        private static TypeInfoProvider AsNative(this ITypeInfoProvider typeInfoProvider)
            => (typeInfoProvider as TypeInfoProvider) ?? new TypeInfoProvider(false, false, typeInfoProvider);
    }
}
