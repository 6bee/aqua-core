// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Dynamic
{
    using Aqua.TypeSystem;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Linq;
    using System.Runtime.Serialization;

    [Serializable]
    [DataContract(IsReference = true)]
    [DebuggerDisplay("Count = {PropertyCount}")]
    public partial class DynamicObject : INotifyPropertyChanging, INotifyPropertyChanged
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DynamicObject"/> class.
        /// </summary>
        public DynamicObject()
            : this(default(TypeInfo), default(PropertySet))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DynamicObject"/> class, setting the specified type.
        /// </summary>
        /// <param name="type">The type to be set.</param>
        public DynamicObject(Type type)
            : this(type is null ? null : new TypeInfo(type, false, false), default(PropertySet))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DynamicObject"/> class, setting the specified type.
        /// </summary>
        /// <param name="type">The type to be set.</param>
        public DynamicObject(TypeInfo type)
            : this(type, default(PropertySet))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DynamicObject"/> class,
        /// setting the specified <see cref="TypeInfo"/> and <see cref="PropertySet"/>.
        /// </summary>
        /// <param name="type">The type to be set.</param>
        /// <param name="propertySet">The property set.</param>
        public DynamicObject(TypeInfo type, PropertySet propertySet)
        {
            Type = type;
            Properties = propertySet ?? new PropertySet();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DynamicObject"/> class, setting the specified members.
        /// </summary>
        /// <param name="properties">Initial collection of properties and values.</param>
        /// <exception cref="ArgumentNullException">The specified members collection is null.</exception>
        public DynamicObject(IEnumerable<KeyValuePair<string, object>> properties)
        {
            if (properties is null)
            {
                throw new ArgumentNullException(nameof(properties));
            }

            Properties = new PropertySet(properties.Select(x => new Property(x)));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DynamicObject"/> class, setting the specified members.
        /// </summary>
        /// <param name="properties">Initial collection of properties and values.</param>
        /// <exception cref="ArgumentNullException">The specified members collection is null.</exception>
        public DynamicObject(IEnumerable<Property> properties)
        {
            if (properties is null)
            {
                throw new ArgumentNullException(nameof(properties));
            }

            Properties = new PropertySet(properties);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DynamicObject"/> class,
        /// representing the object structure defined by the specified object.
        /// </summary>
        /// <param name="obj">The object to be represented by the new dynamic object.</param>
        /// <param name="mapper">Optional instance of dynamic object mapper.</param>
        /// <exception cref="ArgumentNullException">The specified object is null.</exception>
        public DynamicObject(object obj, IDynamicObjectMapper mapper = null)
        {
            if (obj is null)
            {
                throw new ArgumentNullException(nameof(obj));
            }

            var dynamicObject = (mapper ?? new DynamicObjectMapper()).MapObject(obj);
            Type = dynamicObject.Type;
            Properties = dynamicObject.Properties;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DynamicObject"/> class.
        /// </summary>
        /// <param name="dynamicObject">The instance to copy.</param>
        /// <param name="deepCopy">If true re-creates <see cref="Property"/> instances, otherwise fills existing <see cref="Property"/> instances into a new <see cref="PropertySet"/>.</param>
        /// <exception cref="ArgumentNullException">The specified members collection is null.</exception>
        internal protected DynamicObject(DynamicObject dynamicObject, bool deepCopy = true)
        {
            if (dynamicObject is null)
            {
                throw new ArgumentNullException(nameof(dynamicObject));
            }

            Type = dynamicObject.Type is null ? null : new TypeInfo(dynamicObject.Type);

            if (dynamicObject.Properties is null)
            {
                Properties = null;
            }
            else if (deepCopy)
            {
                Properties = new PropertySet(dynamicObject.Properties.Select(x => new Property(x)));
            }
            else
            {
                Properties = new PropertySet(dynamicObject.Properties);
            }
        }

        public event PropertyChangingEventHandler PropertyChanging;

        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Gets or sets the type of object represented by this dynamic object instance.
        /// </summary>
        [DataMember(Order = 1)]
        public TypeInfo Type { get; set; }

        [DataMember(Order = 2)]
        public PropertySet Properties { get; set; }

        /// <summary>
        /// Gets the count of members (dynamically added properties) hold by this dynamic object.
        /// </summary>
        public int PropertyCount => Properties.Count;

        /// <summary>
        /// Gets a collection of member names hold by this dynamic object.
        /// </summary>
        public IEnumerable<string> PropertyNames => Properties.Select(x => x.Name).ToList();

        /// <summary>
        /// Gets a collection of member values hold by this dynamic object.
        /// </summary>
        public IEnumerable<object> Values => Properties.Select(x => x.Value).ToList();

        /// <summary>
        /// Gets or sets a member value.
        /// </summary>
        /// <param name="name">Name of the member to set or get.</param>
        /// <returns>Value of the member specified.</returns>
        public object this[string name]
        {
            get => TryGet(name, out object value)
                ? value
                : throw new Exception($"Member not found for name '{name}'");
            set => Set(name, value);
        }

        /// <summary>
        /// Sets a member and it's value.
        /// </summary>
        /// <param name="name">Name of the member to be assigned.</param>
        /// <param name="value">The value to be set.</param>
        /// <returns>The value specified.</returns>
        public object Set(string name, object value)
        {
            var property = Properties.SingleOrDefault(x => string.Equals(x.Name, name));

            var oldValue = property?.Value;
            OnPropertyChanging(name, oldValue, value);

            if (property is null)
            {
                property = new Property(name, value);
                Properties.Add(property);
            }
            else
            {
                property.Value = value;
            }

            OnPropertyChanged(name, oldValue, value);

            return value;
        }

        /// <summary>
        /// Gets a member's value or null if the specified member is unknown.
        /// </summary>
        /// <returns>The value assigned to the member specified, null if member is not set.</returns>
        public object Get(string name = "")
            => TryGet(name, out object value)
                ? value
                : null;

        /// <summary>
        /// Gets a member's value or default(T) if the specified member is null or unknown.
        /// </summary>
        /// <returns>The value assigned to the member specified, default(T) if member is null or not set.</returns>
        public T Get<T>(string name = "")
        {
            var value = Get(name);
            return value is T ? (T)value : default;
        }

        /// <summary>
        /// Adds a property and it's value.
        /// </summary>
        public void Add(string name, object value) => Properties.Add(name, value);

        /// <summary>
        /// Adds a property.
        /// </summary>
        public void Add(Property property) => Properties.Add(property);

        /// <summary>
        /// Removes a member and it's value.
        /// </summary>
        /// <returns>True if the member is successfully found and removed; otherwise, false.</returns>
        public bool Remove(string name)
        {
            var property = Properties.SingleOrDefault(x => string.Equals(x.Name, name));

            if (property is null)
            {
                return false;
            }

            Properties.Remove(property);
            return true;
        }

        /// <summary>
        /// Gets the value assigned to the specified member.
        /// </summary>
        /// <param name="name">The name of the member.</param>
        /// <param name="value">When this method returns, contains the value assgned with the specified member,
        /// if the member is found; null if the member is not found.</param>
        /// <returns>True is the dynamic object contains a member with the specified name; otherwise false.</returns>
        public bool TryGet(string name, out object value)
        {
            var property = Properties.SingleOrDefault(x => string.Equals(x.Name, name));

            if (property is null)
            {
                value = null;
                return false;
            }

            value = property.Value;
            return true;
        }

        protected virtual void OnPropertyChanging(string name, object oldValue, object newValue)
            => PropertyChanging?.Invoke(this, new PropertyChangingEventArgs(name));

        protected virtual void OnPropertyChanged(string name, object oldValue, object newValue)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        [Obsolete("Method renamed to Create. This method is beeing removed in a future version.", true)]
        public static DynamicObject CreateDynamicObject(object obj, IDynamicObjectMapper mapper = null)
            => Create(obj, mapper);

        /// <summary>
        /// Creates a dynamic objects representing the object structure defined by the specified object.
        /// </summary>
        /// <param name="obj">The object to be represented by the new dynamic object.</param>
        /// <param name="mapper">Optional instance of dynamic object mapper.</param>
        public static DynamicObject Create(object obj, IDynamicObjectMapper mapper = null)
            => (mapper ?? new DynamicObjectMapper()).MapObject(obj);
    }
}
