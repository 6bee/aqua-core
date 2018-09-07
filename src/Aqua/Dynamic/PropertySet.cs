// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Dynamic
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Runtime.Serialization;

    [Serializable]
    [CollectionDataContract]
    public class PropertySet : IEnumerable<Property>
    {
        [Serializable]
        private sealed class PropertyComparer : IEqualityComparer<Property>
        {
            public static readonly PropertyComparer Instance = new PropertyComparer();

            public bool Equals(Property p1, Property p2)
            {
                if (p1 is null || p2 is null)
                {
                    return false;
                }

                if (p1.Name is null || p2.Name is null)
                {
                    return false;
                }

                return p1.Name.Equals(p2.Name);
            }

            public int GetHashCode(Property p) => p?.Name?.GetHashCode() ?? 0;
        }

        private readonly ISet<Property> _properties;

        public PropertySet()
        {
            _properties = new HashSet<Property>(PropertyComparer.Instance);
        }

        public PropertySet(IEnumerable<Property> properties)
        {
            _properties = new HashSet<Property>(properties, PropertyComparer.Instance);
        }

        public int Count => _properties.Count;

        public void Add(string name, object value) => _properties.Add(new Property(name, value));

        public void Add(Property property) => _properties.Add(property);

        public bool Remove(Property property) => _properties.Remove(property);

        public bool Contains(Property property) => _properties.Contains(property);

        public IEnumerator<Property> GetEnumerator() => _properties.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => _properties.GetEnumerator();
    }
}
