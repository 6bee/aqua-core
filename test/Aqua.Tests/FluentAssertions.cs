// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Tests
{
    using System;
    using System.Linq;
    using System.Reflection;

    public static class FluentAssertions
    {
        public static void ShouldBeAnnotatedWith<T>(this Type type) where T : Attribute
        {
            if (!type.GetTypeInfo().IsDefined(typeof(T)))
            {
                throw new ExpectedAnnotation(type, typeof(T));
            }
        }

        private class ExpectedAnnotation : Xunit.Sdk.XunitException
        {
            public ExpectedAnnotation(Type type, Type attributeType)
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
                    return $"Missing custom attribute annotation {Environment.NewLine}Type: {Type.Name} {Environment.NewLine}Expected: {AttributeType.Name} {Environment.NewLine}Found: {Environment.NewLine}{string.Join(Environment.NewLine, Type.GetTypeInfo().GetCustomAttributes().Select(_ => "- " + _.GetType().Name))}";
                }
            }

            public override string ToString()
            {
                return Message;
            }
        }

        public static T With<T>(this T t, Action<T> assertion)
        {
            assertion(t);
            return t;
        }
    }
}