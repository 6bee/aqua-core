// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Dynamic
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;

    [Serializable]
    [CollectionDataContract]
    public class Properties : List<Property>
    {
        public Properties()
        {
        }

        public Properties(IEnumerable<Property> properties)
            : base(properties)
        {
        }

        public void Add(string name, object value) => Add(new Property(name, value));
    }
}
