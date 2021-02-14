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
        ////public class With_type : When_creating_default
        ////{
        ////    public With_type()
        ////        : base(DynamicObject.CreateDefault(typeof(int)))
        ////    {
        ////    }

        ////    public override Type Type => typeof(int);
        ////}

        private const BindingFlags PrivateStatic = BindingFlags.NonPublic | BindingFlags.Static;
        private static readonly MethodInfo _createDefaultMethodInfo = typeof(When_creating_default).GetMethod(nameof(CreateDefault), PrivateStatic);

        ////private readonly DynamicObject _dynamicObject;

        ////protected When_creating_default(DynamicObject dynamicObject)
        ////    => _dynamicObject = dynamicObject;

        ////public abstract Type Type { get; }

        ////public virtual object ExpectedDefault => CreateDefaultForType(Type);

        ////[Fact]
        [Theory]
        [MemberData(nameof(TestData.Types), MemberType = typeof(TestData))]
        public void Should_have_isnull_true(Type type)
        {
            var dynamicObject = DynamicObject.CreateDefault(type);

            dynamicObject.IsNull.ShouldBeTrue();
        }

        ////[Fact]
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
