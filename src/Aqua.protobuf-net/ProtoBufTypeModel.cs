// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.ProtoBuf
{
    using Aqua.EnumerableExtensions;
    using Aqua.ProtoBuf.Dynamic;
    using Aqua.ProtoBuf.TypeSystem;
    using global::ProtoBuf.Meta;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;

    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class ProtoBufTypeModel
    {
        private static readonly IReadOnlyCollection<Type> _systemTypes = new[]
            {
                typeof(string),
                typeof(int),
                typeof(byte),
                typeof(bool),
                typeof(double),
                typeof(char),
                typeof(Guid),
                typeof(long),
                typeof(float),
                typeof(decimal),
                typeof(sbyte),
                typeof(uint),
                typeof(ulong),
                typeof(short),
                typeof(ushort),
                typeof(DateTime),
                typeof(TimeSpan),
            };

        public static AquaTypeModel ConfigureAquaTypes(string? name = null, bool configureDefaultSystemTypes = true)
            => ConfigureAquaTypes(RuntimeTypeModel.Create(name), configureDefaultSystemTypes);

        public static AquaTypeModel ConfigureAquaTypes(this RuntimeTypeModel typeModel, bool configureDefaultSystemTypes = true)
        {
            var aquaTypeModel = new AquaTypeModel(typeModel);

            if (configureDefaultSystemTypes)
            {
                ConfigureDefaultSystemTypes(aquaTypeModel);
            }

            return aquaTypeModel
                .ConfigureAquaTypeSystemTypes()
                .ConfigureAquaDynamicTypes();
        }

        private static void ConfigureDefaultSystemTypes(AquaTypeModel typeModel)
            => _systemTypes.ForEach(t => typeModel.AddDynamicPropertyType(t));
    }
}