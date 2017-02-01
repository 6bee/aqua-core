// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Dynamic
{
    using Aqua.TypeSystem;
    using Aqua.TypeSystem.Extensions;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using MethodInfo = System.Reflection.MethodInfo;
    using PropertyInfo = System.Reflection.PropertyInfo;

    public partial class DynamicObjectMapper : IDynamicObjectMapper
    {
        private sealed class ObjectFormatterContext<TFrom, TTo>
        {
            private readonly Func<Type, Type> _dynamicObjectTypeInfoMapper;

            private readonly Dictionary<TFrom, TTo> _referenceMap;

            public ObjectFormatterContext(Func<Type, Type> dynamicObjectTypeMapper = null)
            {
                _dynamicObjectTypeInfoMapper = dynamicObjectTypeMapper ?? (t => t);
                _referenceMap = new Dictionary<TFrom, TTo>(ReferenceEqualityComparer<TFrom>.Default);
            }

            /// <summary>
            /// Returns an existing instance if found in the reference map, creates a new instance otherwise
            /// </summary>
            internal TTo TryGetOrCreateNew(Type objectType, TFrom from, Func<Type, TFrom, TTo> factory, Action<Type, TFrom, TTo> initializer)
            {
                TTo to;
                if (!_referenceMap.TryGetValue(from, out to))
                {
                    to = factory(objectType, from);

                    try
                    {
                        _referenceMap.Add(from, to);
                    }
                    catch
                    {
                        // detected cyclic reference
                        // can happen for non-serializable types without parameterless constructor, which have cyclic references 
                        return _referenceMap[from];
                    }

                    if (!ReferenceEquals(null, initializer))
                    {
                        initializer(objectType, from, to);
                    }
                }

                return to;
            }

            /// <summary>
            /// Returns an existing instance if found in the reference map, creates a new instance otherwise
            /// </summary>
            internal TTo TryGetOrCreateNew(Type objectType, TFrom from, Func<Type, TFrom, Func<Type, bool>, TTo> factory, Action<Type, TFrom, TTo, Func<Type, bool>> initializer, Func<Type, bool> setTypeInformation)
            {
                TTo to;
                if (!_referenceMap.TryGetValue(from, out to))
                {
                    var setTypeInformationValue = ReferenceEquals(null, setTypeInformation) ? true : setTypeInformation(objectType);

                    to = factory(setTypeInformationValue ? _dynamicObjectTypeInfoMapper(objectType) : null, from, setTypeInformation);

                    try
                    {
                        _referenceMap.Add(from, to);
                    }
                    catch
                    {
                        // detected cyclic reference
                        // can happen for non-serializable types without parameterless constructor, which have cyclic references 
                        return _referenceMap[from];
                    }

                    if (!ReferenceEquals(null, initializer))
                    {
                        initializer(objectType, from, to, setTypeInformation);
                    }
                }

                return to;
            }
        }

        private const string NumericPattern = @"([0-9]*\.?[0-9]+|[0-9]+\.?[0-9]*)([eE][+-]?[0-9]+)?";

        private static readonly string ComplexNumberParserRegexPattern = $"^(?<Re>[+-]?({NumericPattern}))(?<Sign>[+-])[iI](?<Im>{NumericPattern})$";

        private static readonly Type _genericDictionaryType = typeof(Dictionary<,>);

        private static readonly Type _genericKeyValuePairType = typeof(KeyValuePair<,>);

        private static readonly Func<Type, bool> _isNativeType = new[]
            {
                typeof(string),

                typeof(int),
                typeof(int?),
                typeof(uint),
                typeof(uint?),

                typeof(byte),
                typeof(byte?),
                typeof(sbyte),
                typeof(sbyte?),

                typeof(short),
                typeof(short?),
                typeof(ushort),
                typeof(ushort?),

                typeof(long),
                typeof(long?),
                typeof(ulong),
                typeof(ulong?),

                typeof(float),
                typeof(float?),

                typeof(double),
                typeof(double?),

                typeof(decimal),
                typeof(decimal?),

                typeof(char),
                typeof(char?),

                typeof(bool),
                typeof(bool?),

                typeof(Guid),
                typeof(Guid?),

                typeof(DateTime),
                typeof(DateTime?),

                typeof(TimeSpan),
                typeof(TimeSpan?),

                typeof(DateTimeOffset),
                typeof(DateTimeOffset?),

#if NET || NETSTANDARD  || CORECLR
                typeof(System.Numerics.BigInteger),
                typeof(System.Numerics.BigInteger?),

                typeof(System.Numerics.Complex),
                typeof(System.Numerics.Complex?),
#endif
            }.ToDictionary(x => x, x => (object)null).ContainsKey;

        private readonly static Dictionary<Type, Dictionary<Type, object>> _implicitNumericConversionsTable = new Dictionary<Type, Dictionary<Type, object>>() 
        {
            // source: http://msdn.microsoft.com/en-us/library/y5b434w4.aspx
            { typeof(sbyte), new[] { typeof(short), typeof(int), typeof(long), typeof(float), typeof(double), typeof(decimal) }.ToDictionary(x => x, x => default(object)) },
            { typeof(byte), new[] { typeof(short), typeof(ushort), typeof(int), typeof(uint), typeof(long), typeof(ulong), typeof(float), typeof(double), typeof(decimal) }.ToDictionary(x => x, x => default(object)) },
            { typeof(short), new[] { typeof(int), typeof(long), typeof(float), typeof(double), typeof(decimal) }.ToDictionary(x => x, x => default(object)) },
            { typeof(ushort), new[] { typeof(int), typeof(uint), typeof(long), typeof(ulong), typeof(float), typeof(double), typeof(decimal) }.ToDictionary(x => x, x => default(object)) },
            { typeof(int), new[] { typeof(long), typeof(float), typeof(double), typeof(decimal) }.ToDictionary(x => x, x => default(object)) },
            { typeof(uint), new[] { typeof(long), typeof(ulong), typeof(float), typeof(double), typeof(decimal) }.ToDictionary(x => x, x => default(object)) },
            { typeof(long), new[] { typeof(float), typeof(double), typeof(decimal) }.ToDictionary(x => x, x => default(object)) },
            { typeof(char), new[] { typeof(ushort), typeof(int), typeof(uint), typeof(long), typeof(ulong), typeof(float), typeof(double), typeof(decimal) }.ToDictionary(x => x, x => default(object)) },
            { typeof(float), new[] { typeof(double) }.ToDictionary(x => x, x => default(object)) },
            { typeof(ulong), new[] { typeof(float), typeof(double), typeof(decimal) }.ToDictionary(x => x, x => default(object)) },
        };

        private static readonly MethodInfo ToDictionaryMethodInfo = typeof(DynamicObjectMapper)
            .GetMethod("ToDictionary", BindingFlags.Static | BindingFlags.NonPublic);

        private static readonly MethodInfo _mapDynamicObjectInternalMethod = typeof(DynamicObjectMapper)
            .GetMethods(BindingFlags.Instance | BindingFlags.NonPublic)
            .Where(x => x.Name == "MapInternal")
            .Where(x => x.IsGenericMethod && x.GetGenericArguments().Length == 1)
            .Where(x => MatchParameters(x, typeof(DynamicObject)))
            .Single();

        private readonly ObjectFormatterContext<DynamicObject, object> _fromContext;
        private readonly ObjectFormatterContext<object, DynamicObject> _toContext;
        private readonly Func<TypeSystem.TypeInfo, Type> _resolveType;
        private readonly Func<Type, bool> _isKnownType;
        private readonly Func<Type, object, DynamicObject> _createDynamicObject;
        private readonly bool _suppressMemberAssignabilityValidation;
        private readonly bool _formatPrimitiveTypesAsString;

        /// <summary>
        /// Creates a new instance of <see cref="DynamicObjectMapper"/>
        /// </summary>
        /// <param name="settings">Optional settings for dynamic object mapping</param>
        /// <param name="typeResolver">Optional instance to be used to resolve types</param>
        /// <param name="typeMapper">This optional parameter allows mapping type information which get set into the <see cref="DynamicObject"/>s upon their creation.</param>
        /// <param name="dynamicObjectFactory">This optional parameter allows injection of a custom factory for <see cref="DynamicObject"/>.</param>
        /// <param name="isKnownTypeProvider">Optional instance to decide whether a type requires to be mapped into a <see cref="DynamicObject"/>, know types do not get mapped</param>
        public DynamicObjectMapper(DynamicObjectMapperSettings settings = null, ITypeResolver typeResolver = null, ITypeMapper typeMapper = null, IDynamicObjectFactory dynamicObjectFactory = null, IIsKnownTypeProvider isKnownTypeProvider = null)
        {
            if (ReferenceEquals(null, settings))
            {
                settings = new DynamicObjectMapperSettings();
            }

            _suppressMemberAssignabilityValidation = !settings.SilentlySkipUnassignableMembers;

            _formatPrimitiveTypesAsString = settings.FormatPrimitiveTypesAsString;

            _fromContext = new ObjectFormatterContext<DynamicObject, object>();

            _toContext = new ObjectFormatterContext<object, DynamicObject>(ReferenceEquals(null, typeMapper) ? default(Func<Type, Type>) : typeMapper.MapType);

            _resolveType = (typeResolver ?? TypeResolver.Instance).ResolveType;

            _isKnownType = ReferenceEquals(null, isKnownTypeProvider)
                ? (t => false)
                : new Func<Type, bool>(isKnownTypeProvider.IsKnownType);

            _createDynamicObject = ReferenceEquals(null, dynamicObjectFactory) 
                ? (t, o) => new DynamicObject(t)
                : new Func<Type, object, DynamicObject>(dynamicObjectFactory.CreateDynamicObject);
        }

        public System.Collections.IEnumerable Map(IEnumerable<DynamicObject> objects, Type type = null)
        {
            if (ReferenceEquals(null, objects))
            {
                throw new ArgumentNullException(nameof(objects));
            }

            var items = objects.Select(x => Map(x, type));

            if (ReferenceEquals(null, type))
            {
                return items.ToArray();
            }
            else
            {
                var r1 = MethodInfos.Enumerable.Cast.MakeGenericMethod(type).Invoke(null, new[] { items });
                var r2 = MethodInfos.Enumerable.ToArray.MakeGenericMethod(type).Invoke(null, new[] { r1 });
                return (System.Collections.IEnumerable)r2;
            }
        }

        /// <summary>
        /// Maps a <see cref="DynamicObject"/> into a collection of objects
        /// </summary>
        /// <param name="obj"><see cref="DynamicObject"/> to be mapped</param>
        /// <param name="targetType">Target type for mapping, set this parameter to null if type information included within <see cref="DynamicObject"/> should be used.</param>
        /// <returns>The object created based on the <see cref="DynamicObject"/> specified</returns>
        public object Map(DynamicObject obj, Type type)
        {
            return MapFromDynamicObjectGraph(obj, type);
        }

        public object Map(DynamicObject obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return null;
            }

            if (ReferenceEquals(null, obj.Type))
            {
                throw new InvalidOperationException("Type property must not be null");
            }

            var type = _resolveType(obj.Type);
            return Map(obj, type);
        }

        /// <summary>
        /// Maps a <see cref="DynamicObject"/> into an instance of <typeparamref name="T"/>
        /// </summary>
        /// <typeparam name="T">The target type in which the <see cref="DynamicObject"/> have to be mapped to</typeparam>
        /// <param name="obj"><see cref="DynamicObject"/> to be mapped</param>
        /// <returns>The object created based on the <see cref="DynamicObject"/> specified</returns>
        public T Map<T>(DynamicObject obj)
        {
            return (T)MapFromDynamicObjectGraph(obj, typeof(T));
        }

        /// <summary>
        /// Maps a collection of <see cref="DynamicObject"/>s into a collection of <typeparamref name="T"/>
        /// </summary>
        /// <typeparam name="T">The target type in which the <see cref="DynamicObject"/> have to be mapped to</typeparam>
        /// <param name="objects">Collection of <see cref="DynamicObject"/>s to be mapped</param>
        /// <returns>Collection of <typeparamref name="T"/> created based on the <see cref="DynamicObject"/>s specified</returns>
        public IEnumerable<T> Map<T>(IEnumerable<DynamicObject> objects)
        {
            return objects.Select(x => Map<T>(x)).ToList();
        }

        /// <summary>
        /// Maps a collection of objects into a collection of <see cref="DynamicObject"/>
        /// </summary>
        /// <param name="objects">The objects to be mapped</param>
        /// <param name="setTypeInformation">Set this parameter to true if type information should be included within the <see cref="DynamicObject"/>s, set it to false otherwise.</param>
        /// <returns>A collection of <see cref="DynamicObject"/> representing the objects specified</returns>
        public IEnumerable<DynamicObject> MapCollection(object obj, Func<Type, bool> setTypeInformation = null)
        {
            IEnumerable<DynamicObject> enumerable;
            if (ReferenceEquals(null, obj))
            {
                enumerable = null;
            }
            else if (obj is IEnumerable<DynamicObject>)
            {
                // cast
                enumerable = (IEnumerable<DynamicObject>)obj;
            }
            else if (obj is System.Collections.IEnumerable && !(obj is string))
            {
                enumerable = ((System.Collections.IEnumerable)obj)
                    .Cast<object>()
                    .Select(x => MapObject(x, setTypeInformation));
            }
            else
            {
                // put single object into dynamic object
                var value = MapObject(obj, setTypeInformation);
                enumerable = new[] { value };
            }
            var list = ReferenceEquals(null, enumerable) ? null : enumerable.ToList();
            return list;
        }

        /// <summary>
        /// Mapps the specified instance into a <see cref="DynamicObject"/>
        /// </summary>
        /// <remarks>Null references and <see cref="DynamicObject"/> are not mapped.</remarks>
        /// <param name="obj">The instance to be mapped</param>
        /// <param name="setTypeInformation">Set this parameter to true if type information should be included within the <see cref="DynamicObject"/>, set it to false otherwise.</param>
        /// <returns>An instance of <see cref="DynamicObject"/> representing the mapped instance</returns>
        public DynamicObject MapObject(object obj, Func<Type, bool> setTypeInformation = null)
        {
            return MapToDynamicObjectGraph(obj, setTypeInformation);
        }

        protected virtual object MapFromDynamicObjectGraph(object obj, Type targetType)
        {
            return MapFromDynamicObjectIfRequired(obj, targetType);
        }

        protected virtual DynamicObject MapToDynamicObjectGraph(object obj, Func<Type, bool> setTypeInformation)
        {
            return MapInternal(obj, setTypeInformation);
        }

        private object MapFromDynamicObjectIfRequired(object obj, Type targetType)
        {
            if (ReferenceEquals(null, obj))
            {
                return null;
            }

            var dynamicObj = obj as DynamicObject;
            if (!ReferenceEquals(null, dynamicObj))
            {
                // subsequent mapping of nested dynamic object
                if (!ReferenceEquals(null, dynamicObj.Type))
                {
                    var type = _resolveType(dynamicObj.Type);
                    if (ReferenceEquals(null, targetType) || targetType.IsAssignableFrom(type))
                    {
                        targetType = type;
                    }
                }

                if (IsSingleValueWrapper(dynamicObj))
                {
                    return MapFromDynamicObjectIfRequired(dynamicObj.Values.Single(), targetType);
                }

                var mappedValue = MapInternal(dynamicObj, targetType);
                return mappedValue;
            }

            var objectType = obj.GetType();

            if (objectType == targetType && !IsCollection(obj))
            {
                return obj;
            }

            if (_isKnownType(objectType))
            {
                return obj;
            }

            if (_isNativeType(targetType))
            {
                return obj is string ? ParseToNativeType(targetType, (string)obj) : obj;
            }

            if (IsCollection(obj))
            {
                var elementType = TypeHelper.GetElementType(targetType);
                var items = ((System.Collections.IEnumerable)obj)
                    .Cast<object>()
                    .Select(x => MapFromDynamicObjectGraph(x, elementType))
                    .ToList();
                var r1 = MethodInfos.Enumerable.Cast.MakeGenericMethod(elementType).Invoke(null, new[] { items });

                if (targetType.IsArray)
                {
                    var r2 = MethodInfos.Enumerable.ToArray.MakeGenericMethod(elementType).Invoke(null, new[] { r1 });
                    return r2;
                }

                if (IsMatchingDictionary(targetType, elementType))
                {
                    var targetTypeGenericArguments = targetType.GetGenericArguments();
                    var method = ToDictionaryMethodInfo.MakeGenericMethod(targetTypeGenericArguments.ToArray());
                    var r2 = method.Invoke(null, new[] { r1 });
                    return r2;
                }

                if (targetType.IsAssignableFrom(typeof(List<>).MakeGenericType(elementType)))
                {
                    var r2 = MethodInfos.Enumerable.ToList.MakeGenericMethod(elementType).Invoke(null, new[] { r1 });
                    return r2;
                }

                throw new Exception($"Failed to project collection of {elementType} into type {targetType}");
            }

            if (targetType.IsEnum())
            {
                if (objectType.IsEnum())
                {
                    if (objectType.Equals(targetType))
                    {
                        return obj;
                    }
                    // TODO: else we could convert by string or numeric value
                }

                if (obj is string)
                {
                    var enumValue = Enum.Parse(targetType, (string)obj, true);
                    return enumValue;
                }

                {
                    var enumValue = Enum.ToObject(targetType, obj);
                    return enumValue;
                }
            }

            return obj;
        }

        private static bool IsCollection(object obj)
        {
            return obj is System.Collections.IEnumerable && !(obj is string);
        }

        /// <summary>
        /// Maps an object to a dynamic object
        /// </summary>
        /// <remarks>Null references and dynamic objects are not mapped.</remarks>
        private DynamicObject MapInternal(object obj, Func<Type, bool> setTypeInformation)
        {
            if (ReferenceEquals(null, obj))
            {
                return null;
            }

            if (obj is DynamicObject)
            {
                return (DynamicObject)obj;
            }

            Func<Type, object, Func<Type, bool>, DynamicObject> facotry;
            Action<Type, object, DynamicObject, Func<Type, bool>> initializer = null;

            var type = obj.GetType();
            if (_isNativeType(type) || _isKnownType(type))
            {
                facotry = (t, o, f) =>
                {
                    var value = MapToDynamicObjectIfRequired(o, f);
                    var dynamicObject = _createDynamicObject(t, o);
                    dynamicObject.Add(string.Empty, value);
                    return dynamicObject;
                };
            }
            else if (type.IsArray)
            {
                facotry = (t, o, f) =>
                {
                    var list = ((System.Collections.IEnumerable)o)
                        .Cast<object>()
                        .Select(x => MapToDynamicObjectIfRequired(x, f))
                        .ToArray();
                    var dynamicObject = _createDynamicObject(t, o);
                    dynamicObject.Add(string.Empty, list.Any() ? list : null);
                    return dynamicObject;
                };
            }
            else
            {
                facotry = (t, o, f) => _createDynamicObject(t, o);
                initializer = PopulateObjectMembers;
            }

            return _toContext.TryGetOrCreateNew(type, obj, facotry, initializer, setTypeInformation);
        }

        /// <summary>
        /// Maps from object to dynamic object if required.
        /// </summary>
        /// <remarks>Null references, strings, value types, and dynamic objects are no mapped.</remarks>
        private object MapToDynamicObjectIfRequired(object obj, Func<Type, bool> setTypeInformation)
        {
            if (ReferenceEquals(null, obj))
            {
                return null;
            }

            if (obj is DynamicObject || obj is string)
            {
                return obj;
            }

            var type = obj.GetType();

            if (_isKnownType(type))
            {
                return obj;
            }

            if (_isNativeType(type))
            {
                return _formatPrimitiveTypesAsString ? FormatNativeTypeAsString(obj, type) : obj;
            }

            if (type.IsEnum())
            {
                return obj.ToString();
            }

            if (obj is System.Collections.IEnumerable)
            {
                var elementType = TypeHelper.GetElementType(type);
                if (elementType != type)
                {
                    if (_isKnownType(elementType))
                    {
                        return obj;
                    }

                    if (_isNativeType(elementType) && !_formatPrimitiveTypesAsString)
                    {
                        return obj;
                    }
                }

                var list = ((System.Collections.IEnumerable)obj)
                    .Cast<object>()
                    .Select(x => MapToDynamicObjectIfRequired(x, setTypeInformation))
                    .ToArray();
                return list;
            }

            return MapToDynamicObjectGraph(obj, setTypeInformation);
        }

        /// <summary>
        /// Extrancts member values from source object and populates to dynamic object 
        /// </summary>
        private void PopulateObjectMembers(Type type, object from, DynamicObject to, Func<Type, bool> setTypeInformation)
        {
            // TODO: add support for ISerializable
            // TODO: add support for OnSerializingAttribute, OnSerializedAttribute, OnDeserializingAttribute, OnDeserializedAttribute
#if NET
            if (type.IsSerializable())
            {
                MapObjectMembers(from, to, setTypeInformation);
            }
            else
            {
#endif
                var properties = GetPropertiesForMapping(type) ??
                    type.GetProperties(BindingFlags.Instance | BindingFlags.Public)
                        .Where(x => x.CanRead && x.GetIndexParameters().Length == 0);
                foreach (var property in properties)
                {
                    var value = property.GetValue(from);
                    value = MapToDynamicObjectIfRequired(value, setTypeInformation);
                    to.Add(property.Name, value);
                }
#if NET
            }
#endif
        }

        /// <summary>
        /// Can be overriden in a derived class to return a list of <see cref="PropertyInfo"/> for a given type or null if defaul behaviour should be applied
        /// </summary>
        /// <returns>If overriden in a derived class, returns a list of <see cref="PropertyInfo"/> for a given type or null if defaul behaviour should be applied</returns>
        protected virtual IEnumerable<PropertyInfo> GetPropertiesForMapping(Type type)
        {
            return null;
        }

        private T MapInternal<T>(DynamicObject obj)
        {
            return MapInternal<T>(new[] { obj }).Single();
        }

        private IEnumerable<T> MapInternal<T>(IEnumerable<DynamicObject> objects)
        {
            if (ReferenceEquals(null, objects))
            {
                //return null;
                throw new ArgumentNullException(nameof(objects));
            }

            if (!objects.Any())
            {
                return new T[0];
            }

            if (typeof(T).IsAssignableFrom(typeof(DynamicObject)) && typeof(T) != typeof(object))
            {
                return objects.Cast<T>();
            }

            var elementType = typeof(T);
            if (objects.All(item => ReferenceEquals(null, item) || IsSingleValueWrapper(item)))
            {
                // project single property
                var items = objects
                    .SelectMany(i => ReferenceEquals(null, i) ? new object[] { null } : i.Values)
                    .Select(x => MapFromDynamicObjectGraph(x, elementType))
                    .ToArray();
                var r1 = MethodInfos.Enumerable.Cast.MakeGenericMethod(elementType).Invoke(null, new[] { items });
                var r2 = MethodInfos.Enumerable.ToArray.MakeGenericMethod(elementType).Invoke(null, new[] { r1 });
                try
                {
                    return (IEnumerable<T>)r2;
                }
                catch (InvalidCastException)
                {
                    var enumerable = (System.Collections.IEnumerable)r2;
                    return enumerable.Cast<T>();
                }
            }
            else
            {
                // project data record
                Func<Type, DynamicObject, object> factory;
                Action<Type, DynamicObject, object> initializer = null;
#if NET
                if (elementType.IsSerializable())
                {
                    factory = (type, item) => GetUninitializedObject(type);
                    initializer = PopulateObjectMembers;
                }
                else
                {
#endif
                    var dynamicProperties = objects.SelectMany(x => x.Properties).Distinct().ToList();
                    var constructor = elementType.GetConstructors()
                        .Select(i =>
                        {
                            var paramterList = i.GetParameters();
                            return new
                            {
                                Info = i,
                                ParametersCount = paramterList.Length,
                                Parameters = paramterList
                                    .Select(parameter => new
                                    {
                                        Info = parameter,
                                        Property = dynamicProperties
                                            .Where(dynamicProperty => string.Equals(dynamicProperty.Name, parameter.Name, StringComparison.OrdinalIgnoreCase))
                                            .Select(dynamicProperty => new { Name = dynamicProperty.Name, Value = MapFromDynamicObjectGraph(dynamicProperty.Value, parameter.ParameterType) })
                                            .SingleOrDefault(dynamicProperty => IsAssignable(parameter.ParameterType, dynamicProperty.Value)),
                                    })
                                    .ToArray(),
                            };
                        })
                        .OrderByDescending(i => i.ParametersCount == 0 ? int.MaxValue : i.ParametersCount)
                        .FirstOrDefault(i => i.Parameters.All(p => !ReferenceEquals(null, p.Property)));

                    if (!ReferenceEquals(null, constructor))
                    {
                        factory = (type, item) =>
                        {
                            var arguments = constructor.Parameters
                                .Select(x => x.Property.Value)
                                .ToArray();
                            var obj = constructor.Info.Invoke(arguments);
                            return obj;
                        };
                        initializer = CreatePropertyInitializer();
                    }
                    else if (elementType.IsValueType())
                    {
                        factory = (type, item) => Activator.CreateInstance(type);
                        initializer = CreatePropertyInitializer();
                    }
                    else
                    {
                        throw new Exception($"Failed to pick matching contructor for type {elementType.FullName}");
                    }
#if NET
                }
#endif

                var list = (System.Collections.IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(elementType));
                foreach (var item in objects)
                {
                    var obj = ReferenceEquals(null, item) ? null : _fromContext.TryGetOrCreateNew(elementType, item, factory, initializer);
                    list.Add(obj);
                }

                return (IEnumerable<T>)list;
            }

            throw new Exception($"Failed to project dynamic objects into type {typeof(T).FullName}");
        }

        private Action<Type, DynamicObject, object> CreatePropertyInitializer()
        {
            return (type, item, obj) =>
            {
                var targetProperties = obj.GetType().GetProperties().Where(p => p.CanWrite);
                foreach (var property in targetProperties)
                {
                    object rawValue;
                    if (item.TryGet(property.Name, out rawValue))
                    {
                        var value = MapFromDynamicObjectGraph(rawValue, property.PropertyType);

                        if (_suppressMemberAssignabilityValidation || IsAssignable(property.PropertyType, value))
                        {
                            property.SetValue(obj, value);
                        }
                    }
                }
            };
        }

        private object MapInternal(DynamicObject obj, Type type)
        {
            var mapper = GetMapInternalMethod(type);
            var result = InvokeMethod(this, mapper, obj);
            return result;
        }

        private static object ParseToNativeType(Type targetType, string value)
        {
            if (targetType == typeof(string))
            {
                return value;
            }

            if (targetType == typeof(int) || targetType == typeof(int?))
            {
                return int.Parse(value);
            }

            if (targetType == typeof(uint) || targetType == typeof(uint?))
            {
                return uint.Parse(value);
            }

            if (targetType == typeof(byte) || targetType == typeof(byte?))
            {
                return byte.Parse(value);
            }

            if (targetType == typeof(sbyte) || targetType == typeof(sbyte?))
            {
                return sbyte.Parse(value);
            }

            if (targetType == typeof(short) || targetType == typeof(short))
            {
                return short.Parse(value);
            }

            if (targetType == typeof(ushort) || targetType == typeof(ushort?))
            {
                return ushort.Parse(value);
            }

            if (targetType == typeof(long) || targetType == typeof(long?))
            {
                return long.Parse(value);
            }

            if (targetType == typeof(ulong) || targetType == typeof(ulong?))
            {
                return ulong.Parse(value);
            }

            if (targetType == typeof(float) || targetType == typeof(float?))
            {
                return float.Parse(value);
            }

            if (targetType == typeof(double) || targetType == typeof(double?))
            {
                return double.Parse(value);
            }

            if (targetType == typeof(decimal) || targetType == typeof(decimal?))
            {
                return decimal.Parse(value);
            }

            if (targetType == typeof(char) || targetType == typeof(char?))
            {
#if WINRT
                char character;
                if (!char.TryParse(value, out character))
                {
                    throw new FormatException($"Value '{value}' cannot be parsed into character.");
                }

                return character;
#else
                return char.Parse(value);
#endif
            }

            if (targetType == typeof(bool) || targetType == typeof(bool?))
            {
                return bool.Parse(value);
            }

            if (targetType == typeof(Guid) || targetType == typeof(Guid?))
            {
                return Guid.Parse(value);
            }

            if (targetType == typeof(DateTime) || targetType == typeof(DateTime?))
            {
                return DateTime.Parse(value);
            }

            if (targetType == typeof(DateTimeOffset) || targetType == typeof(DateTimeOffset?))
            {
                return DateTimeOffset.Parse(value);
            }

            if (targetType == typeof(TimeSpan) || targetType == typeof(TimeSpan?))
            {
                return TimeSpan.Parse(value);
            }
#if NET || NETSTANDARD || CORECLR
            if (targetType == typeof(System.Numerics.BigInteger) || targetType == typeof(System.Numerics.BigInteger?))
            {
                return System.Numerics.BigInteger.Parse(value);
            }

            if (targetType == typeof(System.Numerics.Complex) || targetType == typeof(System.Numerics.Complex?))
            {
                var m = System.Text.RegularExpressions.Regex.Match(value, ComplexNumberParserRegexPattern);
                if (m.Success)
                {
                    var re = double.Parse(m.Groups["Re"].Value);
                    var im = double.Parse(m.Groups["Sign"].Value + m.Groups["Im"].Value);
                    return new System.Numerics.Complex(re, im);
                }
                else
                {
                    throw new FormatException($"Value '{value}' cannot be parsed into complex number.");
                }
            }
#endif
            throw new NotImplementedException($"string parser for type {targetType} is not implemented");
        }

        private static bool IsSingleValueWrapper(DynamicObject item)
        {
            return (item.PropertyCount == 1 && string.IsNullOrEmpty(item.PropertyNames.Single()));
        }

        private static bool IsMatchingDictionary(Type targetType, Type elementType)
        {
            if (!(targetType.IsGenericType() && elementType.IsGenericType()))
            {
                return false;
            }

            var elementTypeGenericTypeDefinition = elementType.GetGenericTypeDefinition();
            if (!_genericKeyValuePairType.IsAssignableFrom(elementTypeGenericTypeDefinition))
            {
                return false;
            }

            var targetTypeGenericArgumentsCount = targetType.GetGenericArguments().Count();
            if (targetTypeGenericArgumentsCount != 2)
            {
                return false;
            }

            var elementTypeGenericArguments = elementType.GetGenericArguments().ToArray();
            var dictionaryType = _genericDictionaryType.MakeGenericType(elementTypeGenericArguments);
            if (!targetType.IsAssignableFrom(dictionaryType))
            {
                return false;
            }

            return true;
        }

        private static object ToDictionary<TKey, TValue>(IEnumerable<KeyValuePair<TKey, TValue>> items)
        {
            return items.ToDictionary(x => x.Key, x => x.Value);
        }

        private static bool IsAssignable(Type targetType, object value)
        {
            if (targetType.IsValueType())
            {
                if (ReferenceEquals(null, value))
                {
                    return targetType.IsGenericType()
                        && targetType.GetGenericTypeDefinition() == typeof(Nullable<>);
                }

                var type = value.GetType();
                return targetType.IsAssignableFrom(type) || HasImplicitNumericConversions(type, targetType);
            }
            else
            {
                return ReferenceEquals(null, value) || targetType.IsAssignableFrom(value.GetType());
            }
        }

        private static bool HasImplicitNumericConversions(Type from, Type to)
        {
            Dictionary<Type, object> toList;
            return _implicitNumericConversionsTable.TryGetValue(from, out toList) && toList.ContainsKey(to);
        }

        private static object FormatNativeTypeAsString(object obj, Type type)
        {
            if (type == typeof(DateTime) || type == typeof(DateTime?))
            {
                return ((DateTime)obj).ToString("o");
            }

            if (type == typeof(float) || type == typeof(float?))
            {
                return ((float)obj).ToString("R");
            }

            if (type == typeof(double) || type == typeof(double?))
            {
                return ((double)obj).ToString("R");
            }
#if NET || NETSTANDARD || CORECLR
            if (type == typeof(System.Numerics.BigInteger) || type == typeof(System.Numerics.BigInteger?))
            {
                return ((System.Numerics.BigInteger)obj).ToString("R");
            }

            if (type == typeof(System.Numerics.Complex) || type == typeof(System.Numerics.Complex?))
            {
                var c = (System.Numerics.Complex)obj;
                return $"{c.Real:R}{Math.Sign(c.Imaginary):+;-}i{Math.Abs(c.Imaginary):R}";
            }
#endif
            return obj.ToString();
        }

        private static bool MatchParameters(MethodInfo x, params Type[] types)
        {
            var parameters = x.GetParameters();
            if (parameters.Length != types.Length)
            {
                return false;
            }

            for (int i = 0; i < parameters.Length; i++)
            {
                if (parameters[i].ParameterType != types[i]) return false;
            }

            return true;
        }

        private static MethodInfo GetMapInternalMethod(Type type)
        {
            return _mapDynamicObjectInternalMethod.MakeGenericMethod(type);
        }

        private static object InvokeMethod(DynamicObjectMapper instance, MethodInfo method, params object[] args)
        {
            try
            {
                return method.Invoke(instance, args);
            }
            catch (TargetInvocationException ex)
            {
                Exception innerException;
                if (ReferenceEquals(null, ex.InnerException))
                {
                    innerException = ex;
                }
                else if (ReferenceEquals(null, ex.InnerException.InnerException))
                {
                    innerException = ex.InnerException;
                }
                else
                {
                    innerException = ex.InnerException.InnerException;
                }

                throw new Exception(innerException.Message, innerException);
            }
        }
    }
}
