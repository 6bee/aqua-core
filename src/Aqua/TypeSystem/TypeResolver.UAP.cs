// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

#if UAP

namespace Aqua.TypeSystem
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using Windows.Storage;
    using System.Reflection;
    using System.Threading.Tasks;

    partial class TypeResolver
    {
        private readonly Lazy<IEnumerable<Assembly>> _assemblies;

        public TypeResolver()
        {
            _assemblies = new Lazy<IEnumerable<Assembly>>(
                Task.Run<IEnumerable<Assembly>>(async () =>
                {
                    var assemblies = new List<Assembly>();
                    var folder = Windows.ApplicationModel.Package.Current.InstalledLocation;
                    foreach (var file in await folder.GetFilesAsync())
                    {
                        if (string.Equals(file.FileType, ".dll", StringComparison.OrdinalIgnoreCase) ||
                            string.Equals(file.FileType, ".exe", StringComparison.OrdinalIgnoreCase))
                        { 
                            try
                            {
                                assemblies.Add(Assembly.Load(new AssemblyName { Name = file.DisplayName }));
                            }
                            catch (Exception e)
                            {
                                Debug.WriteLine(e.Message);
                            }
                        }
                    }

                    return assemblies;
                }).Result,
                true);
        }

        /// <summary>
        /// Returns a list of assemblies to be scanned on resolving types. 
        /// It's strongly recommended to override this method in a derived class.
        /// </summary>
        protected virtual IEnumerable<Assembly> GetAssemblies() => _assemblies.Value;
    }
}

#endif