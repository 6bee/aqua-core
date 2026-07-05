// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace MessagePack;

using Aqua.MessagePack;
#pragma warning restore IDE0130 // Namespace does not match folder structure

using System.ComponentModel;

/// <summary>
/// Extension methods to configure <see cref="MessagePackSerializerOptions"/> for serializing
/// <i>Aqua</i> types with type-safe formatters.
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public static class MessagePackSerializerOptionsExtensions
{
    extension(MessagePackSerializerOptions options)
    {
        /// <summary>
        /// Returns a copy of the <see cref="MessagePackSerializerOptions"/> configured to serialize
        /// <i>Aqua</i> types using type-safe formatters, with <see cref="MessagePackSecurity.UntrustedData"/> applied.
        /// </summary>
        /// <returns>A configured <see cref="MessagePackSerializerOptions"/> instance.</returns>
        public MessagePackSerializerOptions ConfigureAqua()
        {
            options.AssertNotNull();
            return options
                .WithResolver(AquaFormatterResolver.Instance)
                .WithSecurity(MessagePackSecurity.UntrustedData);
        }
    }
}
