// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Tests.Serialization.TypeSystem.TypeInfo
{
    using Aqua.TypeSystem;
    using System;
    using Xunit;
    using Shouldly;

    public abstract partial class When_using_typeinfo_with_circular_reference_with_anonymous_type
    {
        TypeInfo serializedTypeInfo;

        public When_using_typeinfo_with_circular_reference_with_anonymous_type(Func<TypeInfo, TypeInfo> serialize)
        {
            var instance = new
            {
                Number = 1,
                Value = new
                {
                    X = new 
                    { 
                        Name = "" 
                    }
                }
            };

            var typeInfo = new TypeInfo(instance.GetType(), false, false);

            serializedTypeInfo = serialize(typeInfo);
        }

        [Fact]
        public void Type_info_should_be_generic()
        {
            serializedTypeInfo.IsGenericType.ShouldBeTrue();
        }
    }
}
