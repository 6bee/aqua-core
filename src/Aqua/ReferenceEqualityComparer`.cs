// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua
{
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;

    public sealed class ReferenceEqualityComparer<T> : IEqualityComparer<T>
    {
        private static ReferenceEqualityComparer<T> _default;

        public static ReferenceEqualityComparer<T> Default => _default ?? (_default = new ReferenceEqualityComparer<T>());

        public bool Equals(T x, T y) => ReferenceEquals(x, y);

        public int GetHashCode(T obj) => RuntimeHelpers.GetHashCode(obj);
    }
}
