// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Tests.Dynamic.DynamicObject
{
    using Aqua.Dynamic;
    using Shouldly;
    using System.Linq;
    using Xunit;

    public abstract class When_created_using_copyconstructor
    {
        public class With_deep_copy : When_created_using_copyconstructor
        {
            public With_deep_copy()
                : base(true)
            {
            }
        }

        public class With_shallow_copy : When_created_using_copyconstructor
        {
            public With_shallow_copy()
                : base(false)
            {
            }
        }

        protected When_created_using_copyconstructor(bool deepCopy)
        {
            var properties = new PropertySet
            {
                ("K1", 1),
                ("K2", 2),
            };
            Source = new DynamicObject(properties);
            Copy = new DynamicObject(Source, deepCopy);
            DeepCopy = deepCopy;
        }

        private DynamicObject Source { get; }

        private DynamicObject Copy { get; }

        private bool DeepCopy { get; }

        [Fact]
        public void Setting_deep_copy_to_false_should_share_properties()
        {
            var sourceProperties = Source.Properties.ToArray();
            var copyProperties = Copy.Properties.ToArray();
            for (int i = 0; i < sourceProperties.Length; i++)
            {
                var sourceProperty = sourceProperties[i];
                var copyProperty = copyProperties[i];

                if (DeepCopy)
                {
                    copyProperty.ShouldNotBeSameAs(sourceProperty);
                }
                else
                {
                    copyProperty.ShouldBeSameAs(sourceProperty);
                }
            }
        }

        [Fact]
        public void Setting_deep_copy_to_false_should_not_share_propertyset()
        {
            Copy.Properties.ShouldNotBeSameAs(Source.Properties);
        }
    }
}