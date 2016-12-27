﻿// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Dynamic
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime.Serialization;

    [Serializable]
    [DataContract]
    [KnownType(typeof(object))]
    [KnownType(typeof(object[]))]
    [DebuggerDisplay("{Name} = {Value}")]
    public class Property
    {
        public Property()
        {
        }

        public Property(string name, object value)
        {
            Name = name;
            Value = value;
        }

        public Property(KeyValuePair<string, object> property)
            : this(property.Key, property.Value)
        {
        }

        internal protected Property(Property property)
            : this(property.Name, property.Value)
        {
        }

        [DataMember(Order = 1)]
        public string Name { get; set; }

        [DataMember(Order = 2)]
        public object Value { get; set; }
    }
}