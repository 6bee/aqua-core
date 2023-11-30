// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Tests.Dynamic.DynamicObjectMapper;

using Aqua.Dynamic;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

public class When_mapping_collection_from_list_of_nullable_guids
{
    private readonly List<Guid?> source;
    private readonly IEnumerable<DynamicObject> dynamicObjects;

    public When_mapping_collection_from_list_of_nullable_guids()
    {
        source = new List<Guid?> { Guid.NewGuid(), Guid.NewGuid(), null };
        dynamicObjects = new DynamicObjectMapper().MapCollection(source);
    }

    [Fact]
    public void Dynamic_objects_count_should_be_three()
    {
        dynamicObjects.Count().ShouldBe(3);
    }

    [Fact]
    public void Dynamic_objects_type_property_should_be_set_to_guid()
    {
        dynamicObjects.ElementAt(0).Type.ToType().ShouldBe(typeof(Guid));
        dynamicObjects.ElementAt(1).Type.ToType().ShouldBe(typeof(Guid));
        dynamicObjects.ElementAt(2).ShouldBeNull();
    }

    [Fact]
    public void Dynamic_objects_should_have_one_member()
    {
        dynamicObjects.ElementAt(0).PropertyCount.ShouldBe(1);
        dynamicObjects.ElementAt(1).PropertyCount.ShouldBe(1);
        dynamicObjects.ElementAt(2).ShouldBeNull();
    }

    [Fact]
    public void Dynamic_objects_member_name_should_be_empty_string()
    {
        dynamicObjects.ElementAt(0).PropertyNames.Single().ShouldBe(string.Empty);
        dynamicObjects.ElementAt(1).PropertyNames.Single().ShouldBe(string.Empty);
        dynamicObjects.ElementAt(2).ShouldBeNull();
    }

    [Fact]
    public void Dynamic_objects_member_values_should_be_value_of_source()
    {
        dynamicObjects.ElementAt(0)[string.Empty].ShouldBe(source[0]);
        dynamicObjects.ElementAt(1)[string.Empty].ShouldBe(source[1]);
        dynamicObjects.ElementAt(2).ShouldBeNull();
    }
}
