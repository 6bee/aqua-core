// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Tests.Dynamic.DynamicObject
{
    using System;
    using System.Linq;
    using Aqua.Dynamic;
    using Xunit;
    using Shouldly;

    public class When_converting_to_datetime_object
    {
        DateTime sourceValue;
        DateTime? value;
        DynamicObject dynamicObject;

        public When_converting_to_datetime_object()
        {
            sourceValue = DateTime.Now;
            dynamicObject = new DynamicObject(typeof(DateTime))
            {
                Properties = new Properties
                {
                    { string.Empty, sourceValue.ToString("o") }
                }
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
}
