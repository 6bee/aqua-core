// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Text.Json;

using System.ComponentModel;
using System.Text.Json.Serialization;

internal sealed class AquaReferenceHandler : ReferenceHandler
{
    private readonly ReferenceResolver? _referenceResolver;

    private AquaReferenceHandler()
        => _referenceResolver = null;

    internal AquaReferenceHandler(AquaReferenceHandler referenceHandler)
    {
        referenceHandler.AssertNotNull();
        _referenceResolver = referenceHandler.CreateResolver();
    }

    // NOTE: in contrast to ReferenceHandler.Preserve we allow to grab ReferenceResolver anytime
    //       to enable custom json converters take part in the reference handling game
    public override ReferenceResolver CreateResolver()
        => _referenceResolver ?? new AquaReferenceResolver();

    /// <summary>
    /// Gets a value indicating whether the current instance is a root reference handler.
    /// </summary>
    /// <remarks>
    /// Returns <see langword="true"/> if <see cref="CreateResolver"/> returns a new instance on every call,
    /// <see langword="false"/> if the same instance is served everytime.
    /// </remarks>
    internal bool IsRoot => _referenceResolver is null;

    internal static AquaReferenceHandler Root => new();
}