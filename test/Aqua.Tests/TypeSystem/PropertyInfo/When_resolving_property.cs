// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Tests.TypeSystem.PropertyInfo
{
    using Aqua.TypeSystem;
    using Shouldly;
    using Xunit;

    public class When_resolving_property
    {
        private class A
        {
            public string Property { get; set; }
        }

        [Fact]
        public void Should_throw_upon_casting_property_info_for_inexistent_property()
        {
            var propertyInfo = new PropertyInfo("property", typeof(string), typeof(A));
            Should.Throw<TypeResolverException>(() =>
            {
                _ = (System.Reflection.PropertyInfo)propertyInfo;
            }).Message.ShouldBe("Failed to resolve property, consider using extension method to specify ITypeResolver.");
        }

        [Fact]
        public void Should_resolve_property()
        {
            var propertyInfo = new PropertyInfo("Property", typeof(string), typeof(A));
            var property = (System.Reflection.PropertyInfo)propertyInfo;
            property.ShouldBeSameAs(typeof(A).GetProperty(nameof(A.Property)));
        }
    }
}
