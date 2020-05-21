// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.TypeSystem
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime.Serialization;

    [Serializable]
    [DataContract(Name = "Constructor", IsReference = true)]
    [DebuggerDisplay("Constructor: {Name,nq}")]
    public class ConstructorInfo : MethodBaseInfo
    {
        private const string DefaultStaticConstructorName = ".cctor";

        [NonSerialized]
        private System.Reflection.ConstructorInfo? _constructor;

        public ConstructorInfo()
        {
        }

        public ConstructorInfo(System.Reflection.ConstructorInfo constructor)
            : this(constructor, new TypeInfoProvider())
        {
        }

        internal ConstructorInfo(System.Reflection.ConstructorInfo constructor, TypeInfoProvider typeInfoProvider)
            : base(constructor, typeInfoProvider)
        {
            _constructor = constructor;
        }

        public ConstructorInfo(string name, Type declaringType, IEnumerable<Type>? parameterTypes = null)
            : base(name, declaringType, null, parameterTypes, new TypeInfoProvider())
        {
            if (string.Equals(name, DefaultStaticConstructorName, StringComparison.Ordinal))
            {
                IsStatic = true;
            }
        }

        protected ConstructorInfo(ConstructorInfo constructor)
            : base(constructor, new TypeInfoProvider())
        {
        }

        public override MemberTypes MemberType => MemberTypes.Constructor;

        public System.Reflection.ConstructorInfo Constructor
            => _constructor ?? (_constructor = this.ResolveConstructor(TypeResolver.Instance))
            ?? throw new TypeResolverException($"Failed to resolve constructor, consider using extension method to specify {nameof(ITypeResolver)}.");

        public static explicit operator System.Reflection.ConstructorInfo(ConstructorInfo c) => c.Constructor;
    }
}
