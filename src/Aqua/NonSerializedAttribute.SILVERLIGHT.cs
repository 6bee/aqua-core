﻿// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

#if CORECLR || WINRT || SILVERLIGHT

namespace Aqua
{
    using System;

    /// <summary>
    /// Internal attribute as a NONFUNCTIONAL placeholder of it's .NET framework version
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, Inherited = false)]
    internal sealed class NonSerializedAttribute : Attribute
    {
    }
}

#endif