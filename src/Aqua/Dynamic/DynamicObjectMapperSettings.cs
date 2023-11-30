// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Dynamic;

using Aqua.TypeSystem;

public class DynamicObjectMapperSettings
{
    /// <summary>
    /// Gets or sets a value indicating whether unasignalbe members should be skipped silenly.
    /// If set to <see langword="true"/> properties which cannot be assigned due to a type mismatch are silently skipped,
    /// if set to <see langword="false"/> no validation will be performed resulting in an exception when trying to assign a property value with an unmatching type.
    /// The default value is <see langword="true"/>.
    /// </summary>
    public bool SilentlySkipUnassignableMembers { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether native values (numeric, datetime, etc.) should be formatted as strings.
    /// If set to <see langword="true"/> all native type values are stored as strings, ohterwise native values get stored with no transformation.
    /// The default value is <see langword="false"/>.
    /// </summary>
    public bool FormatNativeTypesAsString { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether serializable types should be formatted using <see cref="System.Runtime.Serialization.FormatterServices"/>.
    /// The default value is <see langword="true"/>.
    /// </summary>
    public bool UtilizeFormatterServices { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether <see cref="TypeInfo"/> and <see cref="MemberInfo"/> types should be mapped.
    /// Aqua type system types are mapped only is this value is <see langword="false"/>.
    /// The default value is <see langword="true"/>.
    /// </summary>
    public bool PassthroughAquaTypeSystemTypes { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether a cache of mapped objects should be preserved.
    /// Caching improves performance in case of subsequentely mapping the same objects again.
    /// This should be activated for mapping immutable objects only or if otherwise known, objects don't change their state.
    /// The default value is <see langword="false"/>.
    /// </summary>
    public bool PreserveMappingCache { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether null values should be wrapped as <see cref="DynamicObject"/>.
    /// The default value is <see langword="false"/>.
    /// </summary>
    public bool WrapNullAsDynamicObject { get; set; }

    internal DynamicObjectMapperSettings Copy() => (DynamicObjectMapperSettings)MemberwiseClone();
}
