﻿// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Dynamic
{
    using Aqua.TypeSystem;

    public class DynamicObjectMapperSettings
    {
#pragma warning disable SA1629 // Documentation text should end with a period
        /// <summary>
        /// Gets or sets a value indicating whether unasignalbe members should be skipped silenly.
        /// If set to true properties which cannot be assigned due to a type mismatch are silently skipped,
        /// if set to false no validation will be performed resulting in an exception when trying to assign a property value with an unmatching type.
        /// The default value is <code>true</code>.
        /// </summary>
        public bool SilentlySkipUnassignableMembers { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether native values (numeric, datetime, etc.) should be formatted as strings.
        /// If set to true all native type values are stored as strings, ohterwise native values get stored with no transformation.
        /// The default value is <code>false</code>.
        /// </summary>
        public bool FormatNativeTypesAsString { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether serializable types should be formatted using <see cref="System.Runtime.Serialization.FormatterServices"/>.
        /// The default value is <code>true</code>.
        /// </summary>
        public bool UtilizeFormatterServices { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether <see cref="TypeInfo"/> and <see cref="MemberInfo"/> types should be mapped.
        /// Aqua type system types are mapped only is this value is <code>false</code>.
        /// The default value is <code>true</code>.
        /// </summary>
        public bool PassthroughAquaTypeSystemTypes { get; set; } = true;
#pragma warning restore SA1629 // Documentation text should end with a periods

        internal DynamicObjectMapperSettings Copy() => (DynamicObjectMapperSettings)MemberwiseClone();
    }
}
