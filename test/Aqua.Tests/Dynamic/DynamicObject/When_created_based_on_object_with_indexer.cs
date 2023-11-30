// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Tests.Dynamic.DynamicObject;

using Aqua.Dynamic;
using Shouldly;
using System.Collections.Generic;
using System.Linq;
using Xunit;

public class When_created_based_on_object_with_indexer
{
    private class ClassWithIndexerAndItemProperty
    {
        private readonly Dictionary<string, object> _data = [];

        [System.Runtime.CompilerServices.IndexerName("MyIndexer")]
        public object this[string key]
        {
            get => _data[key];

            set => _data[key] = value;
        }

        [System.Runtime.CompilerServices.IndexerName("MyIndexer")]
        public object this[int index]
        {
            get => _data.Values.ElementAt(index);

            set
            {
                var key = _data.Keys.ElementAt(index);
                _data[key] = value;
            }
        }

        public string Item { get; set; }
    }

    private readonly ClassWithIndexerAndItemProperty source;
    private readonly DynamicObject dynamicObject;

    public When_created_based_on_object_with_indexer()
    {
        source = new ClassWithIndexerAndItemProperty();
        source.Item = "ItemValue1";
        source["K1"] = new object();

        dynamicObject = new DynamicObject(source);
    }

    [Fact]
    public void Member_count_should_be_one()
    {
        dynamicObject.PropertyCount.ShouldBe(1);
    }

    [Fact]
    public void Member_name_should_be_name_of_property()
    {
        dynamicObject.PropertyNames.ShouldContain("Item");
    }

    [Fact]
    public void Member_value_should_be_value_of_property()
    {
        dynamicObject["Item"].ShouldBe(source.Item);
    }

    [Fact]
    public void Type_property_should_be_set_to_type_of_source_object()
    {
        dynamicObject.Type.ShouldNotBeNull();
        dynamicObject.Type.ToType().ShouldBe(typeof(ClassWithIndexerAndItemProperty));
    }
}
