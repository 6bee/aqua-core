// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

#if CORECLR

namespace Aqua.TypeSystem
{
    using Microsoft.Extensions.DependencyModel;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    partial class TypeResolver
    {
        private readonly Lazy<IEnumerable<Assembly>> _assemblies = new Lazy<IEnumerable<Assembly>>(() =>
            {
                // TODO: Verify PlatformServices.Default.LibraryManager.GetLibraries() works in core clr since it does not in dotnet!
                return PlatformServices.Default.LibraryManager.GetLibraries()
                    .SelectMany(_ => _.Assemblies)
                    .Select(_ =>
                    {
                        try
                        {
                            return Assembly.Load(_.Name);
                        }
                        catch
                        {
                            return null;
                        }
                    })
                    .Where(_ => _ != null)
                    .ToArray();
            });

        protected virtual IEnumerable<Assembly> GetAssemblies() => _assemblies.Value;
    }
}

#endif