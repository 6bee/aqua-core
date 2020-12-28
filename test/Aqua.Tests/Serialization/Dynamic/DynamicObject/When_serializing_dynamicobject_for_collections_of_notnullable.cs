﻿// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Tests.Serialization.Dynamic.DynamicObject
{
    using Aqua.Dynamic;
    using Shouldly;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using Xunit;

    public abstract class When_serializing_dynamicobject_for_collections_of_notnullable
    {
        public class BinaryFormatter : When_serializing_dynamicobject_for_collections_of_notnullable
        {
            public BinaryFormatter()
                : base(BinarySerializationHelper.Serialize)
            {
            }
        }

        public class DataContractSerializer : When_serializing_dynamicobject_for_collections_of_notnullable
        {
            public DataContractSerializer()
                : base(DataContractSerializationHelper.Serialize)
            {
            }
        }

        public class JsonSerializer : When_serializing_dynamicobject_for_collections_of_notnullable
        {
            public JsonSerializer()
                : base(JsonSerializationHelper.Serialize)
            {
            }
        }

#if NETFRAMEWORK
        public class NetDataContractSerializer : When_serializing_dynamicobject_for_collections_of_notnullable
        {
            public NetDataContractSerializer()
                : base(NetDataContractSerializationHelper.Serialize)
            {
            }
        }
#endif // NETFRAMEWORK

#if NETCOREAPP
        public class ProtobufNetSerializer : When_serializing_dynamicobject_for_collections_of_notnullable
        {
            public ProtobufNetSerializer()
                : base(ProtobufNetSerializationHelper.Serialize)
            {
            }
        }
#endif // NETCOREAPP

        public class XmlSerializer : When_serializing_dynamicobject_for_collections_of_notnullable
        {
            public XmlSerializer()
                : base(XmlSerializationHelper.Serialize)
            {
            }
        }

        public sealed class QueryableProxy<T> : IQueryable<T>
        {
            private readonly IQueryable<T> _source;

            public QueryableProxy(IEnumerable<T> source)
            {
                _source = source.AsQueryable();
            }

            public QueryableProxy(IQueryable<T> source)
            {
                _source = source;
            }

            public Expression Expression => _source.Expression;

            public Type ElementType => _source.ElementType;

            public IQueryProvider Provider => _source.Provider;

            public IEnumerator<T> GetEnumerator() => _source.GetEnumerator();

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }

        public sealed class EnumerableProxy<T> : IEnumerable<T>
        {
            private readonly IEnumerable<T> _source;

            public EnumerableProxy(IEnumerable<T> source)
            {
                _source = source;
            }

            public IEnumerator<T> GetEnumerator() => _source.GetEnumerator();

            IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_source).GetEnumerator();
        }

        private readonly Func<DynamicObject, DynamicObject> _serialize;

        protected When_serializing_dynamicobject_for_collections_of_notnullable(Func<DynamicObject, DynamicObject> serialize)
        {
            _serialize = serialize;
        }

        [Fact]
        public void Should_roundtrip_enumerableproxy()
        {
            var enumerable = new EnumerableProxy<int>(new[] { 0, 1, 22, -333 });
            var resurrected = Roundtrip(enumerable);
            resurrected.SequenceShouldBeEqual(enumerable);
        }

        [Fact]
        public void Should_roundtrip_array()
        {
            var enumerable = new[] { 0, 1, 22, -333 };
            var resurrected = Roundtrip(enumerable);
            resurrected.SequenceShouldBeEqual(enumerable);
            resurrected.ShouldBeOfType<int[]>();
        }

        [Fact]
        public void Should_roundtrip_enumerable_array()
        {
            var enumerable = new[] { 0, 1, 22, -333 }.AsEnumerable();
            var resurrected = Roundtrip(enumerable);
            resurrected.SequenceShouldBeEqual(enumerable);
            resurrected.ShouldBeOfType<int[]>();
        }

        [Fact]
        public void Should_roundtrip_enumerable_List()
        {
            var enumerable = new List<int> { 0, 1, 22, -333 }.AsEnumerable();
            var resurrected = Roundtrip(enumerable);
            resurrected.SequenceShouldBeEqual(enumerable);
            resurrected.ShouldBeOfType<List<int>>();
        }

        [Fact]
        public void Should_roundtrip_queryableproxy()
        {
            var enumerable = new QueryableProxy<int>(new[] { 0, 1, 22, -333 }.AsQueryable());
            var resurrected = Roundtrip(enumerable);
            resurrected.SequenceShouldBeEqual(enumerable);
        }

        [Fact]
        public void Should_roundtrip_enumerablequeryable()
        {
            var enumerable = new[] { 0, 1, 22, -333 }.AsQueryable();
            var resurrected = Roundtrip(enumerable);
            resurrected.SequenceShouldBeEqual(enumerable);
            resurrected.ShouldBeOfType<EnumerableQuery<int>>();
        }

        private T Roundtrip<T>(T obj)
        {
            var dynamicObject = new DynamicObjectMapper().MapObject(obj);
            var serializedDynamicObject = _serialize(dynamicObject);
            var resurrected = new DynamicObjectMapper().Map<T>(serializedDynamicObject);
            return resurrected;
        }
    }
}
