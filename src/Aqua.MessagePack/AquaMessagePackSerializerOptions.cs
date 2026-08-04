// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.MessagePack;

public class AquaMessagePackSerializerOptions : MessagePackSerializerOptions
{
    private MessagePackSerializerContext? _context;

    public AquaMessagePackSerializerOptions(IFormatterResolver resolver)
        : base(resolver)
    {
    }

    public AquaMessagePackSerializerOptions(MessagePackSerializerOptions copyFrom)
        : base(copyFrom)
    {
    }

    public ReferenceHandler ReferenceHandler { get; private set; }

    public MessagePackSerializerContext Context => _context ??= new(ReferenceHandler);

    public AquaMessagePackSerializerOptions With(ReferenceHandler referenceHandler)
    {
        var copy = (AquaMessagePackSerializerOptions)Clone();
        copy.ReferenceHandler = referenceHandler;
        return copy;
    }

    public AquaMessagePackSerializerOptions WithPreserveReferences() => With(ReferenceHandler.Preserve);

    public AquaMessagePackSerializerOptions WithIgnoreCyclicReferences() => With(ReferenceHandler.IgnoreCycles);

    protected override MessagePackSerializerOptions Clone() => new AquaMessagePackSerializerOptions(this);
}
