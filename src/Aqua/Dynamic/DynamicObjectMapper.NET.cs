// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

#if NET || NET35

namespace Aqua.Dynamic
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.Serialization;
    using System.Text.RegularExpressions;

    partial class DynamicObjectMapper
    {
        private const string BackingFieldRegexPattern = @"^(.+\+)?\<(?<name>.+)\>k__BackingField$";

        /// <summary>
        /// Gets an uninitialized instance of the specified type by using <see cref="FormatterServices" />
        /// </summary>
        private static object GetUninitializedObject(Type type)
        {
            return FormatterServices.GetUninitializedObject(type);
        }

        /// <summary>
        /// Populate object members type by using <see cref="FormatterServices" />
        /// </summary>
        private void PopulateObjectMembers(Type type, DynamicObject from, object to)
        {
            var customPropertySet = GetPropertiesForMapping(type);
            var customPropertyNames = ReferenceEquals(null, customPropertySet) ? null : customPropertySet.ToDictionary(x => x.Name);

            var members = FormatterServices.GetSerializableMembers(type);
            var membersByCleanName = members.ToDictionary(GetCleanMemberName);
            var memberValueMap = new Dictionary<System.Reflection.MemberInfo, object>();

            foreach (var dynamicProperty in from)
            {
                if (!ReferenceEquals(null, customPropertyNames) && !customPropertyNames.ContainsKey(dynamicProperty.Name))
                {
                    continue;
                }

                System.Reflection.MemberInfo member;
                if (membersByCleanName.TryGetValue(dynamicProperty.Name, out member))
                {
                    Type memberType;
                    switch (member.MemberType)
                    {
                        case System.Reflection.MemberTypes.Field:
                            memberType = ((System.Reflection.FieldInfo)member).FieldType;
                            break;

                        case System.Reflection.MemberTypes.Property:
                            memberType = ((System.Reflection.PropertyInfo)member).PropertyType;
                            break;

                        default:
                            throw new Exception($"Unsupported member type {member.MemberType}.");
                    }

                    var value = MapFromDynamicObjectGraph(dynamicProperty.Value, memberType);

                    if (_suppressMemberAssignabilityValidation || IsAssignable(memberType, value))
                    {
                        memberValueMap[member] = value;
                    }
                }
            }

            FormatterServices.PopulateObjectMembers(to, memberValueMap.Keys.ToArray(), memberValueMap.Values.ToArray());
        }

        /// <summary>
        /// Retrieves object members type by using <see cref="FormatterServices" /> and populates dynamic object
        /// </summary>
        private void MapObjectMembers(object from, DynamicObject to, Func<Type, bool> setTypeInformation)
        {
            var type = _resolveType(to.Type);

            var customPropertySet = GetPropertiesForMapping(type);
            var customPropertyNames = ReferenceEquals(null, customPropertySet) ? null : customPropertySet.ToDictionary(x => x.Name);
            var members = FormatterServices.GetSerializableMembers(type);
            var values = FormatterServices.GetObjectData(from, members);
            for (int i = 0; i < members.Length; i++)
            {
                var memberName = GetCleanMemberName(members[i]);
                if (!ReferenceEquals(null, customPropertyNames) && !customPropertyNames.ContainsKey(memberName))
                {
                    continue;
                }

                var value = MapToDynamicObjectIfRequired(values[i], setTypeInformation);
                to[memberName] = value;
            }
        }

        private static string GetCleanMemberName(System.Reflection.MemberInfo member)
        {
            var memberName = member.Name;

            var match = Regex.Match(memberName, BackingFieldRegexPattern);
            if (match.Success)
            {
                memberName = match.Groups["name"].Value;
            }

            if (member.MemberType != System.Reflection.MemberTypes.Property)
            {
                var property = member.DeclaringType.GetProperty(memberName, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.IgnoreCase);
                if (!ReferenceEquals(null, property))
                {
                    memberName = property.Name;
                }
            }

            return memberName;
        }
    }
}

#endif