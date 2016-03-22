// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

#if NET35 || SILVERLIGHT

namespace Aqua.TypeSystem.Emit
{
    using System;
    using System.Reflection.Emit;

    internal static class TypeBuilderExtensions
    {
        internal static Type CreateTypeInfo(this TypeBuilder typeBuilder)
        {
            return typeBuilder.CreateType();
        }
    }
}

#endif