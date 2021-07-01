// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Tests.Dynamic.DynamicObject
{
    using Aqua.Dynamic;
    using Shouldly;
    using System;
    using Xunit;

    public class When_modifying_dynamic_object
    {
        private const string PropertyName = "P_99";
        private const string InitialValue = "99";

        private readonly Property initialProperty;
        private readonly DynamicObject obj;

        public When_modifying_dynamic_object()
        {
            initialProperty = new Property(PropertyName, InitialValue);
            obj = new DynamicObject(new PropertySet(new[] { initialProperty }));
        }

        [Fact]
        public void Should_update_initial_property_via_indexer()
        {
            obj[initialProperty.Name] = 2m;
            obj.Properties.ShouldHaveSingleItem().Value.ShouldBeEquivalentTo(2m);
            initialProperty.Value.ShouldBeEquivalentTo(2m);
        }

        [Fact]
        public void Should_update_initial_property_via_set_name_and_vbalue_method()
        {
            var p = obj.Set(initialProperty.Name, 2m);
            p.ShouldBeEquivalentTo(initialProperty);
            obj.Properties.ShouldHaveSingleItem().Value.ShouldBeEquivalentTo(2m);
            initialProperty.Value.ShouldBeEquivalentTo(2m);
        }

        [Fact]
        public void Should_replace_initial_property_via_set_property_method()
        {
            var p = new Property(initialProperty.Name, 2m);
            obj.Set(p);
            obj.Properties.ShouldHaveSingleItem().Value.ShouldBeEquivalentTo(2m);
            initialProperty.Value.ShouldBeEquivalentTo(InitialValue);
        }

        [Fact]
        public void Should_throw_when_add_property_with_name_clash()
        {
            var ex = Should.Throw<InvalidOperationException>(() => obj.Add(initialProperty.Name, 2m));
            ex.Message.ShouldBe($"Property '{initialProperty.Name}' already contained.");
        }

        [Fact]
        public void Should_throw_when_add_property_with_name_clash2()
        {
            var ex = Should.Throw<InvalidOperationException>(() => obj.Add(new Property(initialProperty.Name, 2m)));
            ex.Message.ShouldBe($"Property '{initialProperty.Name}' already contained.");
        }

        [Fact]
        public void Remove_should_not_modify_property_instance()
        {
            obj.Remove(initialProperty.Name);
            obj.Properties.ShouldBeEmpty();
            initialProperty.Value.ShouldBeEquivalentTo(InitialValue);
        }
    }
}
