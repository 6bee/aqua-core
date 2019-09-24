// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Dynamic
{
    using System;

    public static class DynamicObjectExtensions
    {
        /// <summary>
        /// Creates an instance of the object represented by the dynamic object specified.
        /// </summary>
        /// <param name="dynamicObject">The <see cref="DynamicObject"/> to be mapped.</param>
        /// <remarks>Requires the Type property to be set on this dynamic object.</remarks>
        /// <param name="mapper">Optional instance of dynamic object mapper.</param>
        public static object CreateObject(this DynamicObject dynamicObject, IDynamicObjectMapper mapper = null)
            => (mapper ?? new DynamicObjectMapper()).Map(dynamicObject);

        /// <summary>
        /// Creates an instance of the object type specified and populates the object structure represented by this dynamic object.
        /// </summary>
        /// <param name="dynamicObject">The <see cref="DynamicObject"/> to be mapped.</param>
        /// <param name="type">Type of object to be created.</param>
        /// <param name="mapper">Optional instance of dynamic object mapper.</param>
        public static object CreateObject(this DynamicObject dynamicObject, Type type, IDynamicObjectMapper mapper = null)
            => (mapper ?? new DynamicObjectMapper()).Map(dynamicObject, type);

        /// <summary>
        /// Creates an instance of the object type specified and populates the object structure represented by this dynamic object.
        /// </summary>
        /// <param name="dynamicObject">The <see cref="DynamicObject"/> to be mapped.</param>
        /// <typeparam name="T">Type of object to be created.</typeparam>
        /// <param name="mapper">Optional instance of dynamic object mapper.</param>
        public static T CreateObject<T>(this DynamicObject dynamicObject, IDynamicObjectMapper mapper = null)
            => (mapper ?? new DynamicObjectMapper()).Map<T>(dynamicObject);
    }
}
