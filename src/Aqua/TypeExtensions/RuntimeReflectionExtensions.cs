// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.TypeExtensions
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Reflection;

    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class RuntimeReflectionExtensions
    {
        /// <summary>
        /// Get <see cref="MethodInfo"/> using reflection.
        /// </summary>
        /// <remarks>
        /// This method should be used for types and method controlled by you only.
        /// For any other types, overload should be used to specify exact method signature.
        /// </remarks>
        /// <exception cref="InvalidOperationException">No method can be found matching specified criteria.</exception>
        public static MethodInfo GetMethodEx(this Type declaringType, string name, BindingFlags bindingFlags = ReflectionBinding.Any)
        {
            try
            {
                return GetMethodsCore(declaringType, name, x => true, bindingFlags).Single();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to get MethodInfo '{bindingFlags} {declaringType}.{name}()'", ex);
            }
        }

        /// <summary>
        /// Get <see cref="MethodInfo"/> using reflection.
        /// </summary>
        /// <exception cref="InvalidOperationException">No method can be found matching specified criteria.</exception>
        public static MethodInfo GetMethodEx(this Type declaringType, string name, params Type[] parameters)
            => GetMethodEx(declaringType, name, Array.Empty<Type>(), parameters, ReflectionBinding.Any);

        /// <summary>
        /// Get <see cref="MethodInfo"/> using reflection.
        /// </summary>
        /// <exception cref="InvalidOperationException">No method can be found matching specified criteria.</exception>
        public static MethodInfo GetMethodEx(this Type declaringType, string name, Type[] genericArguments, params Type[] parameters)
            => GetMethodEx(declaringType, name, genericArguments, parameters, ReflectionBinding.Any);

        /// <summary>
        /// Get <see cref="MethodInfo"/> using reflection.
        /// </summary>
        /// <exception cref="InvalidOperationException">No method can be found matching specified criteria.</exception>
        public static MethodInfo GetMethodEx(this Type declaringType, string name, Type[] genericArguments, Type[] parameters, BindingFlags bindingFlags)
        {
            try
            {
                return GetMethodsCore(declaringType, name, x => ParametersMatch(x, genericArguments, parameters), bindingFlags).Single();
            }
            catch (Exception ex)
            {
                static string TypesToString(Type[] types) => string.Join(", ", types.Select(x => x.PrintFriendlyName(false, false)));

                var genericArgumentString = TypesToString(genericArguments);
                if (!string.IsNullOrEmpty(genericArgumentString))
                {
                    genericArgumentString = $"<{genericArgumentString}>";
                }

                var parametersString = TypesToString(parameters);

                throw new InvalidOperationException($"Failed to get MethodInfo '{bindingFlags} {declaringType}.{name}{genericArgumentString}({parametersString})'", ex);
            }

            static bool ParametersMatch(MethodInfo method, Type[] genericArgumentTypes, Type[] parameterTypes)
            {
                method.AssertNotNull(nameof(method));
                genericArgumentTypes.AssertItemsNotNull(nameof(genericArgumentTypes));
                parameterTypes.AssertItemsNotNull(nameof(parameterTypes));

                if (method.IsGenericMethod)
                {
                    if (method.GetGenericArguments().Length != genericArgumentTypes.Length)
                    {
                        return false;
                    }

                    method = method.MakeGenericMethod(genericArgumentTypes);
                }
                else if (genericArgumentTypes.Length > 0)
                {
                    return false;
                }

                var parameters = method.GetParameters();
                if (parameters?.Length != parameterTypes.Length)
                {
                    return false;
                }

                for (int i = 0; i < parameters.Length; i++)
                {
                    var parameterType = parameters[i].ParameterType;
                    var expectedType = parameterTypes[i];
                    if (parameterType != expectedType)
                    {
                        return false;
                    }
                }

                return true;
            }
        }

        private static IEnumerable<MethodInfo> GetMethodsCore(Type declaringType, string name, Func<MethodInfo, bool> filter, BindingFlags bindingFlags)
            => declaringType
            .GetMethods(bindingFlags)
            .Where(x => string.Equals(x.Name, name, StringComparison.Ordinal) && filter(x));
    }
}