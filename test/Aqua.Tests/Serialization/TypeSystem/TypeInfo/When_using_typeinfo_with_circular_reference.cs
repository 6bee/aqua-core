// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Tests.Serialization.TypeSystem.TypeInfo
{
    using Aqua.TypeSystem;
    using Shouldly;
    using System;
    using System.Linq;
    using Xunit;

    public partial class When_using_typeinfo_with_circular_reference
    {
        private class A
        {
            public A SelfReference { get; set; }
        }

        private readonly TypeInfo serializedTypeInfo;

        protected When_using_typeinfo_with_circular_reference(Func<TypeInfo, TypeInfo> serialize)
        {
            var typeInfo = new TypeInfo(typeof(A), true);

            serializedTypeInfo = serialize(typeInfo);

            serializedTypeInfo.ShouldNotBeSameAs(typeInfo);
        }

        [Fact]
        public void Serialization_should_leave_circular_reference_intact()
        {
            serializedTypeInfo.Properties.Single().DeclaringType.ShouldBeSameAs(serializedTypeInfo);
        }
    }
}
