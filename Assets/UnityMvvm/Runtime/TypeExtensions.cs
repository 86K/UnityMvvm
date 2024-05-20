using System;
using System.Reflection;

namespace Fusion.Mvvm
{
    public static class TypeExtensions
    {
        public static bool IsSubclassOfGenericTypeDefinition(this Type type, Type genericTypeDefinition)
        {
#if NETFX_CORE
            if (!genericTypeDefinition.GetTypeInfo().IsGenericTypeDefinition)
#else
            if (!genericTypeDefinition.IsGenericTypeDefinition)
#endif
                return false;

#if NETFX_CORE
            if (type.GetTypeInfo().IsGenericType && type.GetGenericTypeDefinition().Equals(genericTypeDefinition))
#else
            if (type.IsGenericType && type.GetGenericTypeDefinition() == genericTypeDefinition)
#endif
                return true;

#if NETFX_CORE
            Type baseType = type.GetTypeInfo().BaseType;
#else
            Type baseType = type.BaseType;
#endif
            if (baseType != null && baseType != typeof(object))
            {
                if (IsSubclassOfGenericTypeDefinition(baseType, genericTypeDefinition))
                    return true;
            }

            foreach (Type t in type.GetInterfaces())
            {
                if (IsSubclassOfGenericTypeDefinition(t, genericTypeDefinition))
                    return true;
            }

            return false;
        }

        public static object CreateDefault(this Type type)
        {
            if (type == null)
                return null;

            if (type == typeof(string))
                return "";
#if NETFX_CORE
            if (!type.GetTypeInfo().IsValueType)
#else
            if (!type.IsValueType)
#endif
                return null;

            if (Nullable.GetUnderlyingType(type) != null)
                return null;

            return Activator.CreateInstance(type);
        }

        public static bool IsStatic(this MemberInfo info)
        {
            var fieldInfo = info as FieldInfo;
            if (fieldInfo != null)
                return fieldInfo.IsStatic;

            var propertyInfo = info as PropertyInfo;
            if (propertyInfo != null)
            {
                var method = propertyInfo.GetGetMethod();
                if (method != null)
                    return method.IsStatic;

                method = propertyInfo.GetSetMethod();
                if (method != null)
                    return method.IsStatic;
            }

            var methodInfo = info as MethodInfo;
            if (methodInfo != null)
                return methodInfo.IsStatic;

            var eventInfo = info as EventInfo;
            if (eventInfo != null)
            {
                var method = eventInfo.GetAddMethod();
                if (method != null)
                    return method.IsStatic;

                method = eventInfo.GetRemoveMethod();
                if (method != null)
                    return method.IsStatic;
            }

            return false;
        }

        public static object ToSafe(this Type type, object value)
        {
            if (value == null)
                return type.CreateDefault();

            var safeValue = value;
            try
            {
                if (!type.IsInstanceOfType(value))
                {
                    if (value is IObservableProperty property)
                    {
                        safeValue = property.Value;
                        if (!type.IsInstanceOfType(safeValue))
                        {
                            safeValue = ChangeType(safeValue, type);
                        }
                    }
                    else if (type == typeof(string))
                    {
                        safeValue = value.ToString();
                    }
#if NETFX_CORE
                    else if (type.GetTypeInfo().IsEnum)
#else
                    else if (type.IsEnum)
#endif
                    {
                        safeValue = value is string s ? Enum.Parse(type, s, true) : Enum.ToObject(type, value);
                    }
#if NETFX_CORE
                    else if (type.GetTypeInfo().IsValueType)
#else
                    else if (type.IsValueType)
#endif
                    {
                        var underlyingType = Nullable.GetUnderlyingType(type) ?? type;
                        safeValue = underlyingType == typeof(bool) ? ConvertToBoolean(value) : ChangeType(value, underlyingType);
                    }
                    else
                    {
                        safeValue = ChangeType(value, type);
                    }
                }
            }
            catch (Exception)
            {
            }

            return safeValue;
        }

        private static bool ConvertToBoolean(object result)
        {
            if (result == null)
                return false;

            if (result is string s)
                return s.ToLower().Equals("true");

            if (result is bool b)
                return b;

            var resultType = result.GetType();
#if NETFX_CORE
            if (resultType.GetTypeInfo().IsValueType)
#else
            if (resultType.IsValueType)
#endif
            {
                var underlyingType = Nullable.GetUnderlyingType(resultType) ?? resultType;
                return !result.Equals(underlyingType.CreateDefault());
            }

            return true;
        }

        private static object ChangeType(object value, Type type)
        {
            try
            {
                return Convert.ChangeType(value, type);
            }
            catch (Exception)
            {
                return value;
            }
        }
        
        public static MemberInfo FindFirstMemberInfo(this Type type, string name)
        {
            var members = type.GetMember(name);
            if (members.Length <= 0)
                return null;
            return members[0];
        }

        public static MemberInfo FindFirstMemberInfo(this Type type, string name, BindingFlags flags)
        {
            var members = type.GetMember(name, flags);
            if (members.Length <= 0)
                return null;
            return members[0];
        }
    }
}