// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

using System;
using System.Diagnostics.CodeAnalysis;

[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = false)]
[SuppressMessage(
    "Major Bug",
    "S3903:Types should be defined in named namespaces",
    Justification = "Global extension method with internal visibility. Support friend assemblies in different namespances.")]
internal sealed class ValidatedNotNullAttribute : Attribute
{
}
