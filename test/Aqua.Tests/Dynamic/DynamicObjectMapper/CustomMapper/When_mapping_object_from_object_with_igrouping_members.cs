// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Tests.Dynamic.DynamicObjectMapper.CustomMapper;

using Aqua.Dynamic;
using Aqua.TypeExtensions;
using Shouldly;
using System.Collections;
using System.Reflection;
using Xunit;

public class When_mapping_object_from_object_with_igrouping_members
{
    private const BindingFlags PrivateInstance = BindingFlags.NonPublic | BindingFlags.Instance;

    private class CustomClass
    {
        public IGrouping<string, int> Grouping { get; set; }
    }

    private class CustomDynamicObjectMapper : DynamicObjectMapper
    {
        protected override DynamicObject MapToDynamicObjectGraph(object obj, Func<Type, bool> setTypeInformation)
        {
            if (obj is not null && obj.GetType().Implements(typeof(IGrouping<,>)))
            {
                var mappedGrouping = typeof(CustomDynamicObjectMapper)
                    .GetMethod(nameof(MapGrouping), PrivateInstance)
                    .MakeGenericMethod(obj.GetType().GenericTypeArguments)
                    .Invoke(this, [obj, setTypeInformation]);
                return (DynamicObject)mappedGrouping;
            }

            return base.MapToDynamicObjectGraph(obj, setTypeInformation);
        }

        protected override bool ShouldMapToDynamicObject(IEnumerable collection)
        {
            return collection.GetType().Implements(typeof(IGrouping<,>));
        }

        private DynamicObject MapGrouping<TKey, TElement>(IGrouping<TKey, TElement> grouping, Func<Type, bool> setTypeInformation)
        {
            var mappedGrouping = new DynamicObject(grouping.GetType());
            mappedGrouping.Add("Key", MapToDynamicObjectGraph(grouping.Key, setTypeInformation));
            mappedGrouping.Add("Elements", this.MapCollection(grouping, setTypeInformation).ToArray());
            return mappedGrouping;
        }
    }

    private readonly DynamicObject dynamicGrouping;
    private readonly DynamicObject dynamicObject;

    public When_mapping_object_from_object_with_igrouping_members()
    {
        IGrouping<string, int> grouping = Enumerable.Range(1, 5).GroupBy(x => "Hello", x => x).Single();
        var source = new CustomClass
        {
            Grouping = grouping,
        };
        dynamicGrouping = new CustomDynamicObjectMapper().MapObject(grouping);
        dynamicObject = new CustomDynamicObjectMapper().MapObject(source);
    }

    [Fact]
    public void Dynamic_grouping_type_should_be_igrouping()
    {
        dynamicGrouping.Type.ToType().Implements(typeof(IGrouping<,>)).ShouldBeTrue();
    }

    [Fact]
    public void Dynamic_object_grouping_should_be_mapped()
    {
        dynamicObject["Grouping"].ShouldBeOfType<DynamicObject>();
    }

    [Fact]
    public void Dynamic_object_grouping_type_should_be_igrouping()
    {
        ((DynamicObject)dynamicObject["Grouping"]).Type.ToType().Implements(typeof(IGrouping<,>)).ShouldBeTrue();
    }
}