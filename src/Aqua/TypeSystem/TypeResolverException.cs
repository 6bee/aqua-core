// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.TypeSystem;

using System;

[Serializable]
public class TypeResolverException : Exception
{
    public TypeResolverException()
    {
    }

    public TypeResolverException(string message)
        : base(message)
    {
    }

    public TypeResolverException(string message, Exception? innerException)
        : base(message, innerException)
    {
    }

#if !NET8_0_OR_GREATER
    protected TypeResolverException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
        : base(info, context)
    {
    }
#endif // NET8_0_OR_GREATER
}