// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

[EditorBrowsable(EditorBrowsableState.Never)]
[SuppressMessage(
    "Major Bug",
    "S3903:Types should be defined in named namespaces",
    Justification = "Global extension method with internal visibility. Support friend assemblies in different namespances.")]
internal static class Check
{
    [DebuggerStepThrough]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T CheckNotNull<T>([ValidatedNotNull] this T value, string name) => value ?? throw new ArgumentNullException(name);
}