// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Tests.Dynamic.DynamicObject
{
    using Aqua.Dynamic;
    using Shouldly;
    using System.Linq;
    using Xunit;

    public class When_created_based_on_object_with_abstract_properties_with_type_information
    {
        private abstract class BaseA
        {
        }

        private class A : BaseA
        {
        }

        private class ClassWithAbstractProperties
        {
            public BaseA Ref { get; set; }

            public object Value1 { get; set; }

            public object Value2 { get; set; }

            public object Value3 { get; set; }

            public object Value4 { get; set; }
        }

        private readonly ClassWithAbstractProperties obj;

        private readonly DynamicObject dynamicObject;

        public When_created_based_on_object_with_abstract_properties_with_type_information()
        {
            obj = new ClassWithAbstractProperties()
            {
                Ref = new A(),
                Value1 = "the value's pay load",
                Value2 = 222,
                Value3 = new object(),
                Value4 = new byte[] { 1, 22, 0, 44 },
            };

            var mapper = new DynamicObjectMapper();

            dynamicObject = mapper.MapObject(obj);
        }

        [Fact]
        public void Should_recreate_object_with_original_values()
        {
            dynamicObject.ShouldNotBeNull();

            dynamicObject.PropertyCount.ShouldBe(5);

            dynamicObject.PropertyNames.ElementAt(0).ShouldBe("Ref");
            dynamicObject.PropertyNames.ElementAt(1).ShouldBe("Value1");
            dynamicObject.PropertyNames.ElementAt(2).ShouldBe("Value2");
            dynamicObject.PropertyNames.ElementAt(3).ShouldBe("Value3");
            dynamicObject.PropertyNames.ElementAt(4).ShouldBe("Value4");

            var refObj = dynamicObject["Ref"].ShouldBeOfType<DynamicObject>();
            refObj.PropertyCount.ShouldBe(0);
            refObj.Type.Type.ShouldBe(typeof(A));

            dynamicObject["Value1"].ShouldBe(obj.Value1);
            dynamicObject["Value2"].ShouldBe(obj.Value2);

            var value3 = dynamicObject["Value3"].ShouldBeOfType<DynamicObject>();
            value3.PropertyCount.ShouldBe(0);
            value3.Type.Type.ShouldBe(typeof(object));

            var bytes = dynamicObject["Value4"].ShouldBeOfType<byte[]>();
            bytes.Length.ShouldBe(4);
            bytes[0].ShouldBe((byte)1);
            bytes[1].ShouldBe((byte)22);
            bytes[2].ShouldBe((byte)0);
            bytes[3].ShouldBe((byte)44);
        }
    }
}
