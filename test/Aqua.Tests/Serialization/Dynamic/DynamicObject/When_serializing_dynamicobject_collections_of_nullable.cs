// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Tests.Serialization.Dynamic.DynamicObject;

using Aqua.Dynamic;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Xunit;

public abstract class When_serializing_dynamicobject_collections_of_nullable(Func<IEnumerable<DynamicObject>, IEnumerable<DynamicObject>> serialize)
{
    // protobuf-net doesn't support serialization of collection with null elements as the root object
#if !NET8_0_OR_GREATER
    public class With_binary_formatter() : When_serializing_dynamicobject_collections_of_nullable(BinarySerializationHelper.Clone);

#endif // NET8_0_OR_GREATER

    public class With_data_contract_serializer() : When_serializing_dynamicobject_collections_of_nullable(DataContractSerializationHelper.Clone);

    public class With_newtown_json_serializer() : When_serializing_dynamicobject_collections_of_nullable(NewtonsoftJsonSerializationHelper.Clone);

    public class With_system_text_json_serializer() : When_serializing_dynamicobject_collections_of_nullable(SystemTextJsonSerializationHelper.Clone);

#if NETFRAMEWORK
    public class With_net_data_contract_serializer() : When_serializing_dynamicobject_collections_of_nullable(NetDataContractSerializationHelper.Clone);
#endif // NETFRAMEWORK

    public class With_xml_serializer() : When_serializing_dynamicobject_collections_of_nullable(x => XmlSerializationHelper.Serialize(x.ToArray()));

    public sealed class QueryableProxy<T>(IQueryable<T> source) : IQueryable<T>
    {
        public QueryableProxy(IEnumerable<T> source)
            : this(source.AsQueryable())
        {
        }

        public Expression Expression => source.Expression;

        public Type ElementType => source.ElementType;

        public IQueryProvider Provider => source.Provider;

        public IEnumerator<T> GetEnumerator() => source.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)source).GetEnumerator();
    }

    public sealed class EnumerableProxy<T>(IEnumerable<T> source) : IEnumerable<T>
    {
        public IEnumerator<T> GetEnumerator() => source.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)source).GetEnumerator();
    }

    [Fact]
    public void Should_roundtrip_array_with_two_null_trings()
    {
        var enumerable = new string[] { null, null };
        var resurrected = Roundtrip(enumerable);
        resurrected.SequenceShouldBeEqual(enumerable);
    }

    [Fact]
    public void Should_roundtrip_enumerableproxy()
    {
        var enumerable = new EnumerableProxy<int?>(new int?[] { null, 1, 22, 333 });
        var resurrected = Roundtrip(enumerable);
        resurrected.SequenceShouldBeEqual(enumerable);
    }

    [Fact]
    public void Should_roundtrip_array()
    {
        var enumerable = new int?[] { null, 1, 22, 333 };
        var resurrected = Roundtrip(enumerable);
        resurrected.SequenceShouldBeEqual(enumerable);
    }

    [Fact]
    public void Should_roundtrip_enumerable_array()
    {
        var enumerable = new int?[] { null, 1, 22, 333 }.AsEnumerable();
        var resurrected = Roundtrip(enumerable);
        resurrected.SequenceShouldBeEqual(enumerable);
    }

    [Fact]
    public void Should_roundtrip_enumerable_List()
    {
        var enumerable = new List<int?> { null, 1, 22, 333 }.AsEnumerable();
        var resurrected = Roundtrip(enumerable);
        resurrected.SequenceShouldBeEqual(enumerable);
    }

    [Fact]
    public void Should_roundtrip_queryableproxy()
    {
        var enumerable = new QueryableProxy<int?>(new int?[] { 1, null, 22, 333 }.AsQueryable());
        var resurrected = Roundtrip(enumerable);
        resurrected.SequenceShouldBeEqual(enumerable);
    }

    [Fact]
    public void Should_roundtrip_enumerablequeryable()
    {
        var enumerable = new int?[] { null, 1, 22, 333 }.AsQueryable();
        var resurrected = Roundtrip(enumerable);
        resurrected.SequenceShouldBeEqual(enumerable);
    }

    private IEnumerable<T> Roundtrip<T>(IEnumerable<T> obj)
    {
        var dynamicObject = new DynamicObjectMapper().MapCollection(obj);
        var serializedDynamicObject = serialize(dynamicObject);
        var resurrected = new DynamicObjectMapper().Map<T>(serializedDynamicObject);
        return resurrected;
    }
}