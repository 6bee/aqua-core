// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.MessagePack.Tests.Formatters;

using Aqua.Dynamic;
using Aqua.MessagePack.Formatters;
using Aqua.TypeSystem;

public class When_using_dynamic_object_formatter
{
    [Fact]
    public void Null_should_round_trip() => FormatterTestHelper.RoundTrip(DynamicObjectFormatter.Instance, default(DynamicObject)).ShouldBeNull();

    [Fact]
    public void Null_properties_should_preserve_is_null()
    {
        var result = FormatterTestHelper.RoundTrip(DynamicObjectFormatter.Instance, new DynamicObject((PropertySet)null));

        result.IsNull.ShouldBeTrue();
        result.Properties.ShouldBeNull();
    }

    [Fact]
    public void Typed_object_should_round_trip()
    {
        var value = new DynamicObject(typeof(string), new PropertySet([("Length", (object)4)]));

        var result = FormatterTestHelper.RoundTrip(DynamicObjectFormatter.Instance, value);

        result.Type.Name.ShouldBe("String");
        result.Get<int>("Length").ShouldBe(4);
    }

    [Fact]
    public void Preserve_should_restore_shared_type_identity()
    {
        var type = new TypeInfo { Name = "Shared" };
        var value = new DynamicObject(type, new PropertySet([("Type", (object)type)]));
        var options = FormatterTestHelper.Options(ReferenceHandler.Preserve);

        var result = FormatterTestHelper.RoundTrip(DynamicObjectFormatter.Instance, value, options);

        result.Type.ShouldBeSameAs(result.Get<TypeInfo>("Type"));
    }
}
