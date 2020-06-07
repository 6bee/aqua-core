// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua
{
    using System;
    using System.ComponentModel;

    [EditorBrowsable(EditorBrowsableState.Never)]
    internal static class Check
    {
        [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = false)]
        public sealed class ValidatedNotNullAttribute : Attribute
        {
        }

        public static T CheckNotNull<T>([ValidatedNotNull] this T value, string name) => value ?? throw new ArgumentNullException(name);
    }
}
