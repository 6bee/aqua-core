// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Tests.Dynamic.DynamicObjectMapper;

using Aqua.Dynamic;

public class When_recreating_exception
{
    private class ExceptionWithCustomCtor(int code) : Exception($"code {code}")
    {
        public int Code { get; } = code;
    }

    private class NonExceptionWithCtors
    {
        public NonExceptionWithCtors()
        {
        }

        public NonExceptionWithCtors(string message)
        {
            FromParameterizedCtor = message;
        }

        public string FromParameterizedCtor { get; }
    }

#if NETFRAMEWORK
    private static readonly DynamicObjectMapper Mapper = new(new() { UtilizeFormatterServices = false });
#else
    private static readonly DynamicObjectMapper Mapper = new();
#endif // NETFRAMEWORK

    [Fact]
    public void Should_recreate_exception_with_message()
    {
        var exception = new DivideByZeroException("Test");

        var mapped = Mapper.MapObject(exception);

        var result = Mapper.Map<DivideByZeroException>(mapped);

        // the message is bound to the parameterized constructor since Exception.Message has no setter
        result.Message.ShouldBe("Test");
    }

    [Fact]
    public void Should_recreate_exception_with_inner_exception_chain()
    {
        var inner = new DivideByZeroException("Inner");
        var outer = new Exception("Outer", inner);

        var mapped = Mapper.MapObject(outer);

        var result = Mapper.Map<Exception>(mapped);

        result.Message.ShouldBe("Outer");
        result.InnerException.ShouldNotBeNull();
        result.InnerException.Message.ShouldBe("Inner");
    }

    [Fact]
    public void Should_recreate_exception_with_data()
    {
        var exception = new DivideByZeroException("Test") { Data = { ["K"] = "V" } };

        var mapped = Mapper.MapObject(exception);

        // the content of the dictionary-typed Data member is not restored
        // (pre-existing dictionary property limitation); the recreation must not fail
        var result = Should.NotThrow(() => Mapper.Map<DivideByZeroException>(mapped));

        result.Message.ShouldBe("Test");

        // the Data property is lazily initialized, so it is not null even if its content was not restored
        result.Data.ShouldNotBeNull();

        // data items are not restored since Data property is read-only
        result.Data.Count.ShouldBe(0);
    }

    [Fact]
    public void Should_recreate_exception_hresult_source_and_help_link()
    {
        var exception = new Exception("Test")
        {
            Source = "test-source",
            HelpLink = "https://example.com",
        };
#if NET8_0_OR_GREATER
        exception.HResult = unchecked((int)0x80004005);
#endif

        var mapped = Mapper.MapObject(exception);

        var result = Mapper.Map<Exception>(mapped);

        result.Message.ShouldBe("Test");
        result.Source.ShouldBe("test-source");
        result.HelpLink.ShouldBe("https://example.com");
        result.HResult.ShouldBe(exception.HResult);
    }

    [Fact]
    public void Should_recreate_exception_type_without_message_constructor()
    {
        var exception = new StackOverflowException();

        var mapped = Mapper.MapObject(exception);

        var result = Mapper.Map<StackOverflowException>(mapped);

        result.ShouldNotBeNull();
        result.Message.ShouldBe(exception.Message);
    }

    [Fact]
    public void Should_recreate_exception_type_with_custom_ctor_parameters()
    {
        var exception = new ExceptionWithCustomCtor(42);

        var mapped = Mapper.MapObject(exception);

        var result = Mapper.Map<ExceptionWithCustomCtor>(mapped);

        result.Message.ShouldBe("code 42");
        result.Code.ShouldBe(42);
    }

    [Fact]
    public void Should_keep_parameterless_ctor_preference_for_non_exception_types()
    {
        var mapper = new DynamicObjectMapper();

        var mapped = new DynamicObject(typeof(NonExceptionWithCtors));
        mapped.Add("message", "ignored");

        var result = mapper.Map<NonExceptionWithCtors>(mapped);

        // non-exception types must keep the parameterless ctor preference,
        // the captured "message" value must not be bound to the parameterized ctor
        result.FromParameterizedCtor.ShouldBeNull();
    }
}
