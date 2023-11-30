// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Text.Json;

using System.Text.Json.Serialization;

internal sealed class AquaReferenceHandler : ReferenceHandler
{
    private readonly ReferenceResolver? _referenceResolver;

    private AquaReferenceHandler()
        => _referenceResolver = null;

    public AquaReferenceHandler(AquaReferenceHandler referenceHandler)
    {
        referenceHandler.AssertNotNull();
        _referenceResolver = referenceHandler.CreateResolver();
    }

    public override ReferenceResolver CreateResolver()
        => _referenceResolver ?? new AquaReferenceResolver();

    public static AquaReferenceHandler Root => new AquaReferenceHandler();

    /// <summary>
    /// Gets <see langword="true"/> if <see cref="CreateResolver"/> returns a new instance on every call,
    /// <see langword="false"/> if the same instance is served everytime.
    /// </summary>
#pragma warning disable SA1623 // Property summary documentation should match accessors
    public bool IsRoot => _referenceResolver is null;
#pragma warning restore SA1623 // Property summary documentation should match accessors
}
