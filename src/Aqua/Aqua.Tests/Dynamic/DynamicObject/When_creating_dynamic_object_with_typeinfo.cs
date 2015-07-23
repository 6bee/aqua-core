// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Tests.Dynamic.DynamicObject
{
    using Aqua.Dynamic;
    using Aqua.TypeSystem;
    using Xunit;
    using Xunit.Should;

    public class When_creating_dynamic_object_with_typeinfo
    {
        class CustomClass { }

        [Fact]
        public void ShouldSetTypePropertyWhenPassingTypeInfoToConstructor()
        {
            var typeInfo = new TypeInfo(typeof(CustomClass));
            var dynamicObject = new DynamicObject(typeInfo);

            dynamicObject.Type.ShouldBe(typeInfo);
        }
    }
}
