// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace System.Diagnostics.CodeAnalysis
{
    using System;

    /// <summary>Specifies that an output may be null only if specified input parameter is null.</summary>
    [AttributeUsage(AttributeTargets.ReturnValue, Inherited = false)]
    internal sealed class NotNullIfNotNullAttribute : Attribute
    {
        public NotNullIfNotNullAttribute(string parameterName)
        {
            ParameterName = parameterName;
        }

        public string ParameterName { get; }
    }
}
