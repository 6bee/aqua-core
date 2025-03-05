// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Dynamic;

using Aqua.EnumerableExtensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.Serialization;

/// <summary>
/// A set of properties where property names are considered as set keys.
/// However, since <see cref="Property" /> is not immutable a <see cref="PropertySet"/> may technically not be considered a set
/// and does not guarantee uniqueness of property names.
/// </summary>
[Serializable]
[CollectionDataContract]
[DebuggerDisplay("PropertySet {GetDebuggerDisplay(),nq}")]
public class PropertySet : IReadOnlyCollection<Property>
{
    [Serializable]
    private sealed class PropertyComparer : IEqualityComparer<Property>
    {
        private PropertyComparer()
        {
        }

        public static readonly PropertyComparer Instance = new();

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
        : this(properties?.ToList()!)
    {
    }

    public PropertySet(IEnumerable<KeyValuePair<string, object?>> properties)
        : this(properties?.Select(Property.From)!)
    {
    }

    public PropertySet(IEnumerable<(string Name, object? Value)> properties)
        : this(properties?.Select(static x => new Property(x.Name, x.Value))!)
    {
    }

    private PropertySet(List<Property> properties)
        => _list = properties.CheckNotNull();

    public virtual int Count => _list.Count;

    public virtual object? this[string name]
    {
        get
        {
            var existing = FindAll(name).FirstOrDefault()
                ?? throw new ArgumentException($"Property '{name}' is not found.", nameof(name));
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

    public virtual void Add(string name, object? value) => Add(new Property(name, value));

    public virtual void Add(Property property)
    {
        property.AssertNotNull();

        if (Contains(property))
        {
            throw new InvalidOperationException($"Property '{property.Name}' already contained.");
        }

        _list.Add(property);
    }

    public virtual bool Remove(Property property) => _list.RemoveAll(CreatePredicate(property)) > 0;

    public virtual bool Contains(Property property) => _list.FindIndex(CreatePredicate(property)) > -1;

    public virtual IEnumerator<Property> GetEnumerator() => _list.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_list).GetEnumerator();

    protected internal IEnumerable<Property> FindAll(string name) => FindAll(new Property(name, null));

    protected internal IEnumerable<Property> FindAll(Property property)
    {
        var match = CreatePredicate(property);
        return from item in _list
               where match(item)
               select item;
    }

    private string GetDebuggerDisplay() => $"[{Count}] {{ {_list.Select(x => x.Name).StringJoin(", ")} }}";

    private static Predicate<Property> CreatePredicate(Property property)
    {
        var comparer = PropertyComparer.Instance;
        var hashCode = comparer.GetHashCode(property);
        return item => comparer.GetHashCode(item) == hashCode && comparer.Equals(item, property);
    }

    public static implicit operator Dictionary<string, object?>(PropertySet propertySet)
        => propertySet.CheckNotNull().ToDictionary();

    public virtual Dictionary<string, object?> ToDictionary()
        => _list.ToDictionary(static x => x.Name ?? string.Empty, static x => x.Value);

    public static PropertySet From(Dictionary<string, object?> dictionary)
        => new(dictionary.CheckNotNull());
}