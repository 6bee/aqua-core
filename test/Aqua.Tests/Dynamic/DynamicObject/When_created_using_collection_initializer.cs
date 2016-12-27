// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Tests.Dynamic.DynamicObject
{
    using Aqua.Dynamic;
    using System;
    using Xunit;
    using Shouldly;

    public class When_created_using_collection_initializer
    {
        [Fact]
        public void Should_initialize_with_specified_values()
        {
            var value1 = "Value1";
            var value2 = new object();
            var value3 = DateTime.Now;

            var dynamicObject = new DynamicObject
            {
                Properties = new PropertySet
                {
                    { "Property1",  value1 },
                    { "Property2",  value2 },
                    { "P -- 3", value3 },
                }
            };

            dynamicObject.PropertyCount.ShouldBe(3);

            dynamicObject.PropertyNames.ShouldContain("Property1");
            dynamicObject.PropertyNames.ShouldContain("Property2");
            dynamicObject.PropertyNames.ShouldContain("P -- 3");

            dynamicObject["Property1"].ShouldBe(value1);
            dynamicObject["Property2"].ShouldBe(value2);
            dynamicObject["P -- 3"].ShouldBe(value3);
        }
    }
}
