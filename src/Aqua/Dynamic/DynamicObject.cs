// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Dynamic;

using Aqua.TypeSystem;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.Serialization;
using System.Xml.Serialization;

[Serializable]
[DataContract(IsReference = true)]
[DebuggerDisplay("{GetDebuggerDisplay(),nq}")]
[KnownType(typeof(DynamicObject)), XmlInclude(typeof(DynamicObject))]
[KnownType(typeof(DynamicObject[])), XmlInclude(typeof(DynamicObject[]))]
public partial class DynamicObject
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DynamicObject"/> class.
    /// </summary>
    public DynamicObject()
        : this(default(TypeInfo), default)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DynamicObject"/> class, setting the specified type.
    /// </summary>
    /// <param name="type">The type to be set.</param>
    /// <param name="propertySet">The property set representing this dynamic object's state.</param>
    public DynamicObject(Type? type, PropertySet? propertySet = null)
        : this(type is null ? null : new TypeInfo(type, false, false), propertySet)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DynamicObject"/> class,
    /// setting the specified <see cref="TypeInfo"/> and <see cref="PropertySet"/>.
    /// </summary>
    /// <param name="type">The type to be set.</param>
    /// <param name="propertySet">The property set representing this dynamic object's state.</param>
    public DynamicObject(TypeInfo? type, PropertySet? propertySet = null)
    {
        Type = type;
        Properties = propertySet ?? [];
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DynamicObject"/> class, setting the specified members.
    /// </summary>
    /// <param name="properties">Initial collection of properties and their values.</param>
    /// <exception cref="ArgumentNullException">The specified members collection is <see langword="null"/>.</exception>
    public DynamicObject(IEnumerable<(string Name, object? Value)>? properties)
        => Properties = properties is null
        ? null
        : new PropertySet(properties.CheckNotNull());

    /// <summary>
    /// Initializes a new instance of the <see cref="DynamicObject"/> class, setting the specified members.
    /// </summary>
    /// <param name="properties">Initial collection of properties and values.</param>
    /// <exception cref="ArgumentNullException">The specified members collection is <see langword="null"/>.</exception>
    public DynamicObject(IEnumerable<Property>? properties)
        => Properties = properties is null
        ? null
        : new PropertySet(properties.CheckNotNull());

    /// <summary>
    /// Initializes a new instance of the <see cref="DynamicObject"/> class with the given <see cref="PropertySet"/>.
    /// </summary>
    /// <param name="propertySet">Initial collection of properties and values.</param>
    /// <exception cref="ArgumentNullException">The specified members collection is <see langword="null"/>.</exception>
    public DynamicObject(PropertySet? propertySet)
        => Properties = propertySet;

    /// <summary>
    /// Initializes a new instance of the <see cref="DynamicObject"/> class,
    /// representing the object structure defined by the specified object.
    /// </summary>
    /// <param name="obj">The object to be represented by the new dynamic object.</param>
    /// <param name="type">Optional type information to be stored. If this argument is <see langword="null"/>, the value's type is stored instead.</param>
    /// <param name="mapper">Optional instance of dynamic object mapper.</param>
    /// <exception cref="ArgumentNullException">The specified object is <see langword="null"/>.</exception>
    public DynamicObject(object? obj, Type? type = null, IDynamicObjectMapper? mapper = null)
    {
        var dynamicObject = (mapper ?? new DynamicObjectMapper()).MapObject(obj);
        Type = type is null
            ? dynamicObject?.Type
            : new TypeInfo(type, false, false);
        Properties = dynamicObject?.Properties;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DynamicObject"/> class.
    /// </summary>
    /// <param name="dynamicObject">The instance to copy.</param>
    /// <param name="deepCopy">If <see langword="true"/> re-creates <see cref="Property"/> instances,
    /// otherwise fills existing <see cref="Property"/> instances into a new <see cref="PropertySet"/>.</param>
    /// <exception cref="ArgumentNullException">The specified members collection is <see langword="null"/>.</exception>
    public DynamicObject(DynamicObject dynamicObject, bool deepCopy = true)
    {
        var type = dynamicObject.CheckNotNull().Type;
        Type = type is null ? null : new TypeInfo(type);

        var properties = dynamicObject.Properties;
        Properties = properties is null
            ? null
            : deepCopy
                ? new PropertySet(properties.Select(x => new Property(x)))
                : new PropertySet(properties);
    }

    /// <summary>
    /// Gets or sets the type of object represented by this dynamic object instance.
    /// </summary>
    [DataMember(Order = 1)]
    public TypeInfo? Type { get; set; }

    /// <summary>
    /// Gets or sets the data members of this dynamic object instance.<br/>
    /// This property may be <see langword="null"/> for the dynamic object represent a <see langword="default"/> value.
    /// </summary>
    [DataMember(Order = 2)]
    public PropertySet? Properties { get; set; }

    /// <summary>
    /// Gets a value indicating whether target instance is <see langword="null"/>.
    /// </summary>
    /// <remarks>
    /// Returns <see langword="true"/> if <see cref="Properties"/> has not been set, <see langword="false"/> otherwise.
    /// </remarks>
    [XmlIgnore]
    public bool IsNull
    {
        get => Properties is null;
        init
        {
            if (value)
            {
                Properties = null;
            }
        }
    }

    /// <summary>
    /// Gets the count of members (dynamically added properties) hold by this dynamic object.
    /// </summary>
    public int PropertyCount => Properties?.Count ?? 0;

    /// <summary>
    /// Gets a collection of member names hold by this dynamic object.
    /// </summary>
    public IEnumerable<string> PropertyNames => Properties?.Select(x => x.Name ?? string.Empty).ToArray() ?? Enumerable.Empty<string>();

    /// <summary>
    /// Gets a collection of member values hold by this dynamic object.
    /// </summary>
    public IEnumerable<object?> Values => Properties?.Select(x => x.Value).ToArray() ?? Enumerable.Empty<object?>();

    /// <summary>
    /// Gets or sets a member value.
    /// </summary>
    /// <param name="name">Name of the member to set or get.</param>
    /// <returns>Value of the member specified.</returns>
    public object? this[string name]
    {
        get => TryGet(name, out var value)
            ? value
            : throw new ArgumentException($"Member not found for name '{name}'");
        set => Set(name, value);
    }

    /// <summary>
    /// Sets a member and it's value.
    /// </summary>
    /// <param name="name">Name of the member to be assigned.</param>
    /// <param name="value">The value to be set.</param>
    /// <returns>The property that was either added or updated.</returns>
    public Property Set(string name, object? value)
    {
        var properties = GetOrCreatePropertSet();
        var property = properties.SingleOrDefault(x => string.Equals(x.Name, name, StringComparison.Ordinal));

        if (property is null)
        {
            property = new Property(name, value);
            properties.Add(property);
        }
        else
        {
            property.Value = value;
        }

        return property;
    }

    /// <summary>
    /// Sets a member.
    /// </summary>
    /// <param name="property">Property to be set.</param>
    public void Set(Property property)
    {
        property.AssertNotNull();
        var properties = GetOrCreatePropertSet();

        if (properties.Any(x => string.Equals(x.Name, property.Name, StringComparison.Ordinal)))
        {
            properties.Remove(property);
        }

        properties.Add(property);
    }

    /// <summary>
    /// Gets a member's value or <see langword="null"/> if the specified member is unknown.
    /// </summary>
    /// <param name="name">Name of the member for the value to be returned.</param>
    /// <returns>The value assigned to the member specified, <see langword="null"/> if member is not set.</returns>
    public object? Get(string name = "")
        => TryGet(name, out var value)
            ? value
            : null;

    /// <summary>
    /// Gets a member's value or <c>default(T)</c> if the specified member is <see langword="null"/> or unknown.
    /// </summary>
    /// <returns>The value assigned to the member specified, <c>default(T)</c> if member is <see langword="null"/> or not set.</returns>
    public T? Get<T>(string name = "")
        => Get(name) is T t
            ? t
            : default;

    /// <summary>
    /// Adds a property and it's value.
    /// </summary>
    public void Add(string name, object? value) => GetOrCreatePropertSet().Add(name, value);

    /// <summary>
    /// Adds a property.
    /// </summary>
    public void Add(Property property) => GetOrCreatePropertSet().Add(property);

    /// <summary>
    /// Removes a member and it's value.
    /// </summary>
    /// <returns><see langword="true"/> if the member is successfully found and removed; otherwise, <see langword="false"/>.</returns>
    public bool Remove(string name)
    {
        var properties = Properties;
        if (TryGetProperty(properties, name, out var property))
        {
            properties.Remove(property);
            return true;
        }

        return false;
    }

    /// <summary>
    /// Gets the value assigned to the specified member.
    /// </summary>
    /// <param name="name">The name of the member.</param>
    /// <param name="value">When this method returns, contains the value assgned with the specified member,
    /// if the member is found; <see langword="null"/> if the member is not found.</param>
    /// <returns><see langword="true"/> is the dynamic object contains a member with the specified name; otherwise <see langword="false"/>.</returns>
    public bool TryGet(string name, out object? value)
    {
        if (TryGetProperty(Properties, name.CheckNotNull(), out var property))
        {
            value = property.Value;
            return true;
        }

        value = null;
        return false;
    }

    private static bool TryGetProperty([NotNullWhen(true)] PropertySet? properties, string name, [NotNullWhen(true)] out Property? property)
    {
        property = properties?.SingleOrDefault(x => string.Equals(x.Name, name, StringComparison.Ordinal));
        return property is not null;
    }

    private PropertySet GetOrCreatePropertSet()
        => Properties ??= [];

    /// <summary>
    /// Creates a dynamic objects representing the object structure defined by the specified object.
    /// </summary>
    /// <param name="obj">The object to be represented by the new dynamic object.</param>
    /// <param name="mapper">Optional instance of dynamic object mapper.</param>
    public static DynamicObject Create(object obj, IDynamicObjectMapper? mapper = null)
        => (mapper ?? new DynamicObjectMapper()).MapObject(obj);

    /// <summary>
    /// Creates a dynamic objects representing the type's default value.
    /// </summary>
    /// <param name="type">The type to be set on the dynamic object for the default value to be represented.</param>
    public static DynamicObject CreateDefault(Type? type)
        => new DynamicObject(type) { IsNull = true };

    /// <summary>
    /// Creates a dynamic objects representing the type's default value.
    /// </summary>
    /// <param name="type">The type to be set on the dynamic object for the default value to be represented.</param>
    public static DynamicObject CreateDefault(TypeInfo? type = null)
        => new DynamicObject(type) { IsNull = true };

    private string GetDebuggerDisplay()
    {
        if (IsNull)
        {
            return $"default({Type})";
        }

        var type = default(string);
        if (Type is TypeInfo t)
        {
            type = $"{t} ";
        }

        return $"{nameof(DynamicObject)}( {type}[Count = {PropertyCount}] )";
    }
}