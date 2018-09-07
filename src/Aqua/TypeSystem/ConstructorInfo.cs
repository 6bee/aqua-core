// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.TypeSystem
{
    using System;
    using System.Diagnostics;
    using System.Runtime.Serialization;
    using BindingFlags = System.Reflection.BindingFlags;

    [Serializable]
    [DataContract(Name = "Constructor", IsReference = true)]
    [DebuggerDisplay("{Name}")]
    public class ConstructorInfo : MethodBaseInfo
    {
        [NonSerialized]
        private System.Reflection.ConstructorInfo _constructor;

        public ConstructorInfo()
        {
        }

        public ConstructorInfo(System.Reflection.ConstructorInfo constructorInfo)
            : this(constructorInfo, new TypeInfoProvider())
        {
        }

        internal ConstructorInfo(System.Reflection.ConstructorInfo constructorInfo, TypeInfoProvider typeInfoProvider)
            : base(constructorInfo, typeInfoProvider)
        {
            _constructor = constructorInfo;
        }

        // TODO: replace binding flags by bool flags
        public ConstructorInfo(string name, Type declaringType, BindingFlags bindingFlags, Type[] genericArguments, Type[] parameterTypes)
            : base(name, declaringType, bindingFlags, genericArguments, parameterTypes, new TypeInfoProvider())
        {
        }

        protected ConstructorInfo(ConstructorInfo constructorInfo)
            : base(constructorInfo, new TypeInfoProvider())
        {
        }

        public override MemberTypes MemberType => MemberTypes.Constructor;

        internal System.Reflection.ConstructorInfo Constructor
            => _constructor ?? (_constructor = this.ResolveConstructor(TypeResolver.Instance));

        public static explicit operator System.Reflection.ConstructorInfo(ConstructorInfo c)
            => c.Constructor;

        public override string ToString()
            => $".ctor {base.ToString()}";
    }
}
