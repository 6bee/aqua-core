// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.TypeSystem
{
    using System;
    using System.Runtime.Serialization;

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

        protected TypeResolverException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
