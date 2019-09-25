// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Dynamic
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.Serialization;
    using System.Security;
    using System.Text.RegularExpressions;

    partial class DynamicObjectMapper
    {
        private const string BackingFieldRegexPattern = @"^(.+\+)?\<(?<name>.+)\>k__BackingField$";

        /// <summary>
        /// Gets an uninitialized instance of the specified type by using <see cref="FormatterServices" />.
        /// </summary>
        [SecuritySafeCritical]
        private static object GetUninitializedObject(Type type)
        {
            return FormatterServices.GetUninitializedObject(type);
        }

        /// <summary>
        /// Populate object members type by using <see cref="FormatterServices" />.
        /// </summary>
        [SecuritySafeCritical]
        private void PopulateObjectMembers(Type type, DynamicObject from, object to)
        {
            var customPropertySet = GetPropertiesForMapping(type);
            var customPropertyNames = customPropertySet?
                .Where(p => p.GetCustomAttribute<UnmappedAttribute>() == null)
                .ToDictionary(x => x.Name);

            var members = FormatterServices.GetSerializableMembers(type);
            var membersByCleanName = members.ToDictionary(GetCleanMemberName);
            var memberValueMap = new Dictionary<MemberInfo, object>();

            foreach (var dynamicProperty in from.Properties)
            {
                if (!(customPropertyNames is null) && !customPropertyNames.ContainsKey(dynamicProperty.Name))
                {
                    continue;
                }

                MemberInfo member;
                if (membersByCleanName.TryGetValue(dynamicProperty.Name, out member))
                {
                    Type memberType;
                    switch (member.MemberType)
                    {
                        case MemberTypes.Field:
                            memberType = ((FieldInfo)member).FieldType;
                            break;

                        case MemberTypes.Property:
                            memberType = ((PropertyInfo)member).PropertyType;
                            break;

                        default:
                            throw new InvalidOperationException($"Unsupported member type {member.MemberType}.");
                    }

                    var value = MapFromDynamicObjectGraph(dynamicProperty.Value, memberType);

                    if (_suppressMemberAssignabilityValidation ||
                        IsAssignable(memberType, value) ||
                        TryExplicitConversions(memberType, ref value))
                    {
                        memberValueMap[member] = value;
                    }
                }
            }

            FormatterServices.PopulateObjectMembers(to, memberValueMap.Keys.ToArray(), memberValueMap.Values.ToArray());
        }

        /// <summary>
        /// Retrieves object members type by using <see cref="FormatterServices" /> and populates dynamic object.
        /// </summary>
        [SecuritySafeCritical]
        private void MapObjectMembers(Type type, object from, DynamicObject to, Func<Type, bool> setTypeInformation)
        {
            var customPropertySet = GetPropertiesForMapping(type);
            var customPropertyNames = customPropertySet?
                .Where(p => p.GetCustomAttribute<UnmappedAttribute>() == null)
                .ToDictionary(x => x.Name);

            var members = FormatterServices.GetSerializableMembers(type);
            var values = FormatterServices.GetObjectData(from, members);
            for (int i = 0; i < members.Length; i++)
            {
                var memberName = GetCleanMemberName(members[i]);
                if (!(customPropertyNames is null) && !customPropertyNames.ContainsKey(memberName))
                {
                    continue;
                }

                var value = MapToDynamicObjectIfRequired(values[i], setTypeInformation);
                to.Add(memberName, value);
            }
        }

        private static string GetCleanMemberName(MemberInfo member)
        {
            var memberName = member.Name;

            var match = Regex.Match(memberName, BackingFieldRegexPattern);
            if (match.Success)
            {
                memberName = match.Groups["name"].Value;
            }

            if (member.MemberType != MemberTypes.Property)
            {
                var property = member.DeclaringType.GetProperty(memberName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                if (!(property is null))
                {
                    memberName = property.Name;
                }
            }

            return memberName;
        }
    }
}
