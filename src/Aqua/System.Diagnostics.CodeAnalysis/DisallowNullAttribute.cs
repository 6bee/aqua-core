// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace System.Diagnostics.CodeAnalysis
{
    using System;

    /// <summary>Specifies that null is not allowed even if the corresponding type allows it.</summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, Inherited = false)]
    internal sealed class DisallowNullAttribute : Attribute
    {
    }
}
