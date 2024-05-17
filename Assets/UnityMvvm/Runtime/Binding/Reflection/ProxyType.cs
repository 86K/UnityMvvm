using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using Object = System.Object;

namespace Fusion.Mvvm
{
    public class ProxyType : IProxyType
    {
        private static readonly BindingFlags DEFAULT_LOOKUP =
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;

        private readonly Dictionary<string, IProxyEventInfo> _events = new Dictionary<string, IProxyEventInfo>();
        private readonly Dictionary<string, IProxyFieldInfo> _fields = new Dictionary<string, IProxyFieldInfo>();
        private readonly Dictionary<string, IProxyPropertyInfo> _properties = new Dictionary<string, IProxyPropertyInfo>();
        private readonly Dictionary<string, List<IProxyMethodInfo>> _methods = new Dictionary<string, List<IProxyMethodInfo>>();
        private IProxyItemInfo _itemInfo;

        private readonly object _lock = new object();
        private readonly ProxyFactory _factory;
        private readonly Type _type;
        private ProxyType _baseType;

        public ProxyType(Type type, ProxyFactory factory)
        {
            _factory = factory;
            _type = type;
        }

        public Type Type => _type;

        private void AddMethodInfo(IProxyMethodInfo methodInfo)
        {
            lock (_lock)
            {
                string name = methodInfo.Name;
                if (!_methods.TryGetValue(name, out var list))
                {
                    list = new List<IProxyMethodInfo>();
                    _methods.Add(name, list);
                }

                list.Add(methodInfo);
            }
        }

        private void RemoveMethodInfo(IProxyMethodInfo methodInfo)
        {
            lock (_lock)
            {
                string name = methodInfo.Name;
                if (!_methods.TryGetValue(name, out var list))
                    return;

                list.Remove(methodInfo);
            }
        }

        private IProxyMethodInfo GetMethodInfo(string name, Type[] parameterTypes)
        {
            lock (_lock)
            {
                if (!_methods.ContainsKey(name))
                    return null;

                List<IProxyMethodInfo> list = _methods[name];
                foreach (IProxyMethodInfo info in list)
                {
                    if (IsParameterMatch(info, parameterTypes))
                        return info;
                }

                return null;
            }
        }

        private bool IsParameterMatch(IProxyMethodInfo proxyMethodInfo, Type[] parameterTypes)
        {
            ParameterInfo[] parameters = proxyMethodInfo.Parameters;
            if ((parameters == null || parameters.Length == 0) && (parameterTypes == null || parameterTypes.Length == 0))
                return true;

            if (parameters != null && parameterTypes != null && parameters.Length == parameterTypes.Length)
            {
                for (int i = 0; i < parameters.Length; i++)
                {
                    if (parameters[i].ParameterType != parameterTypes[i])
                        return false;
                }

                return true;
            }

            return false;
        }

        public void Register(IProxyMemberInfo memberInfo)
        {
            if (!(memberInfo.DeclaringType == _type))
                throw new ArgumentException();

            string name = memberInfo.Name;
            if (memberInfo is IProxyPropertyInfo info)
            {
                _properties.Add(name, info);
            }
            else if (memberInfo is IProxyMethodInfo methodInfo)
            {
                AddMethodInfo(methodInfo);
            }
            else if (memberInfo is IProxyFieldInfo fieldInfo)
            {
                _fields.Add(name, fieldInfo);
            }
            else if (memberInfo is IProxyEventInfo eventInfo)
            {
                _events.Add(name, eventInfo);
            }
            else if (memberInfo is IProxyItemInfo proxyItemInfo)
            {
                _itemInfo = proxyItemInfo;
            }
        }

        public void Unregister(IProxyMemberInfo memberInfo)
        {
            if (!(memberInfo.DeclaringType == _type))
                throw new ArgumentException();

            string name = memberInfo.Name;
            if (memberInfo is IProxyPropertyInfo)
            {
                _properties.Remove(name);
            }
            else if (memberInfo is IProxyMethodInfo info)
            {
                RemoveMethodInfo(info);
            }
            else if (memberInfo is IProxyFieldInfo)
            {
                _fields.Remove(name);
            }
            else if (memberInfo is IProxyEventInfo)
            {
                _events.Remove(name);
            }
            else if (memberInfo is IProxyItemInfo)
            {
                if (_itemInfo == memberInfo)
                    _itemInfo = null;
            }
        }

        private IProxyType GetBase()
        {
            if (_baseType != null)
                return _baseType;

            Type baseType = _type.BaseType;
            if (baseType == null)
                return null;

            _baseType = _factory.GetType(baseType);
            return _baseType;
        }

