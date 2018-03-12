// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Tests.TypeSystem.TypeResolver
{
    using Aqua.TypeSystem;
    using Shouldly;
    using System;
    using Xunit;

    public class When_resolving_array_type
    {
        private class A
        {
        }

        private readonly Type type;

        public When_resolving_array_type()
        {
            var typeInfo = new TypeInfo(typeof(A[]));

            type = TypeResolver.Instance.ResolveType(typeInfo);
        }

        [Fact]
        public void Type_should_be_expected_array_type()
        {
            type.ShouldBe(typeof(A[]));
        }
    }
}
