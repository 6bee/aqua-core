// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.ProtoBuf.Dynamic
{
    using Aqua.Dynamic;

    internal static class DynamicTypeModel
    {
        public static AquaTypeModel ConfigureAquaDynamicTypes(this AquaTypeModel typeModel)
            => typeModel
            .AddTypeSurrogate<DynamicObject, DynamicObjectSurrogate>()
            .AddSubType<Value, Value<DynamicObjectSurrogate>>()
            .AddSubType<Values, Values<DynamicObjectSurrogate>>()
            .AddTypeSurrogate<Property, PropertySurrogate>();
    }
}
