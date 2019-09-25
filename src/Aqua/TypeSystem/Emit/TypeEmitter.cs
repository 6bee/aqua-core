// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.TypeSystem.Emit
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Reflection.Emit;
    using System.Runtime.CompilerServices;
    using System.Security;
    using System.Threading;
    using TypeInfo = Aqua.TypeSystem.TypeInfo;

    public sealed partial class TypeEmitter
    {
        private readonly TypeCache _typeCache = new TypeCache();

        private readonly AssemblyBuilder _assemblyBuilder;
        private readonly ModuleBuilder _module;
        private int _classIndex = -1;

        [SecuritySafeCritical]
        public TypeEmitter()
        {
            var assemblyName = new AssemblyName("Aqua.TypeSystem.Emit.Types");
            try
            {
                _assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.RunAndCollect);
            }
            catch (SecurityException)
            {
                // Note: creation of collectible assemblies requires full-trust.
                _assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
            }

            _module = _assemblyBuilder.DefineDynamicModule(assemblyName.Name);
        }

        public Type EmitType(TypeInfo typeInfo)
        {
            if (typeInfo.IsAnonymousType)
            {
                var properties = typeInfo.Properties.Select(x => x.Name).ToList();
                return _typeCache.GetOrCreate(properties, InternalEmitAnonymousType);
            }
            else
            {
                return _typeCache.GetOrCreate(typeInfo, InternalEmitType);
            }
        }

        private Type InternalEmitType(TypeInfo typeInfo)
        {
            var fullName = CreateUniqueClassName();

            var propertyInfos = typeInfo.Properties.ToArray();

            // define type
            var type = _module.DefineType(fullName, TypeAttributes.Public | TypeAttributes.AutoClass | TypeAttributes.AnsiClass | TypeAttributes.BeforeFieldInit | TypeAttributes.Sealed, typeof(object));

            type.SetCustomAttribute(new CustomAttributeBuilder(typeof(EmittedTypeAttribute).GetConstructor(new Type[0]), new object[0]));
            type.SetCustomAttribute(new CustomAttributeBuilder(typeof(CompilerGeneratedAttribute).GetConstructor(new Type[0]), new object[0]));

            // define fields
            var fields = propertyInfos
                .Select(x => type.DefineField($"_{x.Name}", x.PropertyType.Type, FieldAttributes.Private))
                .ToArray();

            // define default constructor
            var constructor = type.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard, null);
            var objectCtor = typeof(object).GetConstructor(Type.EmptyTypes);
            var il = constructor.GetILGenerator();
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Call, objectCtor);
            il.Emit(OpCodes.Ret);

            // define properties
            var properties = propertyInfos
                .Select((x, i) =>
                {
                    var property = type.DefineProperty(x.Name, PropertyAttributes.HasDefault, x.PropertyType.Type, null);

                    var propertyGetter = type.DefineMethod($"get_{x.Name}", MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig, x.PropertyType.Type, null);
                    var getterPil = propertyGetter.GetILGenerator();
                    getterPil.Emit(OpCodes.Ldarg_0);
                    getterPil.Emit(OpCodes.Ldfld, fields[i]);
                    getterPil.Emit(OpCodes.Ret);

                    property.SetGetMethod(propertyGetter);

                    var propertySetter = type.DefineMethod($"set_{x.Name}", MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig, null, new[] { x.PropertyType.Type });
                    var setterPil = propertySetter.GetILGenerator();
                    setterPil.Emit(OpCodes.Ldarg_0);
                    setterPil.Emit(OpCodes.Ldarg_1);
                    setterPil.Emit(OpCodes.Stfld, fields[i]);
                    setterPil.Emit(OpCodes.Ret);

                    property.SetSetMethod(propertySetter);

                    return property;
                })
                .ToArray();

            // create type
            var t1 = type.CreateTypeInfo();
            return t1.AsType();
        }

        private Type InternalEmitAnonymousType(IEnumerable<string> propertyNames)
        {
            if (!propertyNames.Any())
            {
                throw new Exception("No properties specified");
            }

            var fullName = CreateUniqueClassNameForAnonymousType(propertyNames);

            // define type
            var type = _module.DefineType(fullName, TypeAttributes.Public | TypeAttributes.AutoClass | TypeAttributes.AnsiClass | TypeAttributes.BeforeFieldInit | TypeAttributes.Sealed, typeof(object));

            type.SetCustomAttribute(new CustomAttributeBuilder(typeof(EmittedTypeAttribute).GetConstructor(new Type[0]), new object[0]));
            type.SetCustomAttribute(new CustomAttributeBuilder(typeof(CompilerGeneratedAttribute).GetConstructor(new Type[0]), new object[0]));

            // define generic parameters
            var genericTypeParameterNames = propertyNames.Select((x, i) => $"T{i}").ToArray();
            var genericTypeParameters = type.DefineGenericParameters(genericTypeParameterNames);

            // define fields
            var fields = propertyNames
                .Select((x, i) => type.DefineField($"_{x}", genericTypeParameters[i], FieldAttributes.Private | FieldAttributes.InitOnly))
                .ToArray();

            // define constructor
            var constructor = type.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard, genericTypeParameters.Cast<Type>().ToArray());
            var objectCtor = typeof(object).GetConstructor(Type.EmptyTypes);
            var il = constructor.GetILGenerator();
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Call, objectCtor);
            for (int i = 0; i < fields.Length; i++)
            {
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldarg, i + 1);
                il.Emit(OpCodes.Stfld, fields[i]);
            }

            il.Emit(OpCodes.Ret);

            // define properties
            var properties = propertyNames
                .Select((x, i) =>
                {
                    var property = type.DefineProperty(x, PropertyAttributes.HasDefault, genericTypeParameters[i], null);

                    var propertyGetter = type.DefineMethod($"get_{x}", MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig, genericTypeParameters[i], null);
                    var pil = propertyGetter.GetILGenerator();
                    pil.Emit(OpCodes.Ldarg_0);
                    pil.Emit(OpCodes.Ldfld, fields[i]);
                    pil.Emit(OpCodes.Ret);

                    property.SetGetMethod(propertyGetter);

                    return property;
                })
                .ToArray();

            // create type
            var t1 = type.CreateTypeInfo();
            return t1.AsType();
        }

        private string CreateUniqueClassName()
        {
            var id = Interlocked.Increment(ref _classIndex);
            var fullName = $"{_module.Name}.<>__EmittedType__{id}";
            return fullName;
        }

        private string CreateUniqueClassNameForAnonymousType(IEnumerable<string> properties)
        {
            var id = Interlocked.Increment(ref _classIndex);
            var fullName = $"{_module.Name}.<>__EmittedType__{id}`{properties.Count()}";
            return fullName;
        }
    }
}