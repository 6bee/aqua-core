// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua
{
    using Aqua.TypeSystem;
    using System;

    [Obsolete("Class was renamed to NativeValueInspector", true)]
    public static class PrimitiveValueInspector
    {
        [Obsolete("Class was renamed to NativeValueInspector", true)]
        public static Func<object, object> GetConverter(TypeInfo typeInfo)
                => NativeValueInspector.GetConverter(typeInfo);
    }
}
