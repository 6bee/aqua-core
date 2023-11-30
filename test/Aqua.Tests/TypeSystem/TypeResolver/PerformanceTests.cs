// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Tests.TypeSystem.TypeResolver
{
    using Aqua.TypeSystem;
    using System;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using Xunit;

    public class PerformanceTests
    {
        [Fact]
        [SuppressMessage("Blocker Code Smell", "S2699:Tests should include assertions", Justification = "Speed test")]
        public void Performance_test_for_resolving_nested_anonymous_types()
        {
            Type GenerateAnonymousType<T>(uint nestingCount, T value)
            {
                if (nestingCount is 0)
                {
                    return null;
                }

                var newValue = new { Prop = value };
                return GenerateAnonymousType(nestingCount - 1, newValue) ?? newValue.GetType();
            }

            for (uint i = 15; i <= 20; ++i)
            {
                var type = GenerateAnonymousType(i, "hello");
                var typeInfo1 = new TypeInfo(type);
                var typeInfo2 = new TypeInfo(type);
                var typeResolver = new TypeResolver();

                var watch = Stopwatch.StartNew();
                typeResolver.ResolveType(typeInfo1);
                typeResolver.ResolveType(typeInfo2);
                watch.Stop();

                Debug.WriteLine($"{i} | {watch.ElapsedMilliseconds}");
            }
        }
    }
}
