// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Tests.Dynamic.DynamicObject
{
    using Aqua.Dynamic;
    using System;
    using Xunit;
    using Xunit.Fluent;

    public class When_created_using_parameterless_constructor
    {
        DynamicObject dynamicObject = new DynamicObject();

        [Fact]
        public void Should_be_empty()
        {
            dynamicObject.MemberCount.ShouldBe(0);
        }

        [Fact]
        public void Type_property_should_be_null()
        {
            dynamicObject.Type.ShouldBeNull();
        }
    }
}
