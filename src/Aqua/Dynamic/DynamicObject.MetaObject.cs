// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Dynamic;

using Aqua.TypeExtensions;
using System.Dynamic;
using System.Linq.Expressions;
using MethodInfo = System.Reflection.MethodInfo;

partial class DynamicObject : IDynamicMetaObjectProvider
{
    DynamicMetaObject IDynamicMetaObjectProvider.GetMetaObject(Expression parameter)
        => new MetaObject(parameter, BindingRestrictions.GetInstanceRestriction(parameter, this), this);

    private sealed class MetaObject : DynamicMetaObject
    {
        private static readonly MethodInfo _getMethod = typeof(DynamicObject).GetMethodEx(
            nameof(Get),
            typeof(string));

        private static readonly MethodInfo _setMethod = typeof(DynamicObject).GetMethodEx(
            nameof(Set),
            typeof(string),
            typeof(object));

        public MetaObject(Expression expression, BindingRestrictions restrictions, DynamicObject dynamicObject)
            : base(expression, restrictions, dynamicObject)
        {
        }

        public override DynamicMetaObject BindGetMember(GetMemberBinder binder)
        {
            var self = Expression;
            var keyExpr = Expression.Constant(binder.Name);
            var targetType = _getMethod.DeclaringType!;
            var target = Expression.Call(Expression.Convert(self, targetType), _getMethod, keyExpr);
            return new DynamicMetaObject(target, BindingRestrictions.GetTypeRestriction(self, targetType));
        }

        public override DynamicMetaObject BindSetMember(SetMemberBinder binder, DynamicMetaObject value)
        {
            var self = Expression;
            var keyExpr = Expression.Constant(binder.Name);
            var valueExpr = Expression.Convert(value.Expression, typeof(object));
            var targetType = _setMethod.DeclaringType!;
            var target = Expression.Call(Expression.Convert(self, targetType), _setMethod, keyExpr, valueExpr);
            return new DynamicMetaObject(target, BindingRestrictions.GetTypeRestriction(self, targetType));
        }

        public override IEnumerable<string> GetDynamicMemberNames()
            => Value is DynamicObject dynamicObject
            ? dynamicObject.GetPropertyNames()
            : Enumerable.Empty<string>();
    }
}