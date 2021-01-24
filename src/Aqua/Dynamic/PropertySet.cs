// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Dynamic
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Runtime.Serialization;

    /// <summary>
    /// A sorted collection of properties where property names are considered as set keys.
    /// However, since <see cref="Property" /> is not immutable a <see cref="PropertySet"/> may technically not be considered set
    /// and does not guarantee uniqueness of property names.
    /// </summary>
    [Serializable]
    [CollectionDataContract]
    public class PropertySet : IEnumerable<Property>
    {
        [Serializable]
        private sealed class PropertyComparer : IEqualityComparer<Property>
        {
            private PropertyComparer()
            {
            }

            public static readonly PropertyComparer Instance = new PropertyComparer();

            public bool Equals(Property? x, Property? y)
                => ReferenceEquals(x, y)
                || string.Equals(x?.Name, y?.Name, StringComparison.Ordinal);

            [SuppressMessage("Globalization", "CA1307:Specify StringComparison", Justification = "Globalization issue with hash codes?")]
            public int GetHashCode(Property obj) => obj?.Name?.GetHashCode() ?? 0;
        }

        private readonly List<Property> _list;

        public PropertySet()
            : this(new List<Property>())
        {
        }

        public PropertySet(IEnumerable<Property> properties)
            : this(properties?.ToList() !)
        {
        }

        public PropertySet(IEnumerable<KeyValuePair<string, object?>> properties)
            : this(properties?.Select(x => new Property(x)).ToList() !)
        {
        }

        private PropertySet(List<Property> properties)
        {
            _list = properties.CheckNotNull(nameof(properties));
        }

        public int Count => _list.Count;

        public object? this[string name]
        {
            get
            {
                var existing = FindAll(name).FirstOrDefault();
                if (existing is null)
                {
                    throw new ArgumentException($"Property '{name}' is not found.", nameof(name));
                }

                return existing.Value;
            }

            set
            {
                var existing = FindAll(name).ToList();
                if (existing.Count > 0)
                {
                    existing.ForEach(x => x.Value = value);
                }
                else
                {
                    Add(new Property(name, value));
                }
            }
        }

        public void Add(string name, object? value) => Add(new Property(name, value));

        public void Add(Property property) => _list.Add(property);

        public bool Remove(Property property) => _list.RemoveAll(CreatePredicate(property)) > 0;

        public bool Contains(Property property) => _list.FindIndex(CreatePredicate(property)) > -1;

        public IEnumerator<Property> GetEnumerator() => _list.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => _list.GetEnumerator();

        private IEnumerable<Property> FindAll(string name) => FindAll(new Property(name, null));

        private IEnumerable<Property> FindAll(Property property)
        {
            var match = CreatePredicate(property);
            var count = Count;
            for (int i = 0; i < count; i++)
            {
                i = _list.FindIndex(i, match);
                if (i < 0)
                {
                    yield break;
                }

                yield return _list.ElementAt(i);
            }
        }

        private static Predicate<Property> CreatePredicate(Property property)
        {
            var comparer = PropertyComparer.Instance;
            var hashCode = comparer.GetHashCode(property);
            return item => comparer.GetHashCode(item) == hashCode && comparer.Equals(item, property);
        }

        public static implicit operator Dictionary<string, object?>(PropertySet propertySet)
            => propertySet.CheckNotNull(nameof(propertySet)).ToDictionary();

        public static implicit operator PropertySet(Dictionary<string, object?> dictionary)
            => ToPropertySet(dictionary);

        public Dictionary<string, object?> ToDictionary()
            => _list.ToDictionary(x => x.Name ?? string.Empty, x => x.Value);

        public static PropertySet ToPropertySet(Dictionary<string, object?> dictionary)
            => new PropertySet(dictionary.CheckNotNull(nameof(dictionary)).Select(x => (Property)x));
    }
}
