// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Tests.Dynamic.DynamicObject
{
    using Aqua.Dynamic;
    using System.Linq;
    using Xunit;
    using Xunit.Fluent;

    public class When_created_based_on_object_with_abstract_properties_with_type_information
    {
        abstract class BaseA
        {   
        }

        class A : BaseA
        {
        }

        class ClassWithAbstractProperties
        {
            public BaseA Ref { get; set; }
            public object Value1 { get; set; }
            public object Value2 { get; set; }
            public object Value3 { get; set; }
            public object Value4 { get; set; }
        }

        ClassWithAbstractProperties obj;

        DynamicObject dynamicObject;

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

            dynamicObject.MemberCount.ShouldBe(5);

            dynamicObject.MemberNames.ElementAt(0).ShouldBe("Ref");
            dynamicObject.MemberNames.ElementAt(1).ShouldBe("Value1");
            dynamicObject.MemberNames.ElementAt(2).ShouldBe("Value2");
            dynamicObject.MemberNames.ElementAt(3).ShouldBe("Value3");
            dynamicObject.MemberNames.ElementAt(4).ShouldBe("Value4");

            dynamicObject["Ref"].ShouldBeOfType<DynamicObject>();
            ((DynamicObject)dynamicObject["Ref"]).MemberCount.ShouldBe(0);
            ((DynamicObject)dynamicObject["Ref"]).Type.Type.ShouldBe(typeof(A));

            dynamicObject["Value1"].ShouldBe(obj.Value1);
            dynamicObject["Value2"].ShouldBe(obj.Value2);

            dynamicObject["Value3"].ShouldBeOfType<DynamicObject>();
            ((DynamicObject)dynamicObject["Value3"]).MemberCount.ShouldBe(0);
            ((DynamicObject)dynamicObject["Value3"]).Type.Type.ShouldBe(typeof(object));

            dynamicObject["Value4"].ShouldBeOfType<byte[]>();
            ((byte[])dynamicObject["Value4"]).Length.ShouldBe(4);
            ((byte[])dynamicObject["Value4"])[0].ShouldBe((byte)1);
            ((byte[])dynamicObject["Value4"])[1].ShouldBe((byte)22);
            ((byte[])dynamicObject["Value4"])[2].ShouldBe((byte)0);
            ((byte[])dynamicObject["Value4"])[3].ShouldBe((byte)44);
        }
    }
}
