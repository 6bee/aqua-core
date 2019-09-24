// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

#if !NETSTANDARD1_X

namespace Aqua.TypeSystem
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;

    partial class TypeResolver
    {
        protected virtual IEnumerable<Assembly> GetAssemblies() => AppDomain.CurrentDomain.GetAssemblies();
    }
}

#endif