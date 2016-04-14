// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

#if WINRT || CORECLR

namespace Aqua.TypeSystem
{
    using System;

    internal static class MemberInfoExtensions
    {
        internal static MemberTypes GetMemberType(this System.Reflection.MemberInfo member)
        {
            if (member is System.Reflection.FieldInfo)
            {
                return MemberTypes.Field;
            }

            if (member is System.Reflection.ConstructorInfo)
            {
                return MemberTypes.Constructor;
            }

            if (member is System.Reflection.MethodInfo)
            {
                return MemberTypes.Method;
            }

            if (member is System.Reflection.PropertyInfo)
            {
                return MemberTypes.Property;
            }

            throw new Exception($"Unsupported member type: {member.GetType()}");
        }
    }
}

#endif