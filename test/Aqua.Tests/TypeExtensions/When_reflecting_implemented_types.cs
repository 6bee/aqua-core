// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Tests.TypeExtensions
{
    using Aqua.TypeExtensions;
    using Shouldly;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using Xunit;

    public class When_reflecting_implemented_types
    {
        public class TestInterface
        {
        }

        public class TestBaseClass : TestInterface
        {
        }

        public class TestSubClass : TestBaseClass
        {
        }

        public class TestQueryable<T> : IQueryable<T>
        {
            public Expression Expression => throw new NotImplementedException();

            public Type ElementType => throw new NotImplementedException();

            public IQueryProvider Provider => throw new NotImplementedException();

            public IEnumerator<T> GetEnumerator() => throw new NotImplementedException();

            IEnumerator IEnumerable.GetEnumerator() => throw new NotImplementedException();
        }

        [Fact]
        public void Should_reflect_base_interface()
        {
            var result = typeof(TestQueryable<DateTime>).Implements(typeof(IEnumerable));

            result.ShouldBeTrue();
        }

        [Fact]
        public void Should_reflect_base_generic_interface_with_generic_arguments()
        {
            var result = typeof(TestQueryable<DateTime>).Implements(typeof(IEnumerable<>), out var args);

            result.ShouldBeTrue();
            args.ShouldHaveSingleItem().ShouldBe(typeof(DateTime));
        }

        [Fact]
        public void Should_reflect_closed_generic_interface_with_generic_arguments()
        {
            var result = typeof(TestQueryable<DateTime>).Implements(typeof(IEnumerable<DateTime>), out var args);

            result.ShouldBeTrue();
            args.ShouldBeNull();
        }

        [Fact]
        public void Should_reflect_base_generic_interface()
        {
            var result = typeof(TestQueryable<DateTime>).Implements(typeof(IEnumerable<>));

            result.ShouldBeTrue();
        }

        [Fact]
        public void Should_reflect_generic_interface_with_generic_arguments()
        {
            var result = typeof(TestQueryable<DateTime>).Implements(typeof(IQueryable<>), out var args);

            result.ShouldBeTrue();
            args.ShouldHaveSingleItem().ShouldBe(typeof(DateTime));
        }

        [Fact]
        public void Should_reflect_not_implemented_generic_interface_with_generic_arguments()
        {
            var result = typeof(TestQueryable<DateTime>).Implements(typeof(IList<>), out var args);

            result.ShouldBeFalse();
            args.ShouldBeNull();
        }

        [Fact]
        public void Should_reflect_not_implemented_generic_interface()
        {
            var result = typeof(TestQueryable<DateTime>).Implements(typeof(IList<>));

            result.ShouldBeFalse();
        }

        [Fact]
        public void Should_reflect_implemented_interface()
        {
            var result = typeof(TestSubClass).Implements(typeof(TestInterface));

            result.ShouldBeTrue();
        }

        [Fact]
        public void Should_reflect_base_class()
        {
            var result = typeof(TestSubClass).Implements(typeof(TestBaseClass));

            result.ShouldBeTrue();
        }
    }
}