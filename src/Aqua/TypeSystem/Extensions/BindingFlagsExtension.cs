// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.TypeSystem.Extensions
{
    using System.ComponentModel;
    using System.Reflection;

    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class BindingFlagsExtension
    {
        public static bool Contains(this BindingFlags? flags, BindingFlags bindingFlags) => flags is not null && (flags & bindingFlags) == bindingFlags;

        public static bool MatchesExactly(this BindingFlags? flags, BindingFlags bindingFlags) => flags == bindingFlags;

        public static bool MatchesPartly(this BindingFlags? flags, BindingFlags bindingFlags) => flags is not null && (flags & bindingFlags) is not 0;
    }
}
