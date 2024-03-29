﻿// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Tests.TypeSystem.TypeInfo;

using Aqua.TypeSystem;
using Shouldly;
using System.Linq;
using Xunit;

public class When_creating_type_info_of_circular_type_graph
{
    private class A
    {
        public B B { get; set; }
    }

    private class B
    {
        public A A { get; set; }
    }

    private readonly TypeInfo typeInfo;

    public When_creating_type_info_of_circular_type_graph()
    {
        typeInfo = new TypeInfo(typeof(A));
    }

    [Fact]
    public void Type_should_be_A()
    {
        typeInfo.Name.ShouldBe("A");
    }

    [Fact]
    public void Type_A_should_have_property_of_type_B()
    {
        typeInfo.Properties.Single().PropertyType.Name.ShouldBe("B");
    }

    [Fact]
    public void Type_B_should_have_property_of_type_A()
    {
        typeInfo.Properties.Single().PropertyType.Properties.Single().PropertyType.ShouldBeSameAs(typeInfo);
    }
}