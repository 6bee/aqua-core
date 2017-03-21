// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

#if NETSTANDARD1_3

namespace Aqua.TypeSystem
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Reflection;

    partial class TypeResolver
    {
        private readonly Func<TypeInfo, Type> _typeEmitter;

        private readonly Lazy<IEnumerable<Assembly>> _assemblies;

        public TypeResolver(Func<TypeInfo, Type> typeEmitter = null, bool validateIncludingPropertyInfos = false)
        {
            _validateIncludingPropertyInfos = validateIncludingPropertyInfos;

            _typeEmitter = typeEmitter ?? new Emit.TypeEmitter().EmitType;

            _assemblies = new Lazy<IEnumerable<Assembly>>(() =>
                {
                    var assemblies = new List<Assembly>();
                    foreach (var file in Directory.EnumerateFiles(AppContext.BaseDirectory).Select(x => new FileInfo(x)))
                    {
                        if (string.Equals(file.Extension, ".dll", StringComparison.OrdinalIgnoreCase) ||
                            string.Equals(file.Extension, ".exe", StringComparison.OrdinalIgnoreCase))
                        {
                            try
                            {
                                assemblies.Add(Assembly.Load(new AssemblyName { Name = Path.GetFileNameWithoutExtension(file.Name) }));
                            }
                            catch (Exception ex)
                            {
                                Debug.WriteLine(ex.Message);
                            }
                        }
                    }

                    return assemblies;
                },
                true);
        }

        /// <summary>
        /// Returns a list of assemblies to be scanned on resolving types. 
        /// It's recommended to override this method in a derived class.
        /// </summary>
        protected virtual IEnumerable<Assembly> GetAssemblies() => _assemblies.Value;
    }
}

#endif