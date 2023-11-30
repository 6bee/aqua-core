// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.TypeSystem.Extensions;

using Aqua.TypeExtensions;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

[EditorBrowsable(EditorBrowsableState.Never)]
public static class MemberInfoExtensions
{
    private static readonly Dictionary<MemberInfo, BindingFlags> _bindingFlagsCache = [];

    [return: NotNullIfNotNull("member")]
    public static Aqua.TypeSystem.MemberTypes? GetMemberType(this MemberInfo? member) => (Aqua.TypeSystem.MemberTypes?)member?.MemberType;

    [return: NotNullIfNotNull("member")]
    internal static BindingFlags? GetBindingFlags(this MemberInfo? member)
    {
        if (member is null)
        {
            return null;
        }

        lock (_bindingFlagsCache)
        {
            if (!_bindingFlagsCache.TryGetValue(member, out var bindingFlags))
            {
                bindingFlags = (BindingFlags?)member.GetType()
                    .GetProperty("BindingFlags", ReflectionBinding.PrivateInstance)
                    ?.GetValue(member)
                    ?? BindingFlags.Default;
                _bindingFlagsCache.Add(member, bindingFlags);
            }

            return bindingFlags;
        }
    }
}
