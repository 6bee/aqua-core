// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Tests.TypeSystem.TypeResolver;

using Aqua.TypeSystem;

using System.Reflection;

public class When_resolving_hidden_member
{
    private class TypeHidingBase
    {
        public string Field = string.Empty;

        public string Property { get; }

        public void Method()
        {
            // empty
        }
    }

    private class TypeHiding : TypeHidingBase
    {
        public new string Field = string.Empty;

        public new int Property { get; }

        public new void Method()
        {
            // empty
        }
    }

    [Fact]
    public void ResolveProperty_should_return_original_property_info()
    {
        var typeResolver = new TypeResolver();

        System.Reflection.TypeInfo type = typeof(TypeHiding).GetTypeInfo();
        System.Reflection.PropertyInfo propertyInfo = type.GetDeclaredProperty(nameof(TypeHiding.Property));
        Aqua.TypeSystem.PropertyInfo mappedProperty = new(propertyInfo);
        System.Reflection.PropertyInfo resolvedProperty = mappedProperty.ResolveProperty(typeResolver);

        resolvedProperty.ShouldBe(propertyInfo);
    }

    [Fact]
    public void ResolveMemberInfo_should_return_original_property_info()
    {
        var typeResolver = new TypeResolver();

        System.Reflection.TypeInfo type = typeof(TypeHiding).GetTypeInfo();
        System.Reflection.PropertyInfo propertyInfo = type.GetDeclaredProperty(nameof(TypeHiding.Property));
        Aqua.TypeSystem.PropertyInfo mappedProperty = new(propertyInfo);
        System.Reflection.MemberInfo resolvedMember = mappedProperty.ResolveMemberInfo(typeResolver);

        resolvedMember.ShouldBe(propertyInfo);
    }

    [Fact]
    public void ResolveField_should_return_original_field_info()
    {
        var typeResolver = new TypeResolver();

        System.Reflection.TypeInfo type = typeof(TypeHiding).GetTypeInfo();
        System.Reflection.FieldInfo fieldInfo = type.GetDeclaredField(nameof(TypeHiding.Field));
        Aqua.TypeSystem.FieldInfo mappedField = new(fieldInfo);
        System.Reflection.FieldInfo resolvedField = mappedField.ResolveField(typeResolver);

        resolvedField.ShouldBe(fieldInfo);
    }

    [Fact]
    public void ResolveMemberInfo_should_return_original_field_info()
    {
        var typeResolver = new TypeResolver();

        System.Reflection.TypeInfo type = typeof(TypeHiding).GetTypeInfo();
        System.Reflection.FieldInfo fieldInfo = type.GetDeclaredField(nameof(TypeHiding.Field));
        Aqua.TypeSystem.FieldInfo mappedField = new(fieldInfo);
        System.Reflection.MemberInfo resolvedMember = mappedField.ResolveMemberInfo(typeResolver);

        resolvedMember.ShouldBe(fieldInfo);
    }

    [Fact]
    public void ResolveMethod_should_return_original_method_info()
    {
        var typeResolver = new TypeResolver();

        System.Reflection.TypeInfo type = typeof(TypeHiding).GetTypeInfo();
        System.Reflection.MethodInfo methodInfo = type.GetDeclaredMethod(nameof(TypeHiding.Method));
        Aqua.TypeSystem.MethodInfo mappedMethod = new(methodInfo);
        System.Reflection.MethodInfo resolvedMethod = mappedMethod.ResolveMethod(typeResolver);

        resolvedMethod.ShouldBe(methodInfo);
    }

    [Fact]
    public void ResolveMemberInfo_should_return_original_method_info()
    {
        var typeResolver = new TypeResolver();

        System.Reflection.TypeInfo type = typeof(TypeHiding).GetTypeInfo();
        System.Reflection.MethodInfo methodInfo = type.GetDeclaredMethod(nameof(TypeHiding.Method));
        Aqua.TypeSystem.MethodInfo mappedMethod = new(methodInfo);
        System.Reflection.MemberInfo resolvedMember = mappedMethod.ResolveMemberInfo(typeResolver);

        resolvedMember.ShouldBe(methodInfo);
    }
}
