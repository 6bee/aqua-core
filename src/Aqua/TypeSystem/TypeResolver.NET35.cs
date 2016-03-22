// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

#if NET35

namespace Aqua.TypeSystem
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;

    partial class TypeResolver
    {
        private readonly Func<TypeInfo, Type> _typeEmitter;

        public TypeResolver(Func<TypeInfo, Type> typeEmitter = null)
        {
            _typeEmitter = typeEmitter ?? new Emit.TypeEmitter().EmitType;
        }

        private static IEnumerable<Assembly> GetAssemblies()
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            return assemblies;
        }
    }
}

#endif