        public IProxyMemberInfo GetMember(string name)
        {
            if (name.Equals("Item") && typeof(ICollection).IsAssignableFrom(_type))
            {
                return GetItem();
            }

            IProxyMemberInfo info = GetProperty(name);
            if (info != null)
                return info;

            info = GetMethod(name);
            if (info != null)
                return info;

            info = GetField(name);
            if (info != null)
                return info;

            info = GetEvent(name);
            if (info != null)
                return info;

            return null;
        }

        public IProxyMemberInfo GetMember(string name, BindingFlags flags)
        {
            if (name.Equals("Item") && typeof(ICollection).IsAssignableFrom(_type))
                return GetItem();

            IProxyMemberInfo info = GetProperty(name, flags);
            if (info != null)
                return info;

            info = GetMethod(name, flags);
            if (info != null)
                return info;

            info = GetField(name, flags);
            if (info != null)
                return info;

            info = GetEvent(name);
            if (info != null)
                return info;

            return null;
        }

        public IProxyEventInfo GetEvent(string name)
        {
            if (_events.TryGetValue(name, out var info))
                return info;

            return FindEventInfo(name, DEFAULT_LOOKUP, true);
        }

        private IProxyEventInfo FindEventInfo(string name, BindingFlags flags, bool includeInterface)
        {
            IProxyEventInfo info = null;
            EventInfo eventInfo = _type.GetEvent(name, flags | BindingFlags.DeclaredOnly);
            if (eventInfo != null)
            {
                if (_events.TryGetValue(eventInfo.Name, out info))
                    return info;
                return CreateProxyEventInfo(eventInfo);
            }

            if (_type.BaseType != null && !(_type.BaseType == typeof(Object)))
            {
                if (_baseType != null)
                {
                    info = _baseType.FindEventInfo(name, flags, false);
                }
                else if (_type.BaseType.GetEvent(name, flags & ~BindingFlags.DeclaredOnly) != null)
                {
                    _baseType = _factory.GetType(_type.BaseType, true);
                    info = _baseType.FindEventInfo(name, flags, false);
                }

                if (info != null)
                    return info;
            }

            if (includeInterface)
            {
                Type[] types = _type.GetInterfaces();
                foreach (Type interfaceType in types)
                {
                    ProxyType proxyType = _factory.GetType(interfaceType, false);
                    if (proxyType == null && interfaceType.GetEvent(name, flags | BindingFlags.DeclaredOnly) != null)
                        proxyType = _factory.GetType(interfaceType, true);

                    if (proxyType == null)
                        continue;

                    info = proxyType.FindEventInfo(name, flags, false);
                    if (info != null)
                        return info;
                }
            }

            return null;
        }

        public IProxyFieldInfo GetField(string name)
        {
            if (_fields.TryGetValue(name, out var info))
                return info;

            return FindFieldInfo(name, DEFAULT_LOOKUP, true);
        }

        public IProxyFieldInfo GetField(string name, BindingFlags flags)
        {
            return FindFieldInfo(name, flags, true);
        }

        private IProxyFieldInfo FindFieldInfo(string name, BindingFlags flags, bool includeInterface)
        {
            IProxyFieldInfo info = null;
            FieldInfo fieldInfo = _type.GetField(name, flags | BindingFlags.DeclaredOnly);
            if (fieldInfo != null)
            {
                if (_fields.TryGetValue(fieldInfo.Name, out info))
                    return info;
                return CreateProxyFieldInfo(fieldInfo);
            }

            if (_type.BaseType != null && !(_type.BaseType == typeof(Object)))
            {
                if (_baseType != null)
                {
                    info = _baseType.FindFieldInfo(name, flags, false);
                }
                else if (_type.BaseType.GetField(name, flags & ~BindingFlags.DeclaredOnly) != null)
                {
                    _baseType = _factory.GetType(_type.BaseType, true);
                    info = _baseType.FindFieldInfo(name, flags, false);
                }

                if (info != null)
                    return info;
            }

            if (includeInterface)
            {
                Type[] types = _type.GetInterfaces();
                foreach (Type interfaceType in types)
                {
                    ProxyType proxyType = _factory.GetType(interfaceType, false);
                    if (proxyType == null && interfaceType.GetField(name, flags | BindingFlags.DeclaredOnly) != null)
                        proxyType = _factory.GetType(interfaceType, true);

                    if (proxyType == null)
                        continue;

                    info = proxyType.FindFieldInfo(name, flags, false);
                    if (info != null)
                        return info;
                }
            }

            return null;
        }

        public IProxyPropertyInfo GetProperty(string name)
        {
            if (_properties.TryGetValue(name, out var info))
                return info;

            return FindPropertyInfo(name, DEFAULT_LOOKUP, true);
        }

        public IProxyPropertyInfo GetProperty(string name, BindingFlags flags)
        {
            return FindPropertyInfo(name, flags, true);
        }

