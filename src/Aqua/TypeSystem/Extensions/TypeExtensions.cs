﻿// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.TypeSystem.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.CompilerServices;

    [EditorBrowsable(EditorBrowsableState.Never)]
    public static partial class TypeExtensions
    {
        public static bool IsClass(this Type type)
        {
            return type.GetTypeInfo().IsClass;
        }

        public static bool IsAnonymousType(this Type type)
        {
            return ((type.Name.StartsWith("<>") || type.Name.StartsWith("VB$"))
                && type.IsDefined<CompilerGeneratedAttribute>())
                || type.IsEmittedType();
        }

        public static bool IsEmittedType(this Type type)
        {
            return type.IsDefined<Emit.EmittedTypeAttribute>();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static bool IsDefined<T>(this Type type) where T : Attribute
        {
            return type
#if NETSTANDARD1_X
                .GetTypeInfo()
#endif
                .IsDefined(typeof(T));
        }

        internal static Type AsNonNullableType(this Type type)
        {
            var isNullable = type.IsGenericType() && type.GetGenericTypeDefinition() == typeof(Nullable<>);
            return isNullable ? type.GetGenericArguments()[0] : type;
        }

        public static bool Implements(this Type type, Type interfaceType)
        {
            return type.Implements(interfaceType, new Type[1][]);
        }

        public static bool Implements(this Type type, Type interfaceType, out Type[] genericTypeArguments)
        {
            var typeArgs = new Type[1][];
            var implements = type.Implements(interfaceType, typeArgs);
            genericTypeArguments = implements ? typeArgs[0] : null;
            return implements;
        }

        private static bool Implements(this Type type, Type interfaceType, Type[][] typeArgs)
        {
            var isAssignableFromSpecifiedInterface = interfaceType.IsGenericTypeDefinition()
                ? IsAssignableToGenericTypeDefinition(interfaceType, typeArgs)
                : interfaceType.IsGenericType()
                ? IsAssignableToGenericType(interfaceType, typeArgs)
                : interfaceType.IsAssignableFrom;

            return isAssignableFromSpecifiedInterface(type)
                || type.GetInterfaces().Any(isAssignableFromSpecifiedInterface);
        }

        private static Func<Type, bool> IsAssignableToGenericTypeDefinition(Type interfaceTypeInfo, Type[][] typeArgs)
        {
            var genericArgumentsCount = interfaceTypeInfo.GetGenericArguments().Length;

            return i =>
            {
                var genericArguments = i.GenericTypeArguments;
                var isAssignable = i.IsGenericType()
                    && genericArguments.Count() == genericArgumentsCount
                    && interfaceTypeInfo.MakeGenericType(genericArguments).IsAssignableFrom(i);
                if (isAssignable)
                {
                    typeArgs[0] = genericArguments;
                }

                return isAssignable;
            };
        }

        private static Func<Type, bool> IsAssignableToGenericType(Type interfaceTypeInfo, Type[][] typeArgs)
        {
            var interfaceTypeDefinition = interfaceTypeInfo.GetGenericTypeDefinition();
            var interfaceGenericArguments = interfaceTypeInfo.GetGenericArguments();

            return i =>
            {
                if (i.IsGenericType() && !i.IsGenericTypeDefinition())
                {
                    var typeDefinition = i.GetGenericTypeDefinition();
                    if (typeDefinition == interfaceTypeDefinition)
                    {
                        var genericArguments = i.GetGenericArguments();
                        var allArgumentsAreAssignable = Enumerable.Range(0, genericArguments.Length - 1)
                            .All(index => Implements(genericArguments[index], interfaceGenericArguments[index], typeArgs));
                        if (allArgumentsAreAssignable)
                        {
                            return true;
                        }
                    }
                }

                return false;
            };
        }
    }
}
