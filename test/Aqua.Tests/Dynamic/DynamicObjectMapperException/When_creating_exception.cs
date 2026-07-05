// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Tests.Dynamic.DynamicObjectMapperException;

using Aqua.Dynamic;

public sealed class When_creating_exception
{
    [Fact]
    public void Should_created_without_argument()
    {
        _ = new DynamicObjectMapperException();
    }

    [Fact]
    public void Should_preserve_message()
    {
        var message = new string('t', 1);
        var exception = new DynamicObjectMapperException(message);
        exception.Message.ShouldBeSameAs(message);
    }

    [Fact]
    public void Should_preserve_message_and_inner_exception()
    {
        var inner = new Exception();
        var message = new string('t', 1);
        var exception = new DynamicObjectMapperException(message, inner);
        exception.Message.ShouldBeSameAs(message);
        exception.InnerException.ShouldBeSameAs(inner);
    }
}
