// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;

[EditorBrowsable(EditorBrowsableState.Never)]
[SuppressMessage(
    "Major Bug",
    "S3903:Types should be defined in named namespaces",
    Justification = "Global extension method with internal visibility. Support friend assemblies in different namespances.")]
internal static class Check
{
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = false)]
    public sealed class ValidatedNotNullAttribute : Attribute
    {
    }

    public static T CheckNotNull<T>([ValidatedNotNull] this T value, string name) => value ?? throw new ArgumentNullException(name);
}