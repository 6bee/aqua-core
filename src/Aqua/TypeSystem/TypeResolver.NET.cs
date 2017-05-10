// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

#if NET || NETSTANDARD

namespace Aqua.TypeSystem
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;

    partial class TypeResolver
    {
        private readonly Func<TypeInfo, Type> _typeEmitter;

        public TypeResolver(Func<TypeInfo, Type> typeEmitter = null, bool validateIncludingPropertyInfos = false)
        {
            _validateIncludingPropertyInfos = validateIncludingPropertyInfos;
            _typeEmitter = typeEmitter ?? new Emit.TypeEmitter().EmitType;
        }

#if NET
        protected virtual IEnumerable<Assembly> GetAssemblies() => AppDomain.CurrentDomain.GetAssemblies();
#endif
    }
}

#endif