        private IProxyPropertyInfo FindPropertyInfo(string name, BindingFlags flags, bool includeInterface)
        {
            IProxyPropertyInfo info = null;
            PropertyInfo propertyInfo = _type.GetProperty(name, flags | BindingFlags.DeclaredOnly);

            if (propertyInfo != null)
            {
                if (_properties.TryGetValue(propertyInfo.Name, out info))
                    return info;
                return CreateProxyPropertyInfo(propertyInfo);
            }

            if (_type.BaseType != null && !(_type.BaseType == typeof(Object)))
            {
                if (_baseType != null)
                {
                    info = _baseType.FindPropertyInfo(name, flags, false);
                }
                else if (_type.BaseType.GetProperty(name, flags & ~BindingFlags.DeclaredOnly) != null)
                {
                    _baseType = _factory.GetType(_type.BaseType, true);
                    info = _baseType.FindPropertyInfo(name, flags, false);
                }

                if (info != null)
                    return info;
            }

            if (includeInterface)
            {
                Type[] types = _type.GetInterfaces();
                foreach (Type interfaceType in types)
                {
                    ProxyType proxyType = _factory.GetType(interfaceType, false);
                    if (proxyType == null && interfaceType.GetProperty(name, flags | BindingFlags.DeclaredOnly) != null)
                        proxyType = _factory.GetType(interfaceType, true);

                    if (proxyType == null)
                        continue;

                    info = proxyType.FindPropertyInfo(name, flags, false);
                    if (info != null)
                        return info;
                }
            }

            return null;
        }

        public IProxyItemInfo GetItem()
        {
            if (_itemInfo != null)
                return _itemInfo;

            if (_type.IsArray)
            {
                return CreateArrayProxyItemInfo(_type);
            }

            PropertyInfo propertyInfo = _type.GetProperty("Item", BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly);
            if (propertyInfo != null)
                return CreateProxyItemInfo(propertyInfo);

            IProxyType baseTypeInfo = GetBase();
            if (baseTypeInfo == null)
                return null;

            return baseTypeInfo.GetItem();
        }

        public IProxyMethodInfo GetMethod(string name)
        {
            MethodInfo methodInfo = _type.GetMethod(name);
            if (methodInfo == null)
                return null;

            return GetMethod(name, methodInfo.GetParameterTypes().ToArray());
        }

        public IProxyMethodInfo GetMethod(string name, Type[] parameterTypes)
        {
            IProxyMethodInfo info = GetMethodInfo(name, parameterTypes);
            if (info != null)
                return info;

            return FindMethodInfo(name, parameterTypes, DEFAULT_LOOKUP, true);
        }

        public IProxyMethodInfo GetMethod(string name, BindingFlags flags)
        {
            MethodInfo methodInfo = _type.GetMethod(name, flags);
            if (methodInfo == null)
                return null;

            return GetMethod(name, methodInfo.GetParameterTypes().ToArray(), flags);
        }

        public IProxyMethodInfo GetMethod(string name, Type[] parameterTypes, BindingFlags flags)
        {
            return FindMethodInfo(name, parameterTypes, flags, true);
        }

        private IProxyMethodInfo FindMethodInfo(string name, Type[] parameterTypes, BindingFlags flags, bool includeInterface)
        {
            IProxyMethodInfo info = null;
            MethodInfo methodInfo = _type.GetMethod(name, flags | BindingFlags.DeclaredOnly, null, parameterTypes, null);
            if (methodInfo != null)
            {
                info = GetMethodInfo(name, parameterTypes);
                if (info != null)
                    return info;
                return CreateProxyMethodInfo(methodInfo);
            }

            if (_type.BaseType != null)
            {
                if (_baseType != null)
                {
                    info = _baseType.FindMethodInfo(name, parameterTypes, flags, false);
                }
                else if (_type.BaseType.GetMethod(name, flags & ~BindingFlags.DeclaredOnly) != null)
                {
                    _baseType = _factory.GetType(_type.BaseType, true);
                    info = _baseType.FindMethodInfo(name, parameterTypes, flags, false);
                }

                if (info != null)
                    return info;
            }

            if (includeInterface)
            {
                Type[] types = _type.GetInterfaces();
                foreach (Type interfaceType in types)
                {
                    ProxyType proxyType = _factory.GetType(interfaceType, false);
                    if (proxyType == null && interfaceType.GetMethod(name, flags | BindingFlags.DeclaredOnly, null, parameterTypes, null) != null)
                        proxyType = _factory.GetType(interfaceType, true);

                    if (proxyType == null)
                        continue;

                    info = proxyType.FindMethodInfo(name, parameterTypes, flags, false);
                    if (info != null)
                        return info;
                }
            }

            return null;
        }

