// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Tests.Dynamic.DynamicObject
{
    using Aqua.Dynamic;
    using Shouldly;
    using Xunit;

    public class When_initializing_with_null_value
    {
        [Fact]
        public void Should_have_isnull_true_when_created_with_null_value_and_type()
        {
            var obj = new DynamicObject(null, typeof(int));

            obj.IsNull.ShouldBeTrue();
        }

        [Fact]
        public void Should_have_isnull_true_when_created_with_null_value()
        {
            object value = null;

            var obj = new DynamicObject(value);

            obj.IsNull.ShouldBeTrue();
        }
    }
}