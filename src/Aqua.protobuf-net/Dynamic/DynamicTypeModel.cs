// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.ProtoBuf.Dynamic
{
    using Aqua.Dynamic;
    using global::ProtoBuf.Meta;

    internal static class DynamicTypeModel
    {
        public static RuntimeTypeModel ConfigureAquaDynamicTypes(this RuntimeTypeModel typeModel)
        {
            typeModel.GetType<Property>().SetSurrogate<PropertySurrogate>();

            _ = typeModel.GetType<PropertySet>();

            typeModel.GetType<DynamicObject>().SetSurrogate<DynamicObjectSurrogate>();

            return typeModel;
        }
    }
}
