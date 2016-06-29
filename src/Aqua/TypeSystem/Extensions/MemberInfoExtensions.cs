// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.TypeSystem.Extensions
{
    using System;
    using System.ComponentModel;

    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class MemberInfoExtensions
    {
        public static MemberTypes GetMemberType(this System.Reflection.MemberInfo member)
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
