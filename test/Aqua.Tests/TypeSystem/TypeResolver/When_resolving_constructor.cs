// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Tests.TypeSystem.TypeResolver;

using Aqua.TypeSystem;
using Shouldly;
using Xunit;
using ConstructorInfo = Aqua.TypeSystem.ConstructorInfo;

public class When_resolving_constructor
{
    [Fact]
    public void Of_generic_list()
    {
        var constructorInfo = new ConstructorInfo(typeof(List<string>).GetConstructor(Type.EmptyTypes));
        constructorInfo.ResolveConstructor(new TypeResolver()).GetParameters().ShouldBeEmpty();
    }
}