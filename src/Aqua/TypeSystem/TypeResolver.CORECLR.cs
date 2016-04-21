// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

#if CORECLR

namespace Aqua.TypeSystem
{
    using Microsoft.Extensions.PlatformAbstractions;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    partial class TypeResolver
    {
        private readonly Lazy<IEnumerable<Assembly>> _assemblies;

        private readonly Func<TypeInfo, Type> _typeEmitter;

        public TypeResolver(Func<IEnumerable<Library>> librariesProvider = null, Func<TypeInfo, Type> typeEmitter = null)
        {
            _typeEmitter = typeEmitter ?? new Emit.TypeEmitter().EmitType;

            _assemblies = new Lazy<IEnumerable<Assembly>>(() =>
                {
                    return (librariesProvider ?? DefaultLibrariesProvider)()
                        .SelectMany(_ => _.Assemblies)
                        .Select(_ =>
                        {
                            try
                            {
                                return Assembly.Load(_);
                            }
                            catch
                            {
                                return null;
                            }
                        })
                        .Where(_ => _ != null)
                        .ToArray();
                });
        }

        private static IEnumerable<Library> DefaultLibrariesProvider()
        {
            // TODO: Verify this works in core clr since it does not in dotnet!
            return PlatformServices.Default.LibraryManager.GetLibraries();
        }

        private IEnumerable<Assembly> GetAssemblies()
        {
            return _assemblies.Value;
        }
    }
}

#endif