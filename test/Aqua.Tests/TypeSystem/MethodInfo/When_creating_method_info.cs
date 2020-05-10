// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Tests.TypeSystem.MethodInfo
{
    using Aqua.TypeSystem;
    using Shouldly;
    using System;
    using System.Linq;
    using Xunit;
    using BindingFlags = System.Reflection.BindingFlags;

    public class When_creating_method_info
    {
        private class A
        {
            public static string StaticMethod() => null;

            public string Method() => null;
        }

        [Fact]
        public void Should_throw_on_creating_by_memberinfo_with_null_parameter()
        {
            Should.Throw<ArgumentNullException>(() => new MethodInfo((System.Reflection.MethodInfo)null));
        }

        [Fact]
        public void Should_have_set_is_static_for_static_method_info_created_by_memberinfo()
        {
            var method = typeof(A).GetMethod(nameof(A.StaticMethod), BindingFlags.Static | BindingFlags.Public);
            new MethodInfo(method).IsStatic.ShouldBe(true);
        }

        [Fact]
        public void Should_not_have_set_is_static_by_default_when_created_by_memberinfo()
        {
            var method = typeof(A).GetMethod(nameof(A.Method));
            new MethodInfo(method).IsStatic.ShouldBeNull();
        }

        [Fact]
        public void Should_not_have_set_is_static_by_default_when_created_by_name()
        {
            new MethodInfo(nameof(A.Method), typeof(A)).IsStatic.ShouldBeNull();
        }
    }
}
