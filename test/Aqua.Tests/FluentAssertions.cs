// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Tests;

using System.Diagnostics.CodeAnalysis;
using System.Reflection;

public static class FluentAssertions
{
    public static void ShouldBeAnnotatedWith<T>(this Type type)
        where T : Attribute
    {
        if (!type.GetTypeInfo().IsDefined(typeof(T)))
        {
            throw new ExpectedAnnotationException(type, typeof(T));
        }
    }

    [SuppressMessage("Design", "CA1032:Implement standard exception constructors", Justification = "Used in private scope only")]
    private sealed class ExpectedAnnotationException : Xunit.Sdk.XunitException
    {
        public ExpectedAnnotationException(Type type, Type attributeType)
            : base(null)
        {
            Type = type;
            AttributeType = attributeType;
        }

        public Type Type { get; }

        public Type AttributeType { get; }

        public override string Message
        {
            get
            {
                var n = Environment.NewLine;
                return $"Missing custom attribute annotation {n}" +
                    $"Type: {Type.Name} {n}" +
                    $"Expected: {AttributeType.Name} {n}" +
                    $"Found: {n}" +
                    $"{string.Join(n, Type.GetTypeInfo().GetCustomAttributes().Select(_ => "- " + _.GetType().Name))}";
            }
        }

        public override string ToString() => Message;
    }

    public static T With<T>(this T t, Action<T> assertion)
    {
        assertion(t);
        return t;
    }
}