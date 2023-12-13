// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Dynamic;

using System;
using System.Runtime.Serialization;

#if !NET8_0_OR_GREATER
[Serializable]
#endif // NET8_0_OR_GREATER
public class DynamicObjectMapperException : Exception
{
    public DynamicObjectMapperException()
    {
    }

    public DynamicObjectMapperException(string message)
        : base(message)
    {
    }

    internal DynamicObjectMapperException(Exception innerException)
        : base(innerException.Message, innerException)
    {
    }

    public DynamicObjectMapperException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

#if !NET8_0_OR_GREATER
    protected DynamicObjectMapperException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }
#endif // NET8_0_OR_GREATER
}