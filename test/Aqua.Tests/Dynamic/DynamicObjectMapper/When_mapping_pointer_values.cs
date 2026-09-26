// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Tests.Dynamic.DynamicObjectMapper;

using Aqua.Dynamic;

using System.Globalization;

public class When_mapping_pointer_values
{
    private struct StructWithPointers
    {
        public IntPtr IntPtr;
        public UIntPtr UIntPtr;
    }

    [Serializable]
    private struct SerializableStructWithPointers
    {
        public IntPtr IntPtr;
        public UIntPtr UIntPtr;
    }

    [Fact]
    public void Should_map_int_ptr_as_long_value()
    {
        var mapper = new DynamicObjectMapper();

        var pointer = new IntPtr(0x00007FFFA1B2C3D4L);

        var mapped = mapper.MapObject(pointer);

        // a pointer is stored as a plain native long value, not as a formatted string or boxed pointer
        mapped[""].ShouldBe(pointer.ToInt64());
    }

    [Fact]
    public void Should_map_uint_ptr_as_ulong_value()
    {
        var mapper = new DynamicObjectMapper();

        var pointer = new UIntPtr(0x00007FFFA1B2C3D4UL);

        var mapped = mapper.MapObject(pointer);

        mapped[""].ShouldBe(pointer.ToUInt64());
    }

    [Fact]
    public void Should_recreate_int_ptr_from_long_and_from_int()
    {
        var mapper = new DynamicObjectMapper();

        var fromLong = new DynamicObject(typeof(long));
        fromLong.Add("", 0x0000000000001234L);
        var fromInt = new DynamicObject(typeof(int));
        fromInt.Add("", 0x00001234);

        mapper.Map<IntPtr>(fromLong).ToInt64().ShouldBe(0x1234L);
        mapper.Map<IntPtr>(fromInt).ToInt64().ShouldBe(0x1234L);
    }

    [Fact]
    public void Should_recreate_pointer_from_string_when_format_native_types_as_string()
    {
        var mapper = new DynamicObjectMapper(new DynamicObjectMapperSettings { FormatNativeTypesAsString = true });

        var pointer = new IntPtr(0x00007FFFA1B2C3D4L);

        var mapped = mapper.MapObject(pointer);

        mapped[""].ShouldBe(pointer.ToInt64().ToString(CultureInfo.InvariantCulture));

        mapper.Map<IntPtr>(mapped).ToInt64().ShouldBe(pointer.ToInt64());
    }

    [Fact]
    public void Should_map_pointer_field_of_struct()
    {
        var mapper = new DynamicObjectMapper();

        var source = new StructWithPointers
        {
            IntPtr = new IntPtr(0x0000000000001234L),
            UIntPtr = new UIntPtr(0x0000000000005678UL),
        };

        var mapped = mapper.MapObject(source);

        mapped["IntPtr"].ShouldBe(0x0000000000001234L);
        mapped["UIntPtr"].ShouldBe(0x0000000000005678UL);

        var result = mapper.Map<StructWithPointers>(mapped);

        result.IntPtr.ToInt64().ShouldBe(0x0000000000001234L);
        result.UIntPtr.ToUInt64().ShouldBe(0x0000000000005678UL);
    }

    [Fact]
    public void Should_map_pointer_field_of_serializable_struct()
    {
        var mapper = new DynamicObjectMapper();

        var source = new SerializableStructWithPointers
        {
            IntPtr = new IntPtr(0x0000000000001234L),
            UIntPtr = new UIntPtr(0x0000000000005678UL),
        };

        var mapped = mapper.MapObject(source);

        mapped["IntPtr"].ShouldBe(0x0000000000001234L);
        mapped["UIntPtr"].ShouldBe(0x0000000000005678UL);

        var result = mapper.Map<SerializableStructWithPointers>(mapped);

        result.IntPtr.ToInt64().ShouldBe(0x0000000000001234L);
        result.UIntPtr.ToUInt64().ShouldBe(0x0000000000005678UL);
    }

    [Fact]
    public void Should_map_int_ptr_without_stack_overflow()
    {
        var mapper = new DynamicObjectMapper();

        // mapping a pointer directly used to recurse infinitely on netfx
        // (System.IntPtr expanded into the re-wrapping System.Reflection.Pointer graph)
        Should.NotThrow(() => mapper.MapObject(new IntPtr(0x000000000000ABCDL)));
        Should.NotThrow(() => mapper.MapObject(new UIntPtr(0x000000000000ABCDUL)));
    }

    [Fact]
    public void Should_reject_pointer_value_exceeding_pointer_size()
    {
        // only relevant on 32-bit runtimes; 64-bit pointers accept any long value
        if (IntPtr.Size is not 4)
        {
            return;
        }

        var mapper = new DynamicObjectMapper();

        var dynamicObject = new DynamicObject(typeof(long));
        dynamicObject.Add("", 0x00007FFFA1B2C3D4L);

        Should.Throw<DynamicObjectMapperException>(() => mapper.Map<IntPtr>(dynamicObject));
    }
}
