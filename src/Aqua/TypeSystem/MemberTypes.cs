// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.TypeSystem;

using System;

#if !NET8_0_OR_GREATER
[Serializable]
#endif // NET8_0_OR_GREATER
[Flags]
public enum MemberTypes
{
    Constructor = System.Reflection.MemberTypes.Constructor,
    Field = System.Reflection.MemberTypes.Field,
    Method = System.Reflection.MemberTypes.Method,
    Property = System.Reflection.MemberTypes.Property,
}