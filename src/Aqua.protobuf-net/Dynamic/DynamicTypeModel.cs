// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.ProtoBuf.Dynamic
{
    using Aqua.Dynamic;

    internal static class DynamicTypeModel
    {
        public static AquaTypeModel ConfigureAquaDynamicTypes(this AquaTypeModel typeModel)
            => typeModel
            .AddTypeSurrogate<DynamicObject, DynamicObjectSurrogate>()
            .AddTypeSurrogate<Property, PropertySurrogate>()
            .AddType<PropertySet>()
            .AddType<DynamicObjectArraySurrogate>(t => t[1].SupportNull = true);
    }
}
