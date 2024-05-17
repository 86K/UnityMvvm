using System;
using System.Collections;
using System.Reflection;

namespace Fusion.Mvvm
{
    public class UniversalNodeProxyFactory : INodeProxyFactory
    {
        public ISourceProxy Create(object source, PathToken token)
        {
            IPathNode node = token.Current;
            if (source == null && !node.IsStatic)
                return null;

            if (node.IsStatic)
                return CreateStaticProxy(node);

            return CreateProxy(source, node);
        }

        private ISourceProxy CreateProxy(object source, IPathNode node)
        {
            Type type = source.GetType();
            if (node is IndexedNode)
            {
                IProxyType proxyType = type.AsProxy();
                if (!(source is ICollection collection))
                    throw new Exception($"Type \"{type.Name}\" is not a collection and cannot be accessed by index \"{node}\".");

                var itemInfo = proxyType.GetItem();
                if (itemInfo == null)
                    throw new MissingMemberException(type.FullName, "Item");

                if (node is IntegerIndexedNode intIndexedNode)
                    return new IntItemNodeProxy(collection, intIndexedNode.Value, itemInfo);

                if (node is StringIndexedNode stringIndexedNode)
                    return new StringItemNodeProxy(collection, stringIndexedNode.Value, itemInfo);

                return null;
            }

            if (!(node is MemberNode memberNode))
                return null;

            var memberInfo = memberNode.MemberInfo;
            if (memberInfo != null && memberInfo.DeclaringType != null && !memberInfo.DeclaringType.IsAssignableFrom(type))
                return null;

            if (memberInfo == null)
                memberInfo = type.FindFirstMemberInfo(memberNode.Name);

            if (memberInfo == null || memberInfo.IsStatic())
                throw new MissingMemberException(type.FullName, memberNode.Name);

            var propertyInfo = memberInfo as PropertyInfo;
            if (propertyInfo != null)
            {
                IProxyPropertyInfo proxyPropertyInfo = propertyInfo.AsProxy();
                var valueType = proxyPropertyInfo.ValueType;
                if (typeof(IObservableProperty).IsAssignableFrom(valueType))
                {
                    object observableValue = proxyPropertyInfo.GetValue(source);
                    if (observableValue == null)
                        return null;

                    return new ObservableNodeProxy(source, (IObservableProperty)observableValue);
                }

                if (typeof(IInteractionRequest).IsAssignableFrom(valueType))
                {
                    object request = proxyPropertyInfo.GetValue(source);
                    if (request == null)
                        return null;

                    return new InteractionNodeProxy(source, (IInteractionRequest)request);
                }

                return new PropertyNodeProxy(source, proxyPropertyInfo);
            }

            var fieldInfo = memberInfo as FieldInfo;
            if (fieldInfo != null)
            {
                IProxyFieldInfo proxyFieldInfo = fieldInfo.AsProxy();
                var valueType = proxyFieldInfo.ValueType;
                if (typeof(IObservableProperty).IsAssignableFrom(valueType))
                {
                    object observableValue = proxyFieldInfo.GetValue(source);
                    if (observableValue == null)
                        return null;

                    return new ObservableNodeProxy(source, (IObservableProperty)observableValue);
                }

                if (typeof(IInteractionRequest).IsAssignableFrom(valueType))
                {
                    object request = proxyFieldInfo.GetValue(source);
                    if (request == null)
                        return null;

                    return new InteractionNodeProxy(source, (IInteractionRequest)request);
                }

                return new FieldNodeProxy(source, proxyFieldInfo);
            }

            var methodInfo = memberInfo as MethodInfo;
            if (methodInfo != null && methodInfo.ReturnType == typeof(void))
                return new MethodNodeProxy(source, methodInfo.AsProxy());

            var eventInfo = memberInfo as EventInfo;
            if (eventInfo != null)
                return new EventNodeProxy(source, eventInfo.AsProxy());

            return null;
        }

        private ISourceProxy CreateStaticProxy(IPathNode node)
        {
            if (!(node is MemberNode memberNode))
                return null;

            Type type = memberNode.Type;
            var memberInfo = memberNode.MemberInfo;
            if (memberInfo == null)
                memberInfo = type.FindFirstMemberInfo(memberNode.Name, BindingFlags.Public | BindingFlags.Static);

            if (memberInfo == null)
                throw new MissingMemberException(type.FullName, memberNode.Name);

            var propertyInfo = memberInfo as PropertyInfo;
            if (propertyInfo != null)
            {
                var proxyPropertyInfo = propertyInfo.AsProxy();
                var valueType = proxyPropertyInfo.ValueType;
                if (typeof(IObservableProperty).IsAssignableFrom(valueType))
                {
                    object observableValue = proxyPropertyInfo.GetValue(null);
                    if (observableValue == null)
                        throw new NullReferenceException($"The \"{propertyInfo.Name}\" property is null in class \"{type.Name}\".");

                    return new ObservableNodeProxy((IObservableProperty)observableValue);
                }

                if (typeof(IInteractionRequest).IsAssignableFrom(valueType))
                {
                    object request = proxyPropertyInfo.GetValue(null);
                    if (request == null)
                        throw new NullReferenceException($"The \"{propertyInfo.Name}\" property is null in class \"{type.Name}\".");

                    return new InteractionNodeProxy((IInteractionRequest)request);
                }

                return new PropertyNodeProxy(proxyPropertyInfo);
            }

            var fieldInfo = memberInfo as FieldInfo;
            if (fieldInfo != null)
            {
                var proxyFieldInfo = fieldInfo.AsProxy();
                var valueType = proxyFieldInfo.ValueType;
                if (typeof(IObservableProperty).IsAssignableFrom(valueType))
                {
                    object observableValue = proxyFieldInfo.GetValue(null);
                    if (observableValue == null)
                        throw new NullReferenceException($"The \"{fieldInfo.Name}\" property is null in class \"{type.Name}\".");

                    return new ObservableNodeProxy((IObservableProperty)observableValue);
                }

                if (typeof(IInteractionRequest).IsAssignableFrom(valueType))
                {
                    object request = proxyFieldInfo.GetValue(null);
                    if (request == null)
                        throw new NullReferenceException($"The \"{fieldInfo.Name}\" property is null in class \"{type.Name}\".");

                    return new InteractionNodeProxy((IInteractionRequest)request);
                }

                return new FieldNodeProxy(proxyFieldInfo);
            }

            var methodInfo = memberInfo as MethodInfo;
            if (methodInfo != null && methodInfo.ReturnType == typeof(void))
                return new MethodNodeProxy(methodInfo.AsProxy());

            var eventInfo = memberInfo as EventInfo;
            if (eventInfo != null)
                return new EventNodeProxy(eventInfo.AsProxy());

            return null;
        }
    }
}