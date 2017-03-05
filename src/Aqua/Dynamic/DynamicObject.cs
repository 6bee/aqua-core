// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Dynamic
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Linq;
    using System.Runtime.Serialization;
    using TypeSystem;

    [Serializable]
    [DataContract(IsReference = true)]
    [DebuggerDisplay("Count = {PropertyCount}")]
    public partial class DynamicObject : INotifyPropertyChanging, INotifyPropertyChanged
    {
        /// <summary>
        /// Creates a new instance of a dynamic object
        /// </summary>
        public DynamicObject()
            : this(default(TypeInfo), default(PropertySet))
        {
        }

        /// <summary>
        /// Creates a new instance of a dynamic object, setting the specified type
        /// </summary>
        /// <param name="type">The type to be set</param>
        public DynamicObject(Type type)
            : this(ReferenceEquals(null, type) ? null : new TypeInfo(type, includePropertyInfos: false), default(PropertySet))
        {
        }

        /// <summary>
        /// Creates a new instance of a dynamic object, setting the specified type
        /// </summary>
        /// <param name="type">The type to be set</param>
        public DynamicObject(TypeInfo type)
            :this(type, default(PropertySet))
        {
        }

        /// <summary>
        /// Creates a new instance of a dynamic object, setting the specified <see cref="TypeInfo"/> and <see cref="PropertySet"/>
        /// </summary>
        /// <param name="type">The type to be set</param>
        public DynamicObject(TypeInfo type, PropertySet propertySet)
        {
            Type = type;
            Properties = propertySet ?? new PropertySet();
        }

        /// <summary>
        /// Creates a new instance of a dynamic object, setting the specified members
        /// </summary>
        /// <param name="properties">Initial collection of properties and values</param>
        /// <exception cref="ArgumentNullException">The specified members collection is null</exception>
        public DynamicObject(IEnumerable<KeyValuePair<string, object>> properties)
        {
            if (ReferenceEquals(null, properties))
            {
                throw new ArgumentNullException(nameof(properties));
            }

            Properties = new PropertySet(properties.Select(x => new Property(x)));
        }

        /// <summary>
        /// Creates a new instance of a dynamic object, setting the specified members
        /// </summary>
        /// <param name="properties">Initial collection of properties and values</param>
        /// <exception cref="ArgumentNullException">The specified members collection is null</exception>
        public DynamicObject(IEnumerable<Property> properties)
        {
            if (ReferenceEquals(null, properties))
            {
                throw new ArgumentNullException(nameof(properties));
            }

            Properties = new PropertySet(properties);
        }

        /// <summary>
        /// Creates a new instance of a dynamic object, representing the object structure defined by the specified object
        /// </summary>
        /// <param name="obj">The object to be represented by the new dynamic object</param>
        /// <param name="mapper">Optional instance of dynamic object mapper</param>
        /// <exception cref="ArgumentNullException">The specified object is null</exception>
        public DynamicObject(object obj, IDynamicObjectMapper mapper = null)
        {
            if (ReferenceEquals(null, obj))
            {
                throw new ArgumentNullException(nameof(obj));
            }

            var dynamicObject = (mapper ?? new DynamicObjectMapper()).MapObject(obj);
            Type = dynamicObject.Type;
            Properties = dynamicObject.Properties;
        }

        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="dynamicObject">The instance to copy</param>
        /// <param name="deepCopy">If true re-creates <see cref="Property"/> instances, otherwise fills existing <see cref="Property"/> instances into a new <see cref="PropertySet"/></param>
        /// <exception cref="ArgumentNullException">The specified members collection is null</exception>
        internal protected DynamicObject(DynamicObject dynamicObject, bool deepCopy = true)
        {
            if (ReferenceEquals(null, dynamicObject))
            {
                throw new ArgumentNullException(nameof(dynamicObject));
            }

            Type = ReferenceEquals(null, dynamicObject.Type) ? null : new TypeInfo(dynamicObject.Type);

            if (ReferenceEquals(null, dynamicObject.Properties))
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
        /// Gets or sets the type of object represented by this dynamic object instance
        /// </summary>
        [DataMember(Order = 1)]
        public TypeInfo Type { get; set; }

        [DataMember(Order = 2)]
        public PropertySet Properties { get; set; }

        /// <summary>
        /// Gets the count of members (dynamically added properties) hold by this dynamic object
        /// </summary>
        public int PropertyCount => Properties.Count;

        /// <summary>
        /// Gets a collection of member names hold by this dynamic object
        /// </summary>
        public IEnumerable<string> PropertyNames => Properties.Select(x => x.Name).ToList();

        /// <summary>
        /// Gets a collection of member values hold by this dynamic object
        /// </summary>
        public IEnumerable<object> Values => Properties.Select(x => x.Value).ToList();

        /// <summary>
        /// Gets or sets a member value
        /// </summary>
        /// <param name="name">Name of the member to set or get</param>
        /// <returns>Value of the member specified</returns>
        public object this[string name]
        {
            get
            {
                object value;
                if (TryGet(name, out value))
                {
                    return value;
                }

                throw new Exception($"Member not found for name '{name}'");
            }
            set
            {
                Set(name, value);
            }
        }

        /// <summary>
        /// Sets a member and it's value
        /// </summary>
        /// <param name="name">Name of the member to be assigned</param>
        /// <param name="value">The value to be set</param>
        /// <returns>The value specified</returns>
        public object Set(string name, object value)
        {
            var property = Properties.SingleOrDefault(x => string.Equals(x.Name, name));

            var oldValue = property?.Value;
            OnPropertyChanging(name, oldValue, value);

            if (ReferenceEquals(null, property))
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

        protected virtual void OnPropertyChanging(string name, object oldValue, object newValue)
        {
            PropertyChanging?.Invoke(this, new PropertyChangingEventArgs(name));
        }

        protected virtual void OnPropertyChanged(string name, object oldValue, object newValue)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        /// <summary>
        /// Gets a member's value or null if the specified member is unknown
        /// </summary>
        /// <returns>The value assigned to the member specified, null if member is not set</returns>
        public object Get(string name = "")
        {
            object value;
            if (!TryGet(name, out value))
            {
                value = null;
            }

            return value;
        }

        /// <summary>
        /// Gets a member's value or default(T) if the specified member is null or unknown
        /// </summary>
        /// <returns>The value assigned to the member specified, default(T) if member is null or not set</returns>
        public T Get<T>(string name = "")
        {
            var value = Get(name);
            return value is T ? (T)value : default(T);
        }

        /// <summary>
        /// Adds a property and it's value
        /// </summary>
        public void Add(string name, object value) => Properties.Add(name, value);

        /// <summary>
        /// Adds a property
        /// </summary>
        public void Add(Property property) => Properties.Add(property);

        /// <summary>
        /// Removes a member and it's value
        /// </summary>
        /// <returns>True if the member is successfully found and removed; otherwise, false</returns>
        public bool Remove(string name)
        {
            var property = Properties.SingleOrDefault(x => string.Equals(x.Name, name));

            if (ReferenceEquals(null, property))
            {
                return false;
            }

            Properties.Remove(property);
            return true;
        }

        /// <summary>
        /// Gets the value assigned to the specified member
        /// </summary>
        /// <param name="name">The name of the member</param>
        /// <param name="value">When this method returns, contains the value assgned with the specified member, 
        /// if the member is found; null if the member is not found.</param>
        /// <returns>True is the dynamic object contains a member with the specified name; otherwise false</returns>
        public bool TryGet(string name, out object value)
        {
            var property = Properties.SingleOrDefault(x => string.Equals(x.Name, name));

            if (ReferenceEquals(null, property))
            {
                value = null;
                return false;
            }

            value = property.Value;
            return true;
        }
        
        [Obsolete("Method renamed to Create. This method is beeing removed in a future version.", true)]
        public static DynamicObject CreateDynamicObject(object obj, IDynamicObjectMapper mapper = null)
            => Create(obj, mapper);

        /// <summary>
        /// Creates a dynamic objects representing the object structure defined by the specified object
        /// </summary>
        /// <param name="obj">The object to be represented by the new dynamic object</param>
        /// <param name="mapper">Optional instance of dynamic object mapper</param>
        public static DynamicObject Create(object obj, IDynamicObjectMapper mapper = null)
        {
            return (mapper ?? new DynamicObjectMapper()).MapObject(obj);
        }
    }
}
