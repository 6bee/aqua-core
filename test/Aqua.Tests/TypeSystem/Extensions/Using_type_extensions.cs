// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Tests.TypeSystem.Extensions
{
    using Aqua.TypeSystem.Extensions;
    using Shouldly;
    using Xunit;

    public class Using_type_extensions
    {
        private class CustomType
        {
        }

        [Fact]
        public void Is_anonymous_should_return_true_for_anonymous_type()
        {
            new { X = 1 }.GetType().IsAnonymousType().ShouldBeTrue();
        }

        [Fact]
        public void Is_anonymous_should_return_false_for_custom_type()
        {
            new CustomType().GetType().IsAnonymousType().ShouldBeFalse();
        }

        [Fact]
        public void Is_anonymous_should_return_false_for_string_type()
        {
            typeof(string).IsAnonymousType().ShouldBeFalse();
        }
    }
}
