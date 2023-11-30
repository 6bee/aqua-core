// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.TypeExtensions;

using System.Reflection;

internal static class ReflectionBinding
{
    public const BindingFlags PublicStatic = BindingFlags.Public | BindingFlags.Static;
    public const BindingFlags PrivateStatic = BindingFlags.NonPublic | BindingFlags.Static;
    public const BindingFlags AnyStatic = PublicStatic | PrivateStatic;

    public const BindingFlags PublicInstance = BindingFlags.Public | BindingFlags.Instance;
    public const BindingFlags PrivateInstance = BindingFlags.NonPublic | BindingFlags.Instance;
    public const BindingFlags AnyInstance = PublicInstance | PrivateInstance;

    public const BindingFlags Any = AnyInstance | AnyStatic;
}
