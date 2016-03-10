// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.TypeSystem
{
    using System.ComponentModel;

    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class MemberInfoExtensions
    {
        public static Aqua.TypeSystem.MemberTypes GetMemberType(this System.Reflection.MemberInfo member)
        {
            var t = (Aqua.TypeSystem.MemberTypes)member.MemberType;
            return t;
        }
    }
}