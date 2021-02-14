// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Dynamic
{
    using Aqua.EnumerableExtensions;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;

    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class DynamicObjectMapperExtensions
    {
        /// <summary>
        /// Maps a collection of <see cref="DynamicObject" />s into a collection of objects.
        /// </summary>
        /// <param name="objectMapper">The <see cref="IDynamicObjectMapper"/> instance used to map the <see cref="DynamicObject"/>s.</param>
        /// <param name="objects">Collection of <see cref="DynamicObject" /> to be mapped.</param>
        /// <param name="type">Target type for mapping, set this parameter to <see langword="null"/> if type information included within individual <see cref="DynamicObject" />s should be used.</param>
        /// <returns>Collection of objects created based on the <see cref="DynamicObject" />s specified.</returns>
        public static IEnumerable Map(this IDynamicObjectMapper objectMapper, IEnumerable<DynamicObject?> objects, Type? type = null)
        {
            if (objectMapper is null)
            {
                throw new ArgumentNullException(nameof(objectMapper));
            }

            if (objects is null)
            {
                throw new ArgumentNullException(nameof(objects));
            }

            IEnumerable<object?> source = objects.Select(x => objectMapper.Map(x, type));
            return type is null || type == typeof(object)
                ? source.ToArray()
                : source.CastCollectionToArrayOfType(type);
        }

#nullable disable
        /// <summary>
        /// Maps a collection of <see cref="DynamicObject" />s into a collection of <typeparamref name="T" />.
        /// </summary>
        /// <typeparam name="T">The target type in which the <see cref="DynamicObject" /> have to be mapped to.</typeparam>
        /// <param name="objectMapper">The <see cref="IDynamicObjectMapper"/> instance used to map the <see cref="DynamicObject"/>s.</param>
        /// <param name="objects">Collection of <see cref="DynamicObject" />s to be mapped.</param>
        /// <returns>Collection of <typeparamref name="T" /> created based on the <see cref="DynamicObject" />s specified.</returns>
        public static IEnumerable<T> Map<T>(this IDynamicObjectMapper objectMapper, IEnumerable<DynamicObject> objects)
            => (IEnumerable<T>)objectMapper.Map(objects, typeof(T));
#nullable restore
    }
}