        private IProxyEventInfo CreateProxyEventInfo(EventInfo eventInfo)
        {
            ProxyEventInfo info = new ProxyEventInfo(eventInfo);
            _events.Add(info.Name, info);
            return info;
        }

        private IProxyFieldInfo CreateProxyFieldInfo(FieldInfo fieldInfo)
        {
            IProxyFieldInfo info = null;
            try
            {
                info = (IProxyFieldInfo)Activator.CreateInstance(
                    typeof(ProxyFieldInfo<,>).MakeGenericType(fieldInfo.DeclaringType, fieldInfo.FieldType), fieldInfo);
            }
            catch (Exception)
            {
                info = new ProxyFieldInfo(fieldInfo);
            }

            if (info != null)
                _fields.Add(info.Name, info);
            return info;
        }

        private IProxyPropertyInfo CreateProxyPropertyInfo(PropertyInfo propertyInfo)
        {
            IProxyPropertyInfo info = null;
            try
            {
                Type type = propertyInfo.DeclaringType;
                if (propertyInfo.IsStatic())
                {
                    ParameterInfo[] parameters = propertyInfo.GetIndexParameters();
                    if (parameters != null && parameters.Length > 0)
                        throw new Exception();

                    info = (IProxyPropertyInfo)Activator.CreateInstance(
                        typeof(StaticProxyPropertyInfo<,>).MakeGenericType(type, propertyInfo.PropertyType), propertyInfo);
                }
                else
                {
                    ParameterInfo[] parameters = propertyInfo.GetIndexParameters();
                    if (parameters != null && parameters.Length == 1)
                        throw new Exception();

                    info = (IProxyPropertyInfo)Activator.CreateInstance(typeof(ProxyPropertyInfo<,>).MakeGenericType(type, propertyInfo.PropertyType),
                        propertyInfo);
                }
            }
            catch (Exception)
            {
                info = new ProxyPropertyInfo(propertyInfo);
            }

            if (info != null)
                _properties.Add(info.Name, info);
            return info;
        }

