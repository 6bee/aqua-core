// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.TypeSystem;

[Serializable]
[Flags]
public enum MemberTypes
{
    Constructor = System.Reflection.MemberTypes.Constructor,
    Field = System.Reflection.MemberTypes.Field,
    Method = System.Reflection.MemberTypes.Method,
    Property = System.Reflection.MemberTypes.Property,
}