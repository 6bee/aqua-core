// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Utils
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.CompilerServices;

    public sealed class ReferenceEqualityComparer<T> : IEqualityComparer<T>
    {
        [SuppressMessage("Design", "CA1000:Do not declare static members on generic types", Justification = "Default instance")]
        public static ReferenceEqualityComparer<T> Default { get; } = new ReferenceEqualityComparer<T>();

        public bool Equals(T? x, T? y) => ReferenceEquals(x, y);

        public int GetHashCode(T obj) => RuntimeHelpers.GetHashCode(obj);
    }
}
