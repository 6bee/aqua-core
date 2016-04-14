// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Dynamic
{
    using System;

    public interface IDynamicObjectFactory
    {
        DynamicObject CreateDynamicObject(Type type, object instance);
    }
}
