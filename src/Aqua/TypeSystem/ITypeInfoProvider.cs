// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.TypeSystem
{
    using System;

    public interface ITypeInfoProvider
    {
        TypeInfo Get(Type type, bool? includePropertyInfos = null, bool? setMemberDeclaringTypes = null);
    }
}
