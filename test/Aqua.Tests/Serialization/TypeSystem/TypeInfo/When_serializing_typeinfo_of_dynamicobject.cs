// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Tests.Serialization.TypeSystem.TypeInfo
{
    using Aqua.Dynamic;
    using Aqua.TypeSystem;
    using Shouldly;
    using System;
    using Xunit;

    public abstract partial class When_serializing_typeinfo_of_dynamicobject
    {
        TypeInfo typeInfo;
        TypeInfo serializedTypeInfo;

        protected When_serializing_typeinfo_of_dynamicobject(Func<TypeInfo, TypeInfo> serialize)
        {
            typeInfo = new TypeInfo(typeof(DynamicObject), true);

            serializedTypeInfo = serialize(typeInfo);
        }

        [Fact]
        public void Serialization_should_return_new_instance()
        {
            serializedTypeInfo.ShouldNotBeSameAs(typeInfo);
        }
    }
}
