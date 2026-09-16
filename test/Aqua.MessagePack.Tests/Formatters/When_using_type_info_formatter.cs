// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.MessagePack.Tests.Formatters;

using Aqua.MessagePack.Formatters;
using Aqua.TypeSystem;
using global::MessagePack;
using System.Buffers;

public class When_using_type_info_formatter
{
    [Fact]
    public void Null_should_round_trip()
    {
        FormatterTestHelper.RoundTrip(TypeInfoFormatter.Instance, default(TypeInfo)).ShouldBeNull();
    }

    [Fact]
    public void Minimal_type_should_round_trip()
    {
        var result = FormatterTestHelper.RoundTrip(TypeInfoFormatter.Instance, new TypeInfo { Name = "Item" });

        result.Name.ShouldBe("Item");
        result.Namespace.ShouldBeNull();
        result.GenericArguments.ShouldBeNull();
    }

    [Fact]
    public void Complete_type_graph_should_round_trip()
    {
        var declaringType = new TypeInfo { Name = "Outer", Namespace = "Example" };
        var propertyType = new TypeInfo { Name = "String", Namespace = "System" };
        var value = new TypeInfo
        {
            Name = "Inner",
            Namespace = "Example",
            DeclaringType = declaringType,
            GenericArguments = [new TypeInfo { Name = "Int32", Namespace = "System" }],
            IsAnonymousType = true,
            IsGenericType = true,
            Properties = [new PropertyInfo("Name", propertyType, declaringType) { IsStatic = false }],
        };

        var result = FormatterTestHelper.RoundTrip(TypeInfoFormatter.Instance, value);

        result.Name.ShouldBe("Inner");
        result.Namespace.ShouldBe("Example");
        result.DeclaringType.Name.ShouldBe("Outer");
        result.GenericArguments.Single().Name.ShouldBe("Int32");
        result.IsAnonymousType.ShouldBeTrue();
        result.IsGenericType.ShouldBeTrue();
        result.Properties.Single().Name.ShouldBe("Name");
        result.Properties.Single().PropertyType.Name.ShouldBe("String");
    }

    [Fact]
    public void Preserve_should_restore_shared_reference_identity()
    {
        var shared = new TypeInfo { Name = "Shared" };
        var value = new TypeInfo { Name = "Root", DeclaringType = shared, GenericArguments = [shared] };
        var options = FormatterTestHelper.Options(ReferenceHandler.Preserve);

        var result = FormatterTestHelper.RoundTrip(TypeInfoFormatter.Instance, value, options);

        result.DeclaringType.ShouldBeSameAs(result.GenericArguments.Single());
    }

    [Fact]
    public void Truncated_field_array_should_default_missing_members()
    {
        var buffer = new ArrayBufferWriter<byte>();
        var writer = new MessagePackWriter(buffer);
        writer.WriteArrayHeader(2);
        writer.Write(0);
        writer.Write("Item");
        writer.Flush();

        var result = FormatterTestHelper.Decode(TypeInfoFormatter.Instance, buffer.WrittenSpan.ToArray());

        result.Name.ShouldBe("Item");
        result.Namespace.ShouldBeNull();
        result.IsGenericType.ShouldBeFalse();
    }

    [Fact]
    public void Extra_fields_should_be_skipped()
    {
        var buffer = new ArrayBufferWriter<byte>();
        var writer = new MessagePackWriter(buffer);
        writer.WriteArrayHeader(9);
        writer.Write(0);
        writer.Write("Item");
        writer.WriteNil();
        writer.WriteNil();
        writer.WriteNil();
        writer.Write(false);
        writer.Write(false);
        writer.WriteNil();
        writer.Write("ignored");
        writer.Flush();

        var result = FormatterTestHelper.Decode(TypeInfoFormatter.Instance, buffer.WrittenSpan.ToArray());

        result.Name.ShouldBe("Item");
    }
}
