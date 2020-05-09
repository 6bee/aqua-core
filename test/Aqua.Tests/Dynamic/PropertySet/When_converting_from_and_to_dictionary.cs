// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Tests.Dynamic.PropertySet
{
    using Aqua.Dynamic;
    using Shouldly;
    using System.Collections.Generic;
    using Xunit;

    public class When_converting_from_and_to_dictionary
    {
        [Fact]
        public void Should_implicit_cast_property_set_to_dictionary()
        {
            var propertySet = new PropertySet
            {
                new Property("P1", 1),
                new Property("P2", "2"),
                new Property("P3", null),
            };

            Dictionary<string, object> dictionary = propertySet;

            dictionary.Count.ShouldBe(3);
            dictionary.ShouldContainKeyAndValue("P1", 1);
            dictionary.ShouldContainKeyAndValue("P2", "2");
            dictionary.ShouldContainKeyAndValue("P3", null);
        }

        [Fact]
        public void Should_implicit_cast_dictionary_to_property_set()
        {
            var dictionary = new Dictionary<string, object>
            {
                { "P1", 1 },
                { "P2", "2" },
                { "P3", null },
            };

            PropertySet propertySet = dictionary;

            propertySet.Count.ShouldBe(3);
            propertySet["P1"].ShouldBe(1);
            propertySet["P2"].ShouldBe("2");
            propertySet["P3"].ShouldBeNull();
        }
    }
}
