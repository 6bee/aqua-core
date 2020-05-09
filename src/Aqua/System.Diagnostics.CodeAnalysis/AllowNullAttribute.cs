// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace System.Diagnostics.CodeAnalysis
{
    using System;

    /// <summary>Specifies that null is allowed as an input even if the corresponding type disallows it.</summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, Inherited = false)]
    internal sealed class AllowNullAttribute : Attribute
    {
    }
}
