// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Tests.Serialization.Dynamic.DynamicObject
{
    using Aqua.Dynamic;
    using Aqua.Tests.Serialization;
    using Shouldly;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using Xunit;

    public abstract class When_serializing_dynamicobject_collections_of_nullable
    {
        // protobuf-net doesn't support serialization of collection with null elements as the root object
        public class BinaryFormatter : When_serializing_dynamicobject_collections_of_nullable
        {
            public BinaryFormatter()
                : base(BinarySerializationHelper.Serialize)
            {
            }
        }

        public class DataContractSerializer : When_serializing_dynamicobject_collections_of_nullable
        {
            public DataContractSerializer()
                : base(DataContractSerializationHelper.Serialize)
            {
            }
        }

        public class JsonSerializer : When_serializing_dynamicobject_collections_of_nullable
        {
            public JsonSerializer()
                : base(JsonSerializationHelper.Serialize)
            {
            }
        }

#if NETFX
        public class NetDataContractSerializer : When_serializing_dynamicobject_collections_of_nullable
        {
            public NetDataContractSerializer()
                : base(NetDataContractSerializationHelper.Serialize)
            {
            }
        }
#endif

        public class XmlSerializer : When_serializing_dynamicobject_collections_of_nullable
        {
            public XmlSerializer()
                : base(x => XmlSerializationHelper.Serialize(x.ToArray()))
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

        private readonly Func<IEnumerable<DynamicObject>, IEnumerable<DynamicObject>> _serialize;

        protected When_serializing_dynamicobject_collections_of_nullable(Func<IEnumerable<DynamicObject>, IEnumerable<DynamicObject>> serialize)
        {
            _serialize = serialize;
        }

        [Fact]
        public void Should_roundtrip_array_with_two_null_trings()
        {
            var enumerable = new string[] { null, null };
            var resurrected = Roundtrip(enumerable);
            AssertSequenceEqual(enumerable, resurrected);
        }

        [Fact]
        public void Should_roundtrip_enumerableproxy()
        {
            var enumerable = new EnumerableProxy<int?>(new int?[] { null, 1, 22, 333 });
            var resurrected = Roundtrip(enumerable);
            resurrected
                .ShouldBeAssignableTo<IEnumerable<int?>>()
                .SequenceEqual(enumerable).ShouldBeTrue();
        }

        [Fact]
        public void Should_roundtrip_array()
        {
            var enumerable = new int?[] { null, 1, 22, 333 };
            var resurrected = Roundtrip(enumerable);
            AssertSequenceEqual(enumerable, resurrected);
        }

        [Fact]
        public void Should_roundtrip_enumerable_array()
        {
            var enumerable = new int?[] { null, 1, 22, 333 }.AsEnumerable();
            var resurrected = Roundtrip(enumerable);
            AssertSequenceEqual(enumerable, resurrected);
        }

        [Fact]
        public void Should_roundtrip_enumerable_List()
        {
            var enumerable = new List<int?> { null, 1, 22, 333 }.AsEnumerable();
            var resurrected = Roundtrip(enumerable);
            AssertSequenceEqual(enumerable, resurrected);
        }

        [Fact]
        public void Should_roundtrip_queryableproxy()
        {
            var enumerable = new QueryableProxy<int?>(new int?[] { 1, null, 22, 333 }.AsQueryable());
            var resurrected = Roundtrip(enumerable);
            AssertSequenceEqual(enumerable, resurrected);
        }

        [Fact]
        public void Should_roundtrip_enumerablequeryable()
        {
            var enumerable = new int?[] { null, 1, 22, 333 }.AsQueryable();
            var resurrected = Roundtrip(enumerable);
            AssertSequenceEqual(enumerable, resurrected);
        }

        private static void AssertSequenceEqual<T>(IEnumerable<T> source, IEnumerable<T> result)
            => result
            .ShouldBeAssignableTo<IEnumerable<T>>()
            .SequenceEqual(source).ShouldBeTrue();

        private IEnumerable<T> Roundtrip<T>(IEnumerable<T> obj)
        {
            var dynamicObject = new DynamicObjectMapper().MapCollection(obj);
            var serializedDynamicObject = _serialize(dynamicObject);
            var resurrected = new DynamicObjectMapper().Map<T>(serializedDynamicObject);
            return resurrected;
        }
    }
}
