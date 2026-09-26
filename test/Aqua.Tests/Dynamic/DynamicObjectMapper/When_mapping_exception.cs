// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Tests.Dynamic.DynamicObjectMapper;

using Aqua.Dynamic;

public class When_mapping_exception
{
    private static readonly DynamicObjectMapper Mapper = new();

    private static readonly Exception Exception = CreateException();

    private static Exception CreateException()
    {
        var inner = new DivideByZeroException("Inner") { Data = { ["division by"] = 0 } };
        return new Exception("Outer", inner) { Data = { ["K"] = "V" } };
    }

    [Fact]
    public void Should_map_exception_without_stack_overflow()
    {
        // mapping an exception used to result in a stack overflow on netfx
        // (Exception._xptrs of type IntPtr expanded into the re-wrapping System.Reflection.Pointer graph)
        Should.NotThrow(() => Mapper.MapObject(Exception));
    }

    [Fact]
    public void Should_map_exception_state_members()
    {
        var mapped = Mapper.MapObject(Exception);

        mapped.Type.ToType().ShouldBe(typeof(Exception));

#if NET8_0_OR_GREATER
        mapped["Message"].ShouldBe("Outer");
        mapped["HResult"].ShouldBe(Exception.HResult);
        mapped["Data"].ShouldBeOfType<DynamicObject[]>();
        mapped["InnerException"].ShouldBeOfType<DynamicObject>().With(inner => inner["Message"].ShouldBe("Inner"));
#else
        // the FormatterServices path maps the serializable (private) fields of the exception
        mapped["_message"].ShouldBe("Outer");
#endif
    }

    [Fact]
    public void Should_map_exception_properties_without_formatter_services()
    {
        Exception exception = new ArgumentException("message", "paramName");
#if NET8_0_OR_GREATER
        var mapper = new DynamicObjectMapper();
#else
        var mapper = new DynamicObjectMapper(new DynamicObjectMapperSettings { UtilizeFormatterServices = false });
#endif

        var mapped = mapper.MapObject(exception);

        // ArgumentException composes the parameter name into the message,
        // the exact format differs between the platforms
#if NET8_0_OR_GREATER
        mapped["Message"].ShouldBe("message (Parameter 'paramName')");
#else
        mapped["Message"].ShouldBe($"message{Environment.NewLine}Parameter name: paramName");
#endif
        mapped["ParamName"].ShouldBe("paramName");
    }
}
