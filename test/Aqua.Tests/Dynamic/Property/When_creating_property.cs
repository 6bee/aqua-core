// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Tests.Dynamic.Property;

using Aqua.Dynamic;
using Shouldly;
using System;
using System.Collections.Generic;
using Xunit;

public class When_creating_property
{
    private const string Name = "Pi";
    private const double Value = Math.PI;

    [Fact]
    public void Name_name_and_value_should_be_set()
    {
        var property = new Property(Name, Value);

        property.Name.ShouldBe(Name);
        property.Value.ShouldBe(Value);
    }

    [Fact]
    public void Should_allow_null_value()
    {
        var property = new Property(Name, null);

        property.Name.ShouldBe(Name);
        property.Value.ShouldBeNull();
    }

    [Fact]
    public void Should_throw_if_name_is_null()
    {
        var ex = Should.Throw<ArgumentNullException>(() => _ = new Property(null, Value));
        ex.ParamName.ShouldBe("name");
    }

    [Fact]
    public void Should_automatically_convert_from_tuple()
    {
        Property property = (Name, Value);

        property.Name.ShouldBe(Name);
        property.Value.ShouldBe(Value);
    }

    [Fact]
    public void Should_create_from_keyvaluepair()
    {
        var property = Property.From(new KeyValuePair<string, object>(Name, Value));

        property.Name.ShouldBe(Name);
        property.Value.ShouldBe(Value);
    }
}