// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Tests.Text.Json.JsonSerializerOptionsExtensions;

using System.Text.Json;

public sealed class When_creating_json_seriliazer_options_for_aqua
{
    [Fact]
    public void Should_set_reference_handler()
    {
        var defaultOptions = new JsonSerializerOptions();
        defaultOptions.ReferenceHandler.ShouldBeNull();

        var aquaOptions = defaultOptions.ConfigureAqua();
        aquaOptions.ReferenceHandler.ShouldNotBeNull();
    }
}