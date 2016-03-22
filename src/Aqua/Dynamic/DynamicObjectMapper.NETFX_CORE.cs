// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

#if NETFX_CORE || CORECLR || SILVERLIGHT

namespace Aqua.Dynamic
{
    using System;

    partial class DynamicObjectMapper
    {
        /// <summary>
        /// Not supported for this platform (WinRT, WP, SL)
        /// </summary>
        private static object GetUninitializedObject(Type type)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Not supported for this platform (WinRT, WP, SL)
        /// </summary>
        private static void PopulateObjectMembers(Type type, DynamicObject from, object to)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Not supported for this platform (WinRT, WP, SL)
        /// </summary>
        private static void MapObjectMembers(object from, DynamicObject to, Func<Type, bool> setTypeInformation)
        {
            throw new NotSupportedException();
        }
    }
}

#endif