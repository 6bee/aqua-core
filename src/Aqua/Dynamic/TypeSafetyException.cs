// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Dynamic;

/// <summary>
/// The exception that is thrown when an <see cref="ITypeSafetyChecker"/> rejects a type for instantiation
/// when mapping from <see cref="DynamicObject"/>, in reference to OWASP A8:2017-Insecure Deserialization.
/// </summary>
[Serializable]
public class TypeSafetyException : Exception
{
    public TypeSafetyException()
    {
    }

    public TypeSafetyException(string message)
        : base(message)
    {
    }

    public TypeSafetyException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

#if !NET8_0_OR_GREATER
    protected TypeSafetyException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
        : base(info, context)
    {
    }
#endif // NET8_0_OR_GREATER
}
