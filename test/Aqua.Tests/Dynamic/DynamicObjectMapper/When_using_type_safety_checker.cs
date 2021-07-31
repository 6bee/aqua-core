// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Tests.Dynamic.DynamicObjectMapper
{
    using Aqua.Dynamic;
    using Shouldly;
    using System;
    using Xunit;

    public class When_using_type_safety_checker
    {
        public class SafeType
        {
        }

        public class UnsafeType
        {
        }

        public class TypeSafetyChecker : ITypeSafetyChecker
        {
            public void AssertTypeSafety(Type type)
            {
                if (type is null)
                {
                    throw new ArgumentNullException(nameof(type));
                }

                if (typeof(UnsafeType).IsAssignableFrom(type))
                {
                    throw new TestException("Unsafe type detected.");
                }
            }
        }

        [Fact]
        public void Should_throw_on_mapping_from_unsafe_type()
        {
            var mapper = CreateMapper();
            var dynamicObject = mapper.MapObject(new UnsafeType());

            var ex = Should.Throw<DynamicObjectMapperException>(() => mapper.Map(dynamicObject));
            ex.Message.ShouldBe("Unsafe type detected.");
        }

        [Fact]
        public void Should_map_from_safe_type()
        {
            var mapper = CreateMapper();
            var dynamicObject = mapper.MapObject(new SafeType());

            var obj = mapper.Map(dynamicObject);
            obj.ShouldBeOfType<SafeType>();
        }

        private static DynamicObjectMapper CreateMapper()
            => new DynamicObjectMapper(typeSafetyChecker: new TypeSafetyChecker());
    }
}
