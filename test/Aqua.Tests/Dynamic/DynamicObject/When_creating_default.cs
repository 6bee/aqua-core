// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Tests.Dynamic.DynamicObject
{
    using Aqua.Dynamic;
    using Shouldly;
    using System;
    using System.Reflection;
    using Xunit;

    public class When_creating_default
    {
        private const BindingFlags PrivateStatic = BindingFlags.NonPublic | BindingFlags.Static;
        private static readonly MethodInfo _createDefaultMethodInfo = typeof(When_creating_default).GetMethod(nameof(CreateDefault), PrivateStatic);

        [Theory]
        [MemberData(nameof(TestData.Types), MemberType = typeof(TestData))]
        public void Should_have_isnull_true(Type type)
        {
            var dynamicObject = DynamicObject.CreateDefault(type);

            dynamicObject.IsNull.ShouldBeTrue();
        }

        [Theory]
        [MemberData(nameof(TestData.Types), MemberType = typeof(TestData))]
        public void Mapper_should_procude_expected_default(Type type)
        {
            var dynamicObject = DynamicObject.CreateDefault(type);
            var expectedDefault = CreateDefaultForType(type);

            var mapperResult = new DynamicObjectMapper().Map(dynamicObject);

            mapperResult.ShouldBe(expectedDefault);
        }

        private static object CreateDefaultForType(Type type)
            => _createDefaultMethodInfo.MakeGenericMethod(type).Invoke(null, null);

        private static object CreateDefault<T>()
            => default(T);
    }
}
