// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

#if NET35

namespace Aqua
{
    using System;

    /// <summary>
    /// Custom implementation of the corresponding .NET framework class
    /// </summary>
    [Serializable]
    internal class Tuple<T1, T2>
    {
        public Tuple(T1 item1, T2 item2)
        {
            Item1 = item1;
            Item2 = item2;
        }

        public T1 Item1 { get; }

        public T2 Item2 { get; }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj is Tuple<T1, T2> && Equals((Tuple<T1, T2>)obj);
        }

        private bool Equals(Tuple<T1, T2> other)
        {
            return Equals(Item1, other.Item1)
                && Equals(Item2, other.Item2);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((Item1?.GetHashCode() ?? 0) * 397) ^ (Item2?.GetHashCode() ?? 0);
            }
        }

        public static bool operator ==(Tuple<T1, T2> a, Tuple<T1, T2> b)
        {
            if (ReferenceEquals(null, a) && ReferenceEquals(null, b)) return true;
            if (ReferenceEquals(null, a)) return false;
            if (ReferenceEquals(null, b)) return false;
            return a.Equals(b);
        }

        public static bool operator !=(Tuple<T1, T2> a, Tuple<T1, T2> b)
        {
            return !(a == b);
        }
    }
}

#endif