// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.TypeSystem
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;

    partial class TypeResolver
    {
        private static IEnumerable<Assembly> GetAssemblies()
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            return assemblies;
        }
    }
}
