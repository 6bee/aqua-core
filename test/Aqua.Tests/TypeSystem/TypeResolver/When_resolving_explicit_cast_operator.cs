// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Tests.TypeSystem.TypeResolver;

using Aqua.TypeSystem;
using Shouldly;
using System;
using System.Linq.Expressions;
using Xunit;

public class When_resolving_explicit_cast_operator
{
    [Fact]
    public void ResolveMethod_ExplicitCast_should_return_original_method_info()
    {
        var typeResolver = new TypeResolver();

        Expression<Func<decimal?, double?>> expr = x => (double?)x;
        System.Reflection.MethodInfo methodInfo = ((UnaryExpression)expr.Body).Method;
        Aqua.TypeSystem.MethodInfo mappedMethod = new Aqua.TypeSystem.MethodInfo(methodInfo);
        System.Reflection.MethodInfo resolvedMethod = mappedMethod.ResolveMethod(typeResolver);

        resolvedMethod.ShouldBe(methodInfo);
    }
}