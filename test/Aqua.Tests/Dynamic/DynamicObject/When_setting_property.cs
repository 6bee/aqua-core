// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Tests.Dynamic.DynamicObject
{
    using Aqua.Dynamic;
    using Shouldly;
    using Xunit;

    public class When_setting_property
    {
        private class DynamicObjectTest : DynamicObject
        {
            public DynamicObjectTest()
            {
                PropertyChanging += (sender, args) => PropertyChangingCount++;
                PropertyChanged += (sender, args) => PropertyChangedCount++;
            }

            public int PropertyChangingCount { get; private set; } = 0;

            public int PropertyChangedCount { get; private set; } = 0;

            public int OnPropertyChangingCount { get; private set; } = 0;

            public int OnPropertyChangedCount { get; private set; } = 0;

            protected override void OnPropertyChanged(string name, object oldValue, object newValue)
            {
                OnPropertyChangedCount++;

                this[name].ShouldBe(NewValue);

                var eventCount = PropertyChangedCount;

                base.OnPropertyChanged(name, oldValue, newValue);

                PropertyChangedCount.ShouldBe(eventCount + 1);

                this[name].ShouldBe(NewValue);
            }

            protected override void OnPropertyChanging(string name, object oldValue, object newValue)
            {
                OnPropertyChangingCount++;

                this[name].ShouldBe(OldValue);

                var eventCount = PropertyChangingCount;

                base.OnPropertyChanging(name, oldValue, newValue);

                PropertyChangingCount.ShouldBe(eventCount + 1);

                this[name].ShouldBe(OldValue);
            }
        }

        private const string OldValue = "OldValue";
        private const string NewValue = "NewValue";
        private const string StringProperty = "StringProperty";

        private DynamicObjectTest dynamicObject;

        public When_setting_property()
        {
            dynamicObject = new DynamicObjectTest
            {
                Properties = new PropertySet
                {
                    { StringProperty, OldValue },
                },
            };

            dynamicObject.PropertyChangingCount.ShouldBe(0);
            dynamicObject.PropertyChangedCount.ShouldBe(0);
            dynamicObject.OnPropertyChangingCount.ShouldBe(0);
            dynamicObject.OnPropertyChangedCount.ShouldBe(0);
        }

        [Fact]
        public void Should_invoke_virtual_methods_and_eventhandlers_when_property_set_via_indexer()
        {
            dynamicObject[StringProperty] = NewValue;

            dynamicObject.PropertyChangingCount.ShouldBe(1);
            dynamicObject.PropertyChangedCount.ShouldBe(1);
            dynamicObject.OnPropertyChangingCount.ShouldBe(1);
            dynamicObject.OnPropertyChangedCount.ShouldBe(1);
        }

        [Fact]
        public void Should_invoke_virtual_methods_and_eventhandlers_when_property_set_via_set_method()
        {
            dynamicObject.Set(StringProperty, NewValue);

            dynamicObject.PropertyChangingCount.ShouldBe(1);
            dynamicObject.PropertyChangedCount.ShouldBe(1);
            dynamicObject.OnPropertyChangingCount.ShouldBe(1);
            dynamicObject.OnPropertyChangedCount.ShouldBe(1);
        }

        [Fact]
        public void Should_not_invoke_virtual_methods_and_eventhandlers_when_adding_property()
        {
            dynamicObject.Add("NewStringProperty", NewValue);

            dynamicObject.PropertyChangingCount.ShouldBe(0);
            dynamicObject.PropertyChangedCount.ShouldBe(0);
            dynamicObject.OnPropertyChangingCount.ShouldBe(0);
            dynamicObject.OnPropertyChangedCount.ShouldBe(0);
        }

        [Fact]
        public void Should_not_invoke_virtual_methods_and_eventhandlers_when_removing_property()
        {
            dynamicObject.Remove("NewStringProperty");

            dynamicObject.PropertyChangingCount.ShouldBe(0);
            dynamicObject.PropertyChangedCount.ShouldBe(0);
            dynamicObject.OnPropertyChangingCount.ShouldBe(0);
            dynamicObject.OnPropertyChangedCount.ShouldBe(0);
        }
    }
}