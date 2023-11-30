// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.TypeSystem;

using Aqua.EnumerableExtensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Xml.Serialization;

[Serializable]
[DataContract(Name = "MethodBase", IsReference = true)]
[KnownType(typeof(ConstructorInfo)), XmlInclude(typeof(ConstructorInfo))]
[KnownType(typeof(MethodInfo)), XmlInclude(typeof(MethodInfo))]
public abstract class MethodBaseInfo : MemberInfo
{
    protected MethodBaseInfo()
    {
    }

    protected MethodBaseInfo(System.Reflection.MethodBase method, TypeInfoProvider typeInfoProvider)
        : base(method, typeInfoProvider)
    {
        var genericArguments = method.CheckNotNull().IsGenericMethod ? method.GetGenericArguments() : null;
        GenericArgumentTypes = genericArguments
            .AsNullIfEmpty()?
            .Select(x => typeInfoProvider.GetTypeInfo(x, false, false))
            .ToList();
        ParameterTypes = method
            .GetParameters()
            .AsNullIfEmpty()?
            .Select(x => typeInfoProvider.GetTypeInfo(x.ParameterType, false, false))
            .ToList();
    }

    protected MethodBaseInfo(string name, Type declaringType, IEnumerable<Type>? genericArguments, IEnumerable<Type>? parameterTypes, TypeInfoProvider typeInfoProvider)
        : this(
        name,
        typeInfoProvider.CheckNotNull().GetTypeInfo(declaringType, includePropertyInfos: false, setMemberDeclaringTypes: false),
        genericArguments?.Select(x => typeInfoProvider.GetTypeInfo(x, false, false)),
        parameterTypes?.Select(x => typeInfoProvider.GetTypeInfo(x, false, false)))
    {
    }

    protected MethodBaseInfo(string name, TypeInfo declaringType, IEnumerable<TypeInfo>? genericArguments, IEnumerable<TypeInfo>? parameterTypes)
        : base(name, declaringType)
    {
        GenericArgumentTypes = genericArguments
            .AsNullIfEmpty()?
            .ToList();
        ParameterTypes = parameterTypes
            .AsNullIfEmpty()?
            .ToList();
    }

    protected MethodBaseInfo(MethodBaseInfo method, TypeInfoProvider typeInfoProvider)
        : base(method, typeInfoProvider)
    {
        GenericArgumentTypes = method.CheckNotNull().GenericArgumentTypes
            .AsNullIfEmpty()?
            .Select(x => typeInfoProvider.Get(x))
            .ToList();
        ParameterTypes = method.ParameterTypes
            .AsNullIfEmpty()?
            .Select(x => typeInfoProvider.Get(x))
            .ToList();
    }

    [DataMember(Order = 5, IsRequired = false, EmitDefaultValue = false)]
    public List<TypeInfo>? GenericArgumentTypes { get; set; }

    [DataMember(Order = 6, IsRequired = false, EmitDefaultValue = false)]
    public List<TypeInfo>? ParameterTypes { get; set; }

    public bool IsGenericMethod => GenericArgumentTypes?.Any() ?? false;

    public override string ToString()
    {
        var hasGenericArguments = IsGenericMethod;
        return string.Format(
            "{0}.{1}{3}{4}{5}({2})",
            DeclaringType,
            Name,
            ParameterTypes.StringJoin(", "),
            hasGenericArguments ? "<" : null,
            GenericArgumentTypes.StringJoin(", "),
            hasGenericArguments ? ">" : null);
    }
}
