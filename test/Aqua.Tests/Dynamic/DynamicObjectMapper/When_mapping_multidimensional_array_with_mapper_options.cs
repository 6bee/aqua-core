// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Tests.Dynamic.DynamicObjectMapper;

using Aqua.Dynamic;

public class When_mapping_multidimensional_array_with_mapper_options
{
    [Fact]
    public void Explicit_target_type_should_restore_array_when_type_information_is_omitted()
    {
        var source = new[,] { { 1, 2 }, { 3, 4 } };
        var mapper = new Aqua.Dynamic.DynamicObjectMapper();

        var dynamicObject = mapper.MapObject(source, static _ => false);
        var result = mapper.Map(dynamicObject, typeof(int[,])).ShouldBeOfType<int[,]>();

        dynamicObject.Type.ShouldBeNull();
        result.ShouldBe(source);
    }

    [Fact]
    public void String_formatted_values_should_restore_array()
    {
        var source = new[,] { { 1, 2 }, { 3, 4 } };
        var mapper = new Aqua.Dynamic.DynamicObjectMapper(new DynamicObjectMapperSettings { FormatNativeTypesAsString = true });

        var dynamicObject = mapper.MapObject(source);
        var result = new Aqua.Dynamic.DynamicObjectMapper().Map<int[,]>(dynamicObject);

        dynamicObject.Get().ShouldBeOfType<object[]>().ShouldAllBe(static x => x is string);
        result.ShouldBe(source);
    }

    [Fact]
    public void Reference_and_null_elements_should_restore_array()
    {
        var source = new Sample[,] { { new() { Value = 1 }, null }, { new() { Value = 2 }, new() { Value = 3 } } };

        var result = DynamicObject.Create(source).CreateObject().ShouldBeOfType<Sample[,]>();

        result[0, 0].Value.ShouldBe(1);
        result[0, 1].ShouldBeNull();
        result[1, 0].Value.ShouldBe(2);
        result[1, 1].Value.ShouldBe(3);
    }

    [Fact]
    public void Non_zero_lower_bound_should_throw()
    {
        var source = Array.CreateInstance(typeof(int), [2, 3], [1, 0]);

        var exception = Should.Throw<DynamicObjectMapperException>(() => DynamicObject.Create(source));

        exception.Message.ShouldContain("non-zero lower bounds");
    }

    public class Sample
    {
        public int Value { get; set; }
    }
}
