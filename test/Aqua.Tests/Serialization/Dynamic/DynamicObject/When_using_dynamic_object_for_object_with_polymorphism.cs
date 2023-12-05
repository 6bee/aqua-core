// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Tests.Serialization.Dynamic.DynamicObject;

using Aqua.Dynamic;
using Aqua.Text.Json;
using Shouldly;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using Xunit;

public abstract class When_using_dynamic_object_for_object_with_polymorphism
{
    public class With_newtown_json_serializer : When_using_dynamic_object_for_object_with_polymorphism
    {
        public With_newtown_json_serializer()
            : base(NewtonsoftJsonSerializationHelper.Clone)
        {
        }
    }

    public class With_system_text_json_serializer : When_using_dynamic_object_for_object_with_polymorphism
    {
        public With_system_text_json_serializer()
            : base(SystemTextJsonSerializationHelper.Clone)
        {
        }
    }

    public class With_system_text_json_serializer_with_polymorphism_options : When_using_dynamic_object_for_object_with_polymorphism
    {
        public With_system_text_json_serializer_with_polymorphism_options()
            : base(static x => SystemTextJsonSerializationHelper.Clone(x, CreateOptions()))
        {
        }

        private static JsonSerializerOptions CreateOptions()
        {
            return new JsonSerializerOptions
            {
                WriteIndented = true,
                ReferenceHandler = ReferenceHandler.Preserve,
                TypeInfoResolver = new DefaultJsonTypeInfoResolver
                {
                    Modifiers = { AddPolymorphy },
                },
            }.ConfigureAqua();

            static void AddPolymorphy(JsonTypeInfo typeInfo)
            {
                if (typeInfo.Type == typeof(IInterfaceForStructs))
                {
                    typeInfo.PolymorphismOptions ??= new();
                    typeInfo.PolymorphismOptions.IgnoreUnrecognizedTypeDiscriminators = true;
                    typeInfo.PolymorphismOptions.UnknownDerivedTypeHandling = JsonUnknownDerivedTypeHandling.FallBackToNearestAncestor;
                    typeInfo.PolymorphismOptions.DerivedTypes.Add(new JsonDerivedType(typeof(StructA), "Struct-A"));
                    typeInfo.PolymorphismOptions.DerivedTypes.Add(new JsonDerivedType(typeof(StructB), "Struct-B"));
                }
            }
        }
    }

    public interface IInterfaceForStructs
    {
        int Value { get; init; }
    }

    public readonly struct StructA : IInterfaceForStructs
    {
        public int Value { get; init; }
    }

    public readonly struct StructB : IInterfaceForStructs
    {
        public int Value { get; init; }
    }

    public class ObjectWithStruct
    {
        public IInterfaceForStructs SomeStruct { get; init; }
    }

    private sealed class IsKnownTypeProvider : IIsKnownTypeProvider
    {
        public bool IsKnownType(Type type)
            => type == typeof(StructA)
            || type == typeof(StructB);
    }

    private const int Value = 12;
    private readonly ObjectWithStruct serializedObject;

    protected When_using_dynamic_object_for_object_with_polymorphism(Func<DynamicObject, DynamicObject> serialize)
    {
        var originalObject = new ObjectWithStruct { SomeStruct = new StructB { Value = Value } };

        var dynamicObject = new DynamicObjectMapper(isKnownTypeProvider: new IsKnownTypeProvider()).MapObject(originalObject);

        var serializedDynamicObject = serialize(dynamicObject);

        serializedObject = new DynamicObjectMapper(isKnownTypeProvider: new IsKnownTypeProvider()).Map<ObjectWithStruct>(serializedDynamicObject);
    }

    [Fact]
    public void Should_map_back_to_original_type()
    {
        serializedObject
            .SomeStruct.ShouldBeOfType<StructB>()
            .Value.ShouldBe(Value);
    }
}