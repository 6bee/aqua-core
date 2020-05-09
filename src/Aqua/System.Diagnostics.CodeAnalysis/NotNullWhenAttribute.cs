// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace System.Diagnostics.CodeAnalysis
{
    using System;

    /// <summary>Specifies that when a method returns <see cref="P:System.Diagnostics.CodeAnalysis.NotNullWhenAttribute.ReturnValue" />, the parameter will not be null even if the corresponding type allows it.</summary>
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
    internal sealed class NotNullWhenAttribute : Attribute
    {
#pragma warning disable SA1642 // Constructor summary documentation should begin with standard text
        /// <summary>Initializes the attribute with the specified return value condition.</summary>
        /// <param name="returnValue">The return value condition. If the method returns this value, the associated parameter will not be null.</param>
        public NotNullWhenAttribute(bool returnValue)
#pragma warning restore SA1642 // Constructor summary documentation should begin with standard text
        {
            ReturnValue = returnValue;
        }

        /// <summary>Gets the return value condition.</summary>
#pragma warning disable SA1623 // Property summary documentation should match accessors
        public bool ReturnValue { get; }
#pragma warning restore SA1623 // Property summary documentation should match accessors
    }
}
