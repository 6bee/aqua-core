// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Dynamic
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
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

        protected DynamicObjectMapperException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
