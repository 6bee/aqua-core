// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.NET35.Tests.Lazy
{
    using Shouldly;
    using System.Threading;
    using Xunit;

    public class When_using_threadsave_lazy_object
    {
        int count = 0;
        Lazy<int> lazy;

        public When_using_threadsave_lazy_object()
        {
            lazy = new Lazy<int>(() => Interlocked.Increment(ref count), true);
        }

        [Fact]
        public void Should_not_execute_factory_delegate_if_value_property_was_not_accessed()
        {
            Assert.Equal(0, count);
        }

        [Fact]
        public void Should_return_false_on_is_value_create_before_accessing_value_property()
        {
            Assert.False(lazy.IsValueCreated);
        }

        [Fact]
        public void Should_return_true_on_is_value_create_after_accessing_value_property()
        {
            var v = lazy.Value;
            lazy.IsValueCreated.ShouldBeTrue();
        }

        [Fact]
        public void Value_property_should_return_expected_value()
        {
            lazy.Value.ShouldBe(1);
            lazy.Value.ShouldBe(1);
            lazy.Value.ShouldBe(1);
        }

        [Fact]
        public void Should_execute_factory_delegate_only_once()
        {
            var v1 = lazy.Value;
            var v2 = lazy.Value;
            var v3 = lazy.Value;

            count.ShouldBe(1);            
        }
    }
}
