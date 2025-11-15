// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Tests.Dynamic.DynamicObject;

using Aqua.Dynamic;
using Shouldly;
using Xunit;

public class When_converting_to_datetime_object
{
    private readonly DateTime sourceValue;
    private readonly DateTime? value;

    public When_converting_to_datetime_object()
    {
        sourceValue = DateTime.Now;
        var dynamicObject = new DynamicObject(typeof(DateTime))
        {
            Properties = new PropertySet
            {
                { string.Empty, sourceValue.ToString("o") },
            },
        };
        value = dynamicObject.CreateObject() as DateTime?;
    }

    [Fact]
    public void Datetime_should_have_value()
    {
        value.HasValue.ShouldBeTrue();
    }

    [Fact]
    public void Datetime_value_should_correscpond_to_source()
    {
        value.Value.ShouldBe(sourceValue);
    }
}