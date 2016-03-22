// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

#if CORECLR

namespace Aqua.TypeSystem.Emit
{
    using System;
    using System.Linq;
    using System.Reflection;
    using System.Reflection.Emit;

    internal static class TypeBuilderExtensions
    {
        //internal static ConstructorBuilder DefineConstructor(this TypeBuilder typeBuilder, MethodAttributes attributes, CallingConventions callingConvention, TypeInfo[] parameterTypes)
        //{
        //    return typeBuilder.DefineConstructor(attributes, callingConvention, parameterTypes.Select(_=>_.AsType()).ToArray());
        //}
    }
}

#endif