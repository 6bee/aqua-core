// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Tests.Serialization.Dynamic.DynamicObject;

using Aqua.Dynamic;
using Shouldly;
using System;
using Xunit;

public abstract class When_using_dynamic_object_for_object_with_polymorphism
{
    ////#if !NET8_0_OR_GREATER
    ////    public class With_binary_formatter : When_using_dynamic_object_for_object_with_polymorphism
    ////    {
    ////        public With_binary_formatter()
    ////            : base(BinarySerializationHelper.Clone)
    ////        {
    ////        }
    ////    }

    ////#endif // NET8_0_OR_GREATER

    ////    public class With_data_contract_serializer : When_using_dynamic_object_for_object_with_polymorphism
    ////    {
    ////        public With_data_contract_serializer()
    ////            : base(DataContractSerializationHelper.Clone)
    ////        {
    ////        }
    ////    }

    ////    public class With_newtown_json_serializer : When_using_dynamic_object_for_object_with_polymorphism
    ////    {
    ////        public With_newtown_json_serializer()
    ////            : base(NewtonsoftJsonSerializationHelper.Clone)
    ////        {
    ////        }
    ////    }

    public class With_system_text_json_serializer : When_using_dynamic_object_for_object_with_polymorphism
    {
        public With_system_text_json_serializer()
            : base(SystemTextJsonSerializationHelper.Clone)
        {
        }
    }

    ////#if NETFRAMEWORK
    ////    public class With_net_data_contract_serializer : When_using_dynamic_object_for_object_with_polymorphism
    ////    {
    ////        public With_net_data_contract_serializer()
    ////            : base(NetDataContractSerializationHelper.Clone)
    ////        {
    ////        }
    ////    }
    ////#endif // NETFRAMEWORK

    ////    public class With_protobuf_net_serializer : When_using_dynamic_object_for_object_with_polymorphism
    ////    {
    ////        public With_protobuf_net_serializer()
    ////            : base(ProtobufNetSerializationHelper.Clone)
    ////        {
    ////        }
    ////    }

    ////    public class With_xml_serializer : When_using_dynamic_object_for_object_with_polymorphism
    ////    {
    ////        public With_xml_serializer()
    ////            : base(XmlSerializationHelper.Serialize)
    ////        {
    ////        }
    ////    }

    private sealed class IsKnownTypeProvider : IIsKnownTypeProvider
    {
        public bool IsKnownType(Type type)
            => type == typeof(E); // if not know type, enum value would be mapped as string
    }

    public enum E
    {
        A,
        B,
    }

    public class X
    {
        public object Y { get; init; }
    }

    public class Y
    {
        public object N { get; init; }

        public object E { get; init; }
    }

    private const float FloatValue = 1.2345679e-8f;
    private const E EnumValue = E.B;

    private readonly DynamicObject serializedObject;

    protected When_using_dynamic_object_for_object_with_polymorphism(Func<DynamicObject, DynamicObject> serialize)
    {
        var originalObject = new X
        {
            Y = new Y
            {
                N = FloatValue,
                E = EnumValue,
            },
        };

        var dynamicObject = new DynamicObjectMapper(isKnownTypeProvider: new IsKnownTypeProvider()).MapObject(originalObject);

        serializedObject = serialize(dynamicObject);
    }

    [Fact]
    public void Should_map_back_to_original_type()
    {
        var copy = new DynamicObjectMapper().Map(serializedObject);
        var x = copy.ShouldBeOfType<X>();
        var i = x.Y.ShouldBeOfType<Y>();
        i.N.ShouldBe(FloatValue);
        i.E.ShouldBe(EnumValue);
    }
}