        private IProxyMethodInfo CreateProxyMethodInfo(MethodInfo methodInfo)
        {
            IProxyMethodInfo info = null;
            try
            {
                Type returnType = methodInfo.ReturnType;
                ParameterInfo[] parameters = methodInfo.GetParameters();
                Type type = methodInfo.DeclaringType;
                if (typeof(void) == returnType)
                {
                    if (methodInfo.IsStatic)
                    {
                        if (parameters.Length == 0)
                        {
                            info = (IProxyMethodInfo)Activator.CreateInstance(typeof(StaticProxyActionInfo<>).MakeGenericType(type), methodInfo);
                        }
                        else if (parameters.Length == 1)
                        {
                            info = (IProxyMethodInfo)Activator.CreateInstance(
                                typeof(StaticProxyActionInfo<,>).MakeGenericType(type, parameters[0].ParameterType), methodInfo);
                        }
                        else if (parameters.Length == 2)
                        {
                            info = (IProxyMethodInfo)Activator.CreateInstance(
                                typeof(StaticProxyActionInfo<,,>).MakeGenericType(type, parameters[0].ParameterType, parameters[1].ParameterType),
                                methodInfo);
                        }
                        else if (parameters.Length == 3)
                        {
                            info = (IProxyMethodInfo)Activator.CreateInstance(
                                typeof(StaticProxyActionInfo<,,,>).MakeGenericType(type, parameters[0].ParameterType, parameters[1].ParameterType,
                                    parameters[2].ParameterType), methodInfo);
                        }
                        else
                        {
                            info = new ProxyMethodInfo(methodInfo);
                        }
                    }
                    else
                    {
                        if (parameters.Length == 0)
                        {
                            info = (IProxyMethodInfo)Activator.CreateInstance(typeof(ProxyActionInfo<>).MakeGenericType(type), methodInfo);
                        }
                        else if (parameters.Length == 1)
                        {
                            info = (IProxyMethodInfo)Activator.CreateInstance(
                                typeof(ProxyActionInfo<,>).MakeGenericType(type, parameters[0].ParameterType), methodInfo);
                        }
                        else if (parameters.Length == 2)
                        {
                            info = (IProxyMethodInfo)Activator.CreateInstance(
                                typeof(ProxyActionInfo<,,>).MakeGenericType(type, parameters[0].ParameterType, parameters[1].ParameterType),
                                methodInfo);
                        }
                        else if (parameters.Length == 3)
                        {
                            info = (IProxyMethodInfo)Activator.CreateInstance(
                                typeof(ProxyActionInfo<,,,>).MakeGenericType(type, parameters[0].ParameterType, parameters[1].ParameterType,
                                    parameters[2].ParameterType), methodInfo);
                        }
                        else
                        {
                            info = new ProxyMethodInfo(methodInfo);
                        }
                    }
                }
                else
                {
                    if (methodInfo.IsStatic)
                    {
                        if (parameters.Length == 0)
                        {
                            info = (IProxyMethodInfo)Activator.CreateInstance(typeof(StaticProxyFuncInfo<,>).MakeGenericType(type, returnType),
                                methodInfo);
                        }
                        else if (parameters.Length == 1)
                        {
                            info = (IProxyMethodInfo)Activator.CreateInstance(
                                typeof(StaticProxyFuncInfo<,,>).MakeGenericType(type, parameters[0].ParameterType, returnType), methodInfo);
                        }
                        else if (parameters.Length == 2)
                        {
                            info = (IProxyMethodInfo)Activator.CreateInstance(
                                typeof(StaticProxyFuncInfo<,,,>).MakeGenericType(type, parameters[0].ParameterType, parameters[1].ParameterType,
                                    returnType), methodInfo);
                        }
                        else if (parameters.Length == 3)
                        {
                            info = (IProxyMethodInfo)Activator.CreateInstance(
                                typeof(StaticProxyFuncInfo<,,,,>).MakeGenericType(type, parameters[0].ParameterType, parameters[1].ParameterType,
                                    parameters[2].ParameterType, returnType), methodInfo);
                        }
                        else
                        {
                            info = new ProxyMethodInfo(methodInfo);
                        }
                    }
                    else
                    {
                        if (parameters.Length == 0)
                        {
                            info = (IProxyMethodInfo)Activator.CreateInstance(typeof(ProxyFuncInfo<,>).MakeGenericType(type, returnType), methodInfo);
                        }
                        else if (parameters.Length == 1)
                        {
                            info = (IProxyMethodInfo)Activator.CreateInstance(
                                typeof(ProxyFuncInfo<,,>).MakeGenericType(type, parameters[0].ParameterType, returnType), methodInfo);
                        }
                        else if (parameters.Length == 2)
                        {
                            info = (IProxyMethodInfo)Activator.CreateInstance(
                                typeof(ProxyFuncInfo<,,,>).MakeGenericType(type, parameters[0].ParameterType, parameters[1].ParameterType,
                                    returnType), methodInfo);
                        }
                        else if (parameters.Length == 3)
                        {
                            info = (IProxyMethodInfo)Activator.CreateInstance(
                                typeof(ProxyFuncInfo<,,,,>).MakeGenericType(type, parameters[0].ParameterType, parameters[1].ParameterType,
                                    parameters[2].ParameterType, returnType), methodInfo);
                        }
                        else
                        {
                            info = new ProxyMethodInfo(methodInfo);
                        }
                    }
                }
            }
            catch (Exception)
            {
                info = new ProxyMethodInfo(methodInfo);
            }

            if (info != null)
                AddMethodInfo(info);

            return info;
        }

        private IProxyItemInfo CreateArrayProxyItemInfo(Type type)
        {
            var elementType = type.GetElementType();

            IProxyItemInfo info = null;
            try
            {
                info = (IProxyItemInfo)Activator.CreateInstance(typeof(ArrayProxyItemInfo<,>).MakeGenericType(type, elementType));
            }
            catch (Exception)
            {
                var code = Type.GetTypeCode(elementType);
                switch (code)
                {
                    case TypeCode.Boolean:
                        info = new ArrayProxyItemInfo<bool[], bool>();
                        break;
                    case TypeCode.Byte:
                        info = new ArrayProxyItemInfo<byte[], byte>();
                        break;
                    case TypeCode.Char:
                        info = new ArrayProxyItemInfo<char[], char>();
                        break;
                    case TypeCode.DateTime:
                        info = new ArrayProxyItemInfo<DateTime[], DateTime>();
                        break;
                    case TypeCode.Decimal:
                        info = new ArrayProxyItemInfo<Decimal[], Decimal>();
                        break;
                    case TypeCode.Double:
                        info = new ArrayProxyItemInfo<Double[], Double>();
                        break;
                    case TypeCode.Int16:
                        info = new ArrayProxyItemInfo<Int16[], Int16>();
                        break;
                    case TypeCode.Int32:
                        info = new ArrayProxyItemInfo<Int32[], Int32>();
                        break;
                    case TypeCode.Int64:
                        info = new ArrayProxyItemInfo<Int64[], Int64>();
                        break;
                    case TypeCode.SByte:
                        info = new ArrayProxyItemInfo<SByte[], SByte>();
                        break;
                    case TypeCode.Single:
                        info = new ArrayProxyItemInfo<Single[], Single>();
                        break;
                    case TypeCode.String:
                        info = new ArrayProxyItemInfo<string[], string>();
                        break;
                    case TypeCode.UInt16:
                        info = new ArrayProxyItemInfo<UInt16[], UInt16>();
                        break;
                    case TypeCode.UInt32:
                        info = new ArrayProxyItemInfo<UInt32[], UInt32>();
                        break;
                    case TypeCode.UInt64:
                        info = new ArrayProxyItemInfo<UInt64[], UInt64>();
                        break;
                    case TypeCode.Object:
                    {
                        if (type == typeof(Vector2))
                            info = new ArrayProxyItemInfo<Vector2[], Vector2>();
                        else if (type == typeof(Vector3))
                            info = new ArrayProxyItemInfo<Vector3[], Vector3>();
                        else if (type == typeof(Vector4))
                            info = new ArrayProxyItemInfo<Vector4[], Vector4>();
                        else if (type == typeof(Color))
                            info = new ArrayProxyItemInfo<Color[], Color>();
                        else if (type == typeof(Rect))
                            info = new ArrayProxyItemInfo<Rect[], Rect>();
                        else if (type == typeof(Quaternion))
                            info = new ArrayProxyItemInfo<Quaternion[], Quaternion>();
                        else if (type == typeof(Version))
                            info = new ArrayProxyItemInfo<Version[], Version>();
                        else
                            info = new ArrayProxyItemInfo(type);
                        break;
                    }
                    default:
                        info = new ArrayProxyItemInfo(type);
                        break;
                }
            }

            if (info != null)
                _itemInfo = info;
            return info;
        }

