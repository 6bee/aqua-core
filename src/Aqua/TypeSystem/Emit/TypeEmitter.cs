// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.TypeSystem.Emit
{
    using Aqua.EnumerableExtensions;
    using Aqua.Utils;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Reflection.Emit;
    using System.Runtime.CompilerServices;
    using System.Runtime.ExceptionServices;
    using System.Security;
    using System.Threading;
    using TypeInfo = Aqua.TypeSystem.TypeInfo;

    public sealed partial class TypeEmitter
    {
        private sealed class TypeResolverScope : ITypeResolver
        {
            private readonly HashSet<TypeInfo> _references = new (ReferenceEqualityComparer<TypeInfo>.Default);
            private readonly ITypeResolver _typeResolver;

            public TypeResolverScope(ITypeResolver typeResolver)
                => _typeResolver = typeResolver;

            Type? ITypeResolver.ResolveType(TypeInfo? type)
            {
                if (type is null)
                {
                    return null;
                }

                lock (_references)
                {
                    try
                    {
                        if (!_references.Add(type))
                        {
                            throw new TypeEmitterException($"Cannot emit type with circular reference: '{type}'");
                        }

                        return _typeResolver.ResolveType(type);
                    }
                    finally
                    {
                        _references.Remove(type);
                    }
                }
            }
        }

        private readonly ConstructorInfo _emittedTypeAttributeConstructorInfo = typeof(EmittedTypeAttribute).GetConstructor(Type.EmptyTypes) !;
        private readonly ConstructorInfo _compilerGeneratedAttributeConstructorInfo = typeof(CompilerGeneratedAttribute).GetConstructor(Type.EmptyTypes) !;
        private readonly ConstructorInfo _objectConstructorInfo = typeof(object).GetConstructor(Type.EmptyTypes) !;
        private readonly TypeCache _typeCache;
        private readonly AssemblyBuilder _assemblyBuilder;
        private readonly ModuleBuilder _module;
        private readonly TypeResolverScope _typeResolver;
        private int _classIndex = -1;

        [SecuritySafeCritical]
        public TypeEmitter(ITypeResolver? typeResolver = null)
        {
            _typeResolver = new TypeResolverScope(typeResolver ?? TypeResolver.Instance);
            _typeCache = new TypeCache(_typeResolver);
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

            _module = _assemblyBuilder.DefineDynamicModule(assemblyName.Name!);
        }

        public Type EmitType(TypeInfo type)
        {
            Func<Type> emit;
            if (type.CheckNotNull(nameof(type)).IsAnonymousType)
            {
                Exception CreateException(string reason) => throw new ArgumentException($"Cannot emit anonymous type '{type}' {reason}.");
                var properties = type.Properties?
                    .Select(x => x.Name ?? throw new ArgumentException("with unnamed property contained"))
                    .ToList();
                if (properties is null)
                {
                    throw CreateException("with not proeprties declared");
                }

                emit = () => _typeCache.GetOrCreate(properties, InternalEmitAnonymousType);
            }
            else
            {
                emit = () => _typeCache.GetOrCreate(type, InternalEmitType);
            }

            try
            {
                return emit();
            }
            catch (Exception ex)
            {
                var innerException = ex;
                while (innerException is not null)
                {
                    if (innerException is TypeEmitterException typeEmitterException)
                    {
                        ExceptionDispatchInfo.Capture(typeEmitterException).Throw();
                    }

                    innerException = innerException.InnerException;
                }

                throw new TypeEmitterException($"Failed to emit type {type}", ex);
            }
        }

        private Type InternalEmitType(TypeInfo typeInfo)
        {
            var fullName = CreateUniqueClassName();

            var propertyInfos = typeInfo.Properties?
                .Select(x => new
                {
                    Name = x.Name ?? throw new ArgumentException($"Property name must not be empty for type {typeInfo}."),
                    Type = x.PropertyType.ResolveType(_typeResolver) ?? throw new ArgumentException($"Property type must not be null for type {typeInfo}."),
                })
                .ToArray()
                ?? throw new ArgumentException($"Properties description must not be missing for type {typeInfo}.");

            // define type
            var type = _module.DefineType(fullName, TypeAttributes.Public | TypeAttributes.AutoClass | TypeAttributes.AnsiClass | TypeAttributes.BeforeFieldInit | TypeAttributes.Sealed, typeof(object));

            type.SetCustomAttribute(new CustomAttributeBuilder(_emittedTypeAttributeConstructorInfo, Array.Empty<object>()));
            type.SetCustomAttribute(new CustomAttributeBuilder(_compilerGeneratedAttributeConstructorInfo, Array.Empty<object>()));

            // define fields
            var fields = propertyInfos
                .Select(x => type.DefineField($"_{x.Name}", x.Type, FieldAttributes.Private))
                .ToArray();

            // define default constructor
            var constructor = type.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard, null);
            var il = constructor.GetILGenerator();
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Call, _objectConstructorInfo);
            il.Emit(OpCodes.Ret);

            // define properties
            propertyInfos.ForEach((x, i) =>
                {
                    var property = type.DefineProperty(x.Name, PropertyAttributes.HasDefault, x.Type, null);

                    var propertyGetter = type.DefineMethod($"get_{x.Name}", MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig, x.Type, null);
                    var getterPil = propertyGetter.GetILGenerator();
                    getterPil.Emit(OpCodes.Ldarg_0);
                    getterPil.Emit(OpCodes.Ldfld, fields[i]);
                    getterPil.Emit(OpCodes.Ret);

                    property.SetGetMethod(propertyGetter);

                    var propertySetter = type.DefineMethod($"set_{x.Name}", MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig, null, new[] { x.Type });
                    var setterPil = propertySetter.GetILGenerator();
                    setterPil.Emit(OpCodes.Ldarg_0);
                    setterPil.Emit(OpCodes.Ldarg_1);
                    setterPil.Emit(OpCodes.Stfld, fields[i]);
                    setterPil.Emit(OpCodes.Ret);

                    property.SetSetMethod(propertySetter);
                });

            // create type
            var t1 = type.CreateTypeInfo() ?? throw new TypeEmitterException($"Failed to create {typeof(System.Reflection.TypeInfo).FullName} for '{typeInfo}'.");
            return t1.AsType();
        }

        private Type InternalEmitAnonymousType(IEnumerable<string> propertyNames)
        {
            if (!propertyNames.Any())
            {
                throw new TypeEmitterException("No properties specified");
            }

            var fullName = CreateUniqueClassNameForAnonymousType(propertyNames);

            // define type
            var type = _module.DefineType(fullName, TypeAttributes.Public | TypeAttributes.AutoClass | TypeAttributes.AnsiClass | TypeAttributes.BeforeFieldInit | TypeAttributes.Sealed, typeof(object));

            type.SetCustomAttribute(new CustomAttributeBuilder(_emittedTypeAttributeConstructorInfo, Array.Empty<object>()));
            type.SetCustomAttribute(new CustomAttributeBuilder(_compilerGeneratedAttributeConstructorInfo, Array.Empty<object>()));

            // define generic parameters
            var genericTypeParameterNames = propertyNames.Select((x, i) => $"T{i}").ToArray();
            var genericTypeParameters = type.DefineGenericParameters(genericTypeParameterNames);

            // define fields
            var fields = propertyNames
                .Select((x, i) => type.DefineField($"_{x}", genericTypeParameters[i], FieldAttributes.Private | FieldAttributes.InitOnly))
                .ToArray();

            // define constructor parameter names
            var parameterNames = propertyNames.ToArray();

            // define constructor
            var constructor = type.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard, genericTypeParameters.Cast<Type>().ToArray());
            var il = constructor.GetILGenerator();
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Call, _objectConstructorInfo);
            for (int i = 0; i < fields.Length; i++)
            {
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldarg, i + 1);
                il.Emit(OpCodes.Stfld, fields[i]);
                constructor.DefineParameter(i + 1, ParameterAttributes.None, parameterNames[i]);
            }

            il.Emit(OpCodes.Ret);

            // define properties
            propertyNames.ForEach((x, i) =>
                {
                    var property = type.DefineProperty(x, PropertyAttributes.HasDefault, genericTypeParameters[i], null);

                    var propertyGetter = type.DefineMethod($"get_{x}", MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig, genericTypeParameters[i], null);
                    var pil = propertyGetter.GetILGenerator();
                    pil.Emit(OpCodes.Ldarg_0);
                    pil.Emit(OpCodes.Ldfld, fields[i]);
                    pil.Emit(OpCodes.Ret);

                    property.SetGetMethod(propertyGetter);
                });

            // create type
            var t1 = type.CreateTypeInfo() ?? throw new TypeEmitterException($"Failed to create {typeof(System.Reflection.TypeInfo).FullName} for anonymous type.");
            return t1.AsType();
        }

        private string CreateUniqueClassName()
        {
            var id = Interlocked.Increment(ref _classIndex);
            return $"{_module.Name}.<>__EmittedType__{id}";
        }

        private string CreateUniqueClassNameForAnonymousType(IEnumerable<string> properties)
        {
            var id = Interlocked.Increment(ref _classIndex);
            return $"{_module.Name}.<>__EmittedType__{id}`{properties.Count()}";
        }
    }
}