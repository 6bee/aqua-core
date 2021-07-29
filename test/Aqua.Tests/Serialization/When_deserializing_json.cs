// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Tests.Serialization
{
    using Aqua.TypeSystem;
    using Shouldly;
    using System;
    using Xunit;

    public abstract class When_deserializing_json
    {
        public class With_system_text_json_serializer : When_deserializing_json
        {
            protected override T Deserialize<T>(string json)
            {
                var serializerOptions = new System.Text.Json.JsonSerializerOptions()
                    .AddConverter(new Aqua.Text.Json.Converters.TimeSpanConverter())
                    .ConfigureAqua();
                return System.Text.Json.JsonSerializer.Deserialize<T>(json, serializerOptions);
            }
        }

        public class With_newtonsoft_json_serializer : When_deserializing_json
        {
            protected override T Deserialize<T>(string json)
            {
                var serializerSettings = new global::Newtonsoft.Json.JsonSerializerSettings().ConfigureAqua();
                return global::Newtonsoft.Json.JsonConvert.DeserializeObject<T>(json, serializerSettings);
            }
        }

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

        [Fact]
        public void Should_deserialize_timespan()
        {
            var timestamp = DateTime.Now - new DateTime(DateTime.Now.Year, 1, 1);

            var json = @$"""{timestamp:c}""";

            var copy = Deserialize<TimeSpan>(json);
            copy.ShouldBe(timestamp);
        }

        protected abstract T Deserialize<T>(string json);
    }
}