        private IProxyItemInfo CreateProxyItemInfo(PropertyInfo propertyInfo)
        {
            Type type = propertyInfo.DeclaringType;
            ParameterInfo[] parameters = propertyInfo.GetIndexParameters();
            if (parameters == null || parameters.Length != 1)
                throw new NotSupportedException();

            Type keyType = parameters[0].ParameterType;
            Type valueType = propertyInfo.PropertyType;
            int typeFlag = TypeFlag(type, keyType, valueType);

            IProxyItemInfo info = null;
            try
            {
                if (typeFlag == 1)
                {
                    info = (IProxyItemInfo)Activator.CreateInstance(typeof(DictionaryProxyItemInfo<,,>).MakeGenericType(type, keyType, valueType),
                        propertyInfo);
                }
                else if (typeFlag == 2)
                {
                    info = (IProxyItemInfo)Activator.CreateInstance(typeof(ListProxyItemInfo<,>).MakeGenericType(type, valueType), propertyInfo);
                }
                else
                {
                    info = new ProxyItemInfo(propertyInfo);
                }
            }
            catch (Exception)
            {
                if (typeFlag == 1)
                {
                    info = CreateDictionaryProxyItemInfo(propertyInfo);
                }
                else if (typeFlag == 2)
                {
                    info = CreateListProxyItemInfo(propertyInfo);
                }
                else
                {
                    info = new ProxyItemInfo(propertyInfo);
                }
            }

            if (info != null)
                _itemInfo = info;
            return info;
        }

        private int TypeFlag(Type type, Type keyType, Type valueType)
        {
            try
            {
                if (keyType == typeof(int) && typeof(IList<>).MakeGenericType(valueType).IsAssignableFrom(type))
                    return 2;
                if (typeof(IDictionary<,>).MakeGenericType(keyType, valueType).IsAssignableFrom(type))
                    return 1;
                return 0;
            }
            catch (Exception)
            {
                return 0;
            }
        }

        private IProxyItemInfo CreateListProxyItemInfo(PropertyInfo propertyInfo)
        {
            var type = propertyInfo.PropertyType;
            TypeCode typeCode = Type.GetTypeCode(type);
            switch (typeCode)
            {
                case TypeCode.Boolean:
                    return new ListProxyItemInfo<IList<bool>, bool>(propertyInfo);
                case TypeCode.Byte:
                    return new ListProxyItemInfo<IList<byte>, byte>(propertyInfo);
                case TypeCode.Char:
                    return new ListProxyItemInfo<IList<char>, char>(propertyInfo);
                case TypeCode.DateTime:
                    return new ListProxyItemInfo<IList<DateTime>, DateTime>(propertyInfo);
                case TypeCode.Decimal:
                    return new ListProxyItemInfo<IList<Decimal>, Decimal>(propertyInfo);
                case TypeCode.Double:
                    return new ListProxyItemInfo<IList<Double>, Double>(propertyInfo);
                case TypeCode.Int16:
                    return new ListProxyItemInfo<IList<Int16>, Int16>(propertyInfo);
                case TypeCode.Int32:
                    return new ListProxyItemInfo<IList<Int32>, Int32>(propertyInfo);
                case TypeCode.Int64:
                    return new ListProxyItemInfo<IList<Int64>, Int64>(propertyInfo);
                case TypeCode.SByte:
                    return new ListProxyItemInfo<IList<SByte>, SByte>(propertyInfo);
                case TypeCode.Single:
                    return new ListProxyItemInfo<IList<Single>, Single>(propertyInfo);
                case TypeCode.String:
                    return new ListProxyItemInfo<IList<String>, String>(propertyInfo);
                case TypeCode.UInt16:
                    return new ListProxyItemInfo<IList<UInt16>, UInt16>(propertyInfo);
                case TypeCode.UInt32:
                    return new ListProxyItemInfo<IList<UInt32>, UInt32>(propertyInfo);
                case TypeCode.UInt64:
                    return new ListProxyItemInfo<IList<UInt64>, UInt64>(propertyInfo);
                case TypeCode.Object:
                {
                    if (type == typeof(Vector2))
                        return new ListProxyItemInfo<IList<Vector2>, Vector2>(propertyInfo);
                    if (type == typeof(Vector3))
                        return new ListProxyItemInfo<IList<Vector3>, Vector3>(propertyInfo);
                    if (type == typeof(Vector4))
                        return new ListProxyItemInfo<IList<Vector4>, Vector4>(propertyInfo);
                    if (type == typeof(Color))
                        return new ListProxyItemInfo<IList<Color>, Color>(propertyInfo);
                    if (type == typeof(Rect))
                        return new ListProxyItemInfo<IList<Rect>, Rect>(propertyInfo);
                    if (type == typeof(Quaternion))
                        return new ListProxyItemInfo<IList<Quaternion>, Quaternion>(propertyInfo);
                    if (type == typeof(Version))
                        return new ListProxyItemInfo<IList<Version>, Version>(propertyInfo);

                    return new ProxyItemInfo(propertyInfo);
                }
                default:
                    return new ProxyItemInfo(propertyInfo);
            }
        }

        private IProxyItemInfo CreateDictionaryProxyItemInfo(PropertyInfo propertyInfo)
        {
            ParameterInfo[] parameters = propertyInfo.GetIndexParameters();
            var type = propertyInfo.PropertyType;
            TypeCode typeCode = Type.GetTypeCode(type);
            if (parameters[0].ParameterType == typeof(string))
            {
                switch (typeCode)
                {
                    case TypeCode.Boolean:
                        return new DictionaryProxyItemInfo<IDictionary<string, bool>, string, bool>(propertyInfo);
                    case TypeCode.Byte:
                        return new DictionaryProxyItemInfo<IDictionary<string, byte>, string, byte>(propertyInfo);
                    case TypeCode.Char:
                        return new DictionaryProxyItemInfo<IDictionary<string, char>, string, char>(propertyInfo);
                    case TypeCode.DateTime:
                        return new DictionaryProxyItemInfo<IDictionary<string, DateTime>, string, DateTime>(propertyInfo);
                    case TypeCode.Decimal:
                        return new DictionaryProxyItemInfo<IDictionary<string, Decimal>, string, Decimal>(propertyInfo);
                    case TypeCode.Double:
                        return new DictionaryProxyItemInfo<IDictionary<string, Double>, string, Double>(propertyInfo);
                    case TypeCode.Int16:
                        return new DictionaryProxyItemInfo<IDictionary<string, Int16>, string, Int16>(propertyInfo);
                    case TypeCode.Int32:
                        return new DictionaryProxyItemInfo<IDictionary<string, Int32>, string, Int32>(propertyInfo);
                    case TypeCode.Int64:
                        return new DictionaryProxyItemInfo<IDictionary<string, Int64>, string, Int64>(propertyInfo);
                    case TypeCode.SByte:
                        return new DictionaryProxyItemInfo<IDictionary<string, SByte>, string, SByte>(propertyInfo);
                    case TypeCode.Single:
                        return new DictionaryProxyItemInfo<IDictionary<string, Single>, string, Single>(propertyInfo);
                    case TypeCode.String:
                        return new DictionaryProxyItemInfo<IDictionary<string, string>, string, string>(propertyInfo);
                    case TypeCode.UInt16:
                        return new DictionaryProxyItemInfo<IDictionary<string, UInt16>, string, UInt16>(propertyInfo);
                    case TypeCode.UInt32:
                        return new DictionaryProxyItemInfo<IDictionary<string, UInt32>, string, UInt32>(propertyInfo);
                    case TypeCode.UInt64:
                        return new DictionaryProxyItemInfo<IDictionary<string, UInt64>, string, UInt64>(propertyInfo);
                    case TypeCode.Object:
                    {
                        if (type == typeof(Vector2))
                            return new DictionaryProxyItemInfo<IDictionary<string, Vector2>, string, Vector2>(propertyInfo);
                        if (type == typeof(Vector3))
                            return new DictionaryProxyItemInfo<IDictionary<string, Vector3>, string, Vector3>(propertyInfo);
                        if (type == typeof(Vector4))
                            return new DictionaryProxyItemInfo<IDictionary<string, Vector4>, string, Vector4>(propertyInfo);
                        if (type == typeof(Color))
                            return new DictionaryProxyItemInfo<IDictionary<string, Color>, string, Color>(propertyInfo);
                        if (type == typeof(Rect))
                            return new DictionaryProxyItemInfo<IDictionary<string, Rect>, string, Rect>(propertyInfo);
                        if (type == typeof(Quaternion))
                            return new DictionaryProxyItemInfo<IDictionary<string, Quaternion>, string, Quaternion>(propertyInfo);
                        if (type == typeof(Version))
                            return new DictionaryProxyItemInfo<IDictionary<string, Version>, string, Version>(propertyInfo);

                        return new ProxyItemInfo(propertyInfo);
                    }
                    default:
                        return new ProxyItemInfo(propertyInfo);
                }
            }

            if (parameters[0].ParameterType == typeof(int))
            {
                switch (typeCode)
                {
                    case TypeCode.Boolean:
                        return new DictionaryProxyItemInfo<IDictionary<int, bool>, int, bool>(propertyInfo);
                    case TypeCode.Byte:
                        return new DictionaryProxyItemInfo<IDictionary<int, byte>, int, byte>(propertyInfo);
                    case TypeCode.Char:
                        return new DictionaryProxyItemInfo<IDictionary<int, char>, int, char>(propertyInfo);
                    case TypeCode.DateTime:
                        return new DictionaryProxyItemInfo<IDictionary<int, DateTime>, int, DateTime>(propertyInfo);
                    case TypeCode.Decimal:
                        return new DictionaryProxyItemInfo<IDictionary<int, Decimal>, int, Decimal>(propertyInfo);
                    case TypeCode.Double:
                        return new DictionaryProxyItemInfo<IDictionary<int, Double>, int, Double>(propertyInfo);
                    case TypeCode.Int16:
                        return new DictionaryProxyItemInfo<IDictionary<int, Int16>, int, Int16>(propertyInfo);
                    case TypeCode.Int32:
                        return new DictionaryProxyItemInfo<IDictionary<int, Int32>, int, Int32>(propertyInfo);
                    case TypeCode.Int64:
                        return new DictionaryProxyItemInfo<IDictionary<int, Int64>, int, Int64>(propertyInfo);
                    case TypeCode.SByte:
                        return new DictionaryProxyItemInfo<IDictionary<int, SByte>, int, SByte>(propertyInfo);
                    case TypeCode.Single:
                        return new DictionaryProxyItemInfo<IDictionary<int, Single>, int, Single>(propertyInfo);
                    case TypeCode.String:
                        return new DictionaryProxyItemInfo<IDictionary<int, string>, int, string>(propertyInfo);
                    case TypeCode.UInt16:
                        return new DictionaryProxyItemInfo<IDictionary<int, UInt16>, int, UInt16>(propertyInfo);
                    case TypeCode.UInt32:
                        return new DictionaryProxyItemInfo<IDictionary<int, UInt32>, int, UInt32>(propertyInfo);
                    case TypeCode.UInt64:
                        return new DictionaryProxyItemInfo<IDictionary<int, UInt64>, int, UInt64>(propertyInfo);
                    case TypeCode.Object:
                    {
                        if (type == typeof(Vector2))
                            return new DictionaryProxyItemInfo<IDictionary<int, Vector2>, int, Vector2>(propertyInfo);
                        if (type == typeof(Vector3))
                            return new DictionaryProxyItemInfo<IDictionary<int, Vector3>, int, Vector3>(propertyInfo);
                        if (type == typeof(Vector4))
                            return new DictionaryProxyItemInfo<IDictionary<int, Vector4>, int, Vector4>(propertyInfo);
                        if (type == typeof(Color))
                            return new DictionaryProxyItemInfo<IDictionary<int, Color>, int, Color>(propertyInfo);
                        if (type == typeof(Rect))
                            return new DictionaryProxyItemInfo<IDictionary<int, Rect>, int, Rect>(propertyInfo);
                        if (type == typeof(Quaternion))
                            return new DictionaryProxyItemInfo<IDictionary<int, Quaternion>, int, Quaternion>(propertyInfo);
                        if (type == typeof(Version))
                            return new DictionaryProxyItemInfo<IDictionary<int, Version>, int, Version>(propertyInfo);

                        return new ProxyItemInfo(propertyInfo);
                    }
                    default:
                        return new ProxyItemInfo(propertyInfo);
                }
            }

            return new ProxyItemInfo(propertyInfo);
        }
    }
}