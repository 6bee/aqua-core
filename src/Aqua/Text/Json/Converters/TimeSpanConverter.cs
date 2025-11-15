// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Text.Json.Converters;

using System.Text.Json;
using System.Text.Json.Serialization;

/// <summary>
/// Allows to rountrip <see cref="TimeSpan"/>, values formatted as text within json.
/// Values are formatted using the
/// <see href="https://docs.microsoft.com/en-us/dotnet/standard/base-types/standard-timespan-format-strings#the-constant-c-format-specifier">constant ("c") format specifier</see>.
/// </summary>
/// <remarks>
/// This converter needs to be added explicitely if required,
/// i.e. it does not get added by <see cref="JsonSerializerOptionsExtensions.ConfigureAqua(JsonSerializerOptions)"/>
/// or any of its overloads.
/// </remarks>
public class TimeSpanConverter : JsonConverter<TimeSpan>
{
    public override void Write(Utf8JsonWriter writer, TimeSpan value, JsonSerializerOptions options)
        => writer.WriteStringValue(value.ToString("c"));

    public override TimeSpan Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType is JsonTokenType.String)
        {
            var value = reader.GetString();
            return TimeSpan.TryParse(value, out var timespan)
                ? timespan
                : throw reader.CreateException($"Failed to parse timespan '{timespan}'");
        }

        throw reader.CreateException("Expected string.");
    }
}