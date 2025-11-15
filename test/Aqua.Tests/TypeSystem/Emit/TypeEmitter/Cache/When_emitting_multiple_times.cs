// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Tests.TypeSystem.Emit.TypeEmitter.Cache;

using Aqua.TypeSystem;
using Aqua.TypeSystem.Emit;
using Shouldly;
using Xunit;

public class When_emitting_multiple_times
{
    [Fact]
    public void Should_emity_anonymous_type_as_open_generic_type()
    {
        var o = new { P = default(string) };
        var emitter = new TypeEmitter();
        var type = emitter.EmitType(new TypeInfo(o.GetType()));

        type.IsGenericTypeDefinition.ShouldBeTrue();
        type.ShouldNotBe(o.GetType().GetGenericTypeDefinition());
    }

    [Fact]
    public void Should_return_the_same_instance_for_anonymous_type_with_same_property_definition()
    {
        var emitter = new TypeEmitter();
        var type1 = emitter.EmitType(CreateAnonymousTypeInfo(("P", typeof(int))));
        var type2 = emitter.EmitType(CreateAnonymousTypeInfo(("P", typeof(int))));

        type1.ShouldBeSameAs(type2);
    }

    [Fact]
    public void Should_return_the_same_instance_for_anonymous_type_with_different_property_type()
    {
        var emitter = new TypeEmitter();
        var type1 = emitter.EmitType(CreateAnonymousTypeInfo(("P", typeof(int))));
        var type2 = emitter.EmitType(CreateAnonymousTypeInfo(("P", typeof(uint))));

        type1.ShouldBeSameAs(type2);
    }

    [Fact]
    public void Should_return_different_instance_for_anonymous_type_with_different_property_name()
    {
        var emitter = new TypeEmitter();
        var type1 = emitter.EmitType(CreateAnonymousTypeInfo(("p", typeof(int))));
        var type2 = emitter.EmitType(CreateAnonymousTypeInfo(("P", typeof(int))));

        type1.ShouldNotBeSameAs(type2);
    }

    [Fact]
    public void Should_return_different_instance_for_anonymous_type_with_different_property_order()
    {
        var emitter = new TypeEmitter();
        var type1 = emitter.EmitType(CreateAnonymousTypeInfo(("Q", typeof(int)), ("P", typeof(int))));
        var type2 = emitter.EmitType(CreateAnonymousTypeInfo(("P", typeof(int)), ("Q", typeof(int))));

        type1.ShouldNotBeSameAs(type2);
    }

    private static TypeInfo CreateAnonymousTypeInfo(params (string Name, Type Type)[] properties)
    {
        var t = new TypeInfo
        {
            IsAnonymousType = true,
            IsGenericType = true,
            Name = "T",
            Namespace = "N",
            GenericArguments = properties.Select(x => new TypeInfo(x.Type)).ToList(),
        };
        t.Properties = properties.Select(x => new PropertyInfo(x.Name, new TypeInfo(x.Type), t)).ToList();
        return t;
    }
}