// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Tests.Dynamic.PropertySet;

using Aqua.Dynamic;
using Shouldly;
using Xunit;

public class When_modifying_property_set
{
    private const string PropertyName = "P_99";
    private const string InitialValue = "99";

    private readonly Property initialProperty;
    private readonly PropertySet propertySet;

    public When_modifying_property_set()
    {
        initialProperty = new Property(PropertyName, InitialValue);
        propertySet = [initialProperty];
    }

    [Fact]
    public void Should_update_initial_property_via_indexer()
    {
        propertySet[initialProperty.Name] = 2m;
        propertySet.ShouldHaveSingleItem().Value.ShouldBeEquivalentTo(2m);
        initialProperty.Value.ShouldBeEquivalentTo(2m);
    }

    [Fact]
    public void Should_throw_when_add_property_with_name_clash()
    {
        var ex = Should.Throw<InvalidOperationException>(() => propertySet.Add(initialProperty.Name, 2m));
        ex.Message.ShouldBe($"Property '{initialProperty.Name}' already contained.");
    }

    [Fact]
    public void Should_throw_when_add_property_with_name_clash2()
    {
        var ex = Should.Throw<InvalidOperationException>(() => propertySet.Add(new Property(initialProperty.Name, 2m)));
        ex.Message.ShouldBe($"Property '{initialProperty.Name}' already contained.");
    }

    [Fact]
    public void Remove_should_not_modify_property_instance()
    {
        propertySet.Remove(initialProperty);
        propertySet.ShouldBeEmpty();
        initialProperty.Value.ShouldBeEquivalentTo(InitialValue);
    }
}