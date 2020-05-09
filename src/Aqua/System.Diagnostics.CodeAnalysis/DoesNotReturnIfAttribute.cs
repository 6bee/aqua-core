// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace System.Diagnostics.CodeAnalysis
{
    using System;

    /// <summary>
    /// Specifies that the method will not return if the associated Boolean parameter is passed the specified value.
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter, Inherited = false)]
    internal class DoesNotReturnIfAttribute : Attribute
    {
        /// <summary>Initializes a new instance of the <see cref="DoesNotReturnIfAttribute"/> class.</summary>
        /// <param name="parameterValue">
        /// The condition parameter value. Code after the method will be considered unreachable by diagnostics if the argument to
        /// the associated parameter matches this value.
        /// </param>
        public DoesNotReturnIfAttribute(bool parameterValue)
        {
            ParameterValue = parameterValue;
        }

        /// <summary>Gets the condition parameter value.</summary>
#pragma warning disable SA1623 // Property summary documentation should match accessors
        public bool ParameterValue { get; }
#pragma warning restore SA1623 // Property summary documentation should match accessors
    }
}
