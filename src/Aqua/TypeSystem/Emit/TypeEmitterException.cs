// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.TypeSystem.Emit;

using System;
using System.Runtime.Serialization;

#if !NET8_0_OR_GREATER
[Serializable]
#endif // NET8_0_OR_GREATER
public class TypeEmitterException : Exception
{
    public TypeEmitterException()
    {
    }

    public TypeEmitterException(string message)
        : base(message)
    {
    }

    public TypeEmitterException(string message, Exception? innerException)
        : base(message, innerException)
    {
    }

#if !NET8_0_OR_GREATER
    protected TypeEmitterException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }
#endif // NET8_0_OR_GREATER
}