// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Tests.TypeSystem.Emit.TypeEmitter;

using Aqua.TypeSystem.Emit;
using Shouldly;
using System.Collections.Generic;
using Xunit;
using PropertyInfo = Aqua.TypeSystem.PropertyInfo;
using TypeInfo = Aqua.TypeSystem.TypeInfo;

public class When_emitting_type_with_circular_reference
{
    [Fact]
    public void Should_throw()
    {
        var typeInfo = new TypeInfo
        {
            Name = "TestClass",
            Namespace = "TestNamespace",
        };

        typeInfo.Properties = new List<PropertyInfo>
        {
            new PropertyInfo("CircularReference", typeInfo, typeInfo),
        };

        var ex = Should.Throw<TypeEmitterException>(() => new TypeEmitter().EmitType(typeInfo));
        ex.Message.ShouldBe("Cannot emit type with circular reference: 'TestNamespace.TestClass'");
    }
}
