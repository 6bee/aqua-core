// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

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

    public abstract class When_serializing_dynamicobject_collections_of_notnullable
    {
        public class With_binary_formatter : When_serializing_dynamicobject_collections_of_notnullable
        {
            public With_binary_formatter()
                : base(BinarySerializationHelper.Clone)
            {
            }
        }

        public class With_data_contract_serializer : When_serializing_dynamicobject_collections_of_notnullable
        {
            public With_data_contract_serializer()
                : base(DataContractSerializationHelper.Clone)
            {
            }
        }

        public class With_newtown_json_serializer : When_serializing_dynamicobject_collections_of_notnullable
        {
            public With_newtown_json_serializer()
                : base(NewtonsoftJsonSerializationHelper.Clone)
            {
            }
        }

        public class With_system_text_json_serializer : When_serializing_dynamicobject_collections_of_notnullable
        {
            public With_system_text_json_serializer()
                : base(SystemTextJsonSerializationHelper.Clone)
            {
            }
        }

#if NETFRAMEWORK
        public class With_net_data_contract_serializer : When_serializing_dynamicobject_collections_of_notnullable
        {
            public With_net_data_contract_serializer()
                : base(NetDataContractSerializationHelper.Clone)
            {
            }
        }
#endif // NETFRAMEWORK

        public class With_protobuf_net_serializer : When_serializing_dynamicobject_collections_of_notnullable
        {
            public With_protobuf_net_serializer()
                : base(ProtobufNetSerializationHelper.Clone)
            {
            }
        }

        public class With_xml_serializer : When_serializing_dynamicobject_collections_of_notnullable
        {
            public With_xml_serializer()
                : base(x => XmlSerializationHelper.Serialize(x?.ToArray()))
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

            IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_source).GetEnumerator();
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

        protected When_serializing_dynamicobject_collections_of_notnullable(Func<IEnumerable<DynamicObject>, IEnumerable<DynamicObject>> serialize)
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
        }

        [Fact]
        public void Should_roundtrip_null_array()
        {
            var resurrected = Roundtrip(default(int[]));
            resurrected.ShouldBeNull();
        }

        [Fact]
        public void Should_roundtrip_enumerable_array()
        {
            var enumerable = new[] { 0, 1, 22, -333 }.AsEnumerable();
            var resurrected = Roundtrip(enumerable);
            resurrected.SequenceShouldBeEqual(enumerable);
        }

        [Fact]
        public void Should_roundtrip_enumerable_List()
        {
            var enumerable = new List<int> { 0, 1, 22, -333 }.AsEnumerable();
            var resurrected = Roundtrip(enumerable);
            resurrected.SequenceShouldBeEqual(enumerable);
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
        }

        private IEnumerable<T> Roundtrip<T>(IEnumerable<T> obj)
        {
            var dynamicObject = new DynamicObjectMapper().MapCollection(obj);
            var serializedDynamicObject = _serialize(dynamicObject);
            var resurrected = new DynamicObjectMapper().Map<T>(serializedDynamicObject);
            return resurrected;
        }
    }
}
