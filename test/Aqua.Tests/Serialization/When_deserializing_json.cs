// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Tests.Serialization
{
    using Aqua.TypeSystem;
    using global::Newtonsoft.Json;
    using Shouldly;
    using Xunit;

    public class When_deserializing_json
    {
        [Fact]
        public void Type_should_deserialize_with_id_and_type()
        {
            var json = $@"
{{
    ""$id"": ""1"",
    ""$type"": ""Aqua.TypeSystem.TypeInfo, Aqua"",
    ""Name"": ""SomeType"",
    ""Namespace"": ""Name.Space""
}}";

            var typeInfo = Deserialize<TypeInfo>(json);
            typeInfo.Name.ShouldBe("SomeType");
            typeInfo.Namespace.ShouldBe("Name.Space");
        }

        [Fact]
        public void Type_should_deserialize_with_id()
        {
            var json = $@"
{{
    ""$id"": ""1"",
    ""Name"": ""SomeType"",
    ""Namespace"": ""Name.Space""
}}";

            var typeInfo = Deserialize<TypeInfo>(json);
            typeInfo.Name.ShouldBe("SomeType");
            typeInfo.Namespace.ShouldBe("Name.Space");
        }

        [Fact]
        public void Type_should_deserialize_with_type()
        {
            var json = $@"
{{
    ""$type"": ""Aqua.TypeSystem.TypeInfo, Aqua"",
    ""Name"": ""SomeType"",
    ""Namespace"": ""Name.Space""
}}";

            var typeInfo = Deserialize<TypeInfo>(json);
            typeInfo.Name.ShouldBe("SomeType");
            typeInfo.Namespace.ShouldBe("Name.Space");
        }

        [Fact]
        public void Type_should_deserialize()
        {
            var json = $@"
{{
    ""Name"": ""SomeType"",
    ""Namespace"": ""Name.Space""
}}";

            var typeInfo = Deserialize<TypeInfo>(json);
            typeInfo.Name.ShouldBe("SomeType");
            typeInfo.Namespace.ShouldBe("Name.Space");
        }

        private static T Deserialize<T>(string json)
        {
            var serializerSettings = new JsonSerializerSettings().ConfigureAqua();
            return JsonConvert.DeserializeObject<T>(json, serializerSettings);
        }
    }
}
