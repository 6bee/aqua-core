// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.TypeExtensions;

using System.ComponentModel;
using System.Reflection;

[EditorBrowsable(EditorBrowsableState.Never)]
public static class AssemblyExtensions
{
    /// <summary>
    /// Returns an <see cref="IEnumerable{T}"/> that contains types that were defined and loaded in the specified assembly.
    /// </summary>
    /// <remarks>
    /// This extension method exists due to <see cref="Assembly.GetTypes()"/> potential failure on types
    /// that have dependecies that cannot be resolved, causing the method to throw <see cref="ReflectionTypeLoadException"/>.
    /// </remarks>
    public static IEnumerable<Type> GetLoadedTypes(this Assembly assembly)
    {
        var m = assembly.GetModules(false);
        return m.Length switch
        {
            1 => GetTypes(m[0]),
            _ => m.SelectMany(GetTypes),
        };

        static IEnumerable<Type> GetTypes(Module module)
        {
            try
            {
                return module.GetTypes();
            }
            catch (ReflectionTypeLoadException ex)
            {
                return ex.Types.Where(x => x is not null)!;
            }
        }
    }
}