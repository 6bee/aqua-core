// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua
{
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;

    public sealed class ObjectReferenceEqualityComparer<T> : IEqualityComparer<T>
    {
        public static readonly ObjectReferenceEqualityComparer<T> Instance = new ObjectReferenceEqualityComparer<T>();

        private ObjectReferenceEqualityComparer()
        {
        }

        public bool Equals(T x, T y)
        {
            return ReferenceEquals(x, y);
        }

        public int GetHashCode(T obj)
        {
            return RuntimeHelpers.GetHashCode(obj);
        }
    }
}
