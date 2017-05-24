// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Tests.TypeSystem.TypeResolver
{
    using Aqua.TypeSystem;
    using System.Diagnostics;
    using Xunit;

    public class PerformanceTests
    {
        [Fact]
        public void Performance_test_for_resolving_of_nested_anonymous_types()
        {
            TypeInfo GenerateAnonymousType<T>(uint nestingCount, T value)
            {
                if (nestingCount == 0)
                    return null;

                var newValue = new { Prop = value };
                return GenerateAnonymousType(nestingCount - 1, newValue) ?? new TypeInfo(newValue.GetType());
            }

            for (uint i = 15; i <= 30; ++i)
            {
                TypeInfo type = GenerateAnonymousType(i, "hello");
                var typeResolver = new TypeResolver();

                var watch = Stopwatch.StartNew();
                typeResolver.ResolveType(type);
                watch.Stop();

                Debug.WriteLine($"{i} | {watch.ElapsedMilliseconds}");
            }
        }
    }
}
