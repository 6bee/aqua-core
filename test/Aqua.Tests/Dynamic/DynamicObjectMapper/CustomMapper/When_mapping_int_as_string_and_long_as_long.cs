// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Tests.Dynamic.DynamicObjectMapper.CustomMapper
{
    using Aqua.Dynamic;
    using System;
    using System.Linq;
    using System.Collections.Generic;
    using Xunit;
    using Shouldly;

    public class When_mapping_int_as_string_and_long_as_long
    {
        class CustomMapper : DynamicObjectMapper
        {
            protected override object MapFromDynamicObjectGraph(object obj, Type targetType)
            {
                if (targetType == typeof(int))
                {
                    var value = ((DynamicObject)obj)["Value"];
                    return int.Parse((string)value);
                }

                if (targetType == typeof(long))
                {
                    var value = ((DynamicObject)obj)["Value"];
                    return (long)value;
                }

                throw new NotSupportedException();
            }

            protected override DynamicObject MapToDynamicObjectGraph(object obj, Func<Type, bool> setTypeInformation)
            {
                if (obj is int)
                {
                    return new DynamicObject(typeof(int))
                    {
                        Properties = new PropertySet
                        {
                            { "Value", obj.ToString() }
                        }
                    };
                }
                if (obj is long)
                {
                    return new DynamicObject(typeof(long))
                    {
                        Properties = new PropertySet
                        {
                            { "Value", obj }
                        }
                    };
                }

                throw new NotSupportedException();
            }
        }

        DynamicObject dynamicObjectWithInt;
        DynamicObject dynamicObjectWithLong;

        public When_mapping_int_as_string_and_long_as_long()
        {
            var dynamicObjectMapper = new CustomMapper();

            dynamicObjectWithInt = dynamicObjectMapper.MapObject(123);
            dynamicObjectWithLong = dynamicObjectMapper.MapObject(456L);
        }

        [Fact]
        public void Dynamic_object_for_int_should_contain_string_value()
        {
            dynamicObjectWithInt.Values.Single().ShouldBeOfType<string>();
        }

        [Fact]
        public void Dynamic_object_for_lonb_should_contain_string_value()
        {
            dynamicObjectWithLong.Values.Single().ShouldBeOfType<long>();
        }
    }
}
