// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Tests.Dynamic.DynamicObject
{
    using Aqua.Dynamic;
    using Shouldly;
    using System.Collections.Generic;
    using System.Linq;
    using Xunit;

    public class When_converting_to_object_with_indexer
    {
        private class ClassWithIndexerAndItemProperty
        {
            private readonly Dictionary<string, object> _data = new Dictionary<string, object>();

            [System.Runtime.CompilerServices.IndexerName("MyIndexer")]
            public object this[string key]
            {
                get
                {
                    return _data[key];
                }

                set
                {
                    _data[key] = value;
                }
            }

            [System.Runtime.CompilerServices.IndexerName("MyIndexer")]
            public object this[int index]
            {
                get
                {
                    return _data.Values.ElementAt(index);
                }

                set
                {
                    var key = _data.Keys.ElementAt(index);
                    _data[key] = value;
                }
            }

            public string Item { get; set; }
        }

        [Fact]
        public void ShouldCreateObjectWithIndexerBasedOnDynamicObject()
        {
            var dynamicObject = new DynamicObject
            {
                Properties = new PropertySet
                {
                    { "Item", "ItemValue1" },
                },
            };

            var obj = dynamicObject.CreateObject<ClassWithIndexerAndItemProperty>();

            obj.Item.ShouldBe("ItemValue1");
        }
    }
}
