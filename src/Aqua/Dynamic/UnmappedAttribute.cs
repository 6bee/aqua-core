// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Dynamic
{
    using System;

    /// <summary>
    /// Prevents annotated members to be mapped into <see cref="DynamicObject"/>
    /// using <see cref="DynamicObjectMapper"/>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, Inherited = true)]
    public sealed class UnmappedAttribute : Attribute
    {
    }
}
