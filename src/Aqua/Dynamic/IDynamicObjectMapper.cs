// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Dynamic
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;

    public interface IDynamicObjectMapper
    {
        /// <summary>
        /// Maps a <see cref="DynamicObject"/> into a collection of objects.
        /// </summary>
        /// <param name="obj"><see cref="DynamicObject"/> to be mapped.</param>
        /// <param name="targetType">Target type for mapping, set this parameter to <see langword="null"/> if type information included within <see cref="DynamicObject"/> should be used.</param>
        /// <returns>The object created based on the <see cref="DynamicObject"/> specified.</returns>
        [return: NotNullIfNotNull("obj")]
        object? Map(DynamicObject? obj, Type? targetType = null);

        /// <summary>
        /// Maps a <see cref="DynamicObject"/> into an instance of <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The target type in which the <see cref="DynamicObject"/> have to be mapped to.</typeparam>
        /// <param name="obj"><see cref="DynamicObject"/> to be mapped.</param>
        /// <returns>The object created based on the <see cref="DynamicObject"/> specified.</returns>
        T Map<T>(DynamicObject? obj);

        /// <summary>
        /// Mapps the specified instance into a <see cref="DynamicObject"/>.
        /// </summary>
        /// <param name="obj">The instance to be mapped.</param>
        /// <param name="setTypeInformation">Type information is included within the <see cref="DynamicObject"/> if either lambda is <see langword="null"/> or returns <see langword="true"/>,
        /// no type information is set otherwise.</param>
        /// <returns>An instance of <see cref="DynamicObject"/> representing the mapped instance.</returns>
        [return: NotNullIfNotNull("obj")]
        DynamicObject? MapObject(object? obj, Func<Type, bool>? setTypeInformation = null);

        /// <summary>
        /// Maps a collection of objects into a collection of <see cref="DynamicObject"/>.
        /// </summary>
        /// <param name="objects">The objects to be mapped.</param>
        /// <param name="setTypeInformation">Type information is included within the <see cref="DynamicObject"/>s if either lambda is <see langword="null"/> or returns <see langword="true"/>,
        /// no type information is set otherwise.</param>
        /// <returns>A collection of <see cref="DynamicObject"/> representing the objects specified.</returns>
        [return: NotNullIfNotNull("objects")]
        IEnumerable<DynamicObject?>? MapCollection(object? objects, Func<Type, bool>? setTypeInformation = null);
    }
}
