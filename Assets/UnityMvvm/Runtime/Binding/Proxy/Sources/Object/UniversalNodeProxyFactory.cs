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

        protected virtual ISourceProxy CreateProxy(object source, IPathNode node)
        {
            Type type = source.GetType();
            if (node is IndexedNode)
            {
                IProxyType proxyType = type.AsProxy();
                if (!(source is ICollection))
                    throw new ProxyException("Type \"{0}\" is not a collection and cannot be accessed by index \"{1}\".", type.Name, node.ToString());

                var itemInfo = proxyType.GetItem();
                if (itemInfo == null)
                    throw new MissingMemberException(type.FullName, "Item");

                var intIndexedNode = node as IntegerIndexedNode;
                if (intIndexedNode != null)
                    return new IntItemNodeProxy((ICollection)source, intIndexedNode.Value, itemInfo);

                var stringIndexedNode = node as StringIndexedNode;
                if (stringIndexedNode != null)
                    return new StringItemNodeProxy((ICollection)source, stringIndexedNode.Value, itemInfo);

                return null;
            }

            var memberNode = node as MemberNode;
            if (memberNode == null)
                return null;

            var memberInfo = memberNode.MemberInfo;
            if (memberInfo != null && !memberInfo.DeclaringType.IsAssignableFrom(type))
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
                else if (typeof(IInteractionRequest).IsAssignableFrom(valueType))
                {
                    object request = proxyPropertyInfo.GetValue(source);
                    if (request == null)
                        return null;

                    return new InteractionNodeProxy(source, (IInteractionRequest)request);
                }
                else
                {
                    return new PropertyNodeProxy(source, proxyPropertyInfo);
                }
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
                else if (typeof(IInteractionRequest).IsAssignableFrom(valueType))
                {
                    object request = proxyFieldInfo.GetValue(source);
                    if (request == null)
                        return null;

                    return new InteractionNodeProxy(source, (IInteractionRequest)request);
                }
                else
                {
                    return new FieldNodeProxy(source, proxyFieldInfo);
                }
            }

            var methodInfo = memberInfo as MethodInfo;
            if (methodInfo != null && methodInfo.ReturnType.Equals(typeof(void)))
                return new MethodNodeProxy(source, methodInfo.AsProxy());

            var eventInfo = memberInfo as EventInfo;
            if (eventInfo != null)
                return new EventNodeProxy(source, eventInfo.AsProxy());

            return null;
        }

        protected virtual ISourceProxy CreateStaticProxy(IPathNode node)
        {
            var memberNode = node as MemberNode;
            if (memberNode == null)
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
                else if (typeof(IInteractionRequest).IsAssignableFrom(valueType))
                {
                    object request = proxyPropertyInfo.GetValue(null);
                    if (request == null)
                        throw new NullReferenceException($"The \"{propertyInfo.Name}\" property is null in class \"{type.Name}\".");

                    return new InteractionNodeProxy((IInteractionRequest)request);
                }
                else
                {
                    return new PropertyNodeProxy(proxyPropertyInfo);
                }
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
                else if (typeof(IInteractionRequest).IsAssignableFrom(valueType))
                {
                    object request = proxyFieldInfo.GetValue(null);
                    if (request == null)
                        throw new NullReferenceException($"The \"{fieldInfo.Name}\" property is null in class \"{type.Name}\".");

                    return new InteractionNodeProxy((IInteractionRequest)request);
                }
                else
                {
                    return new FieldNodeProxy(proxyFieldInfo);
                }
            }

            var methodInfo = memberInfo as MethodInfo;
            if (methodInfo != null && methodInfo.ReturnType.Equals(typeof(void)))
                return new MethodNodeProxy(methodInfo.AsProxy());

            var eventInfo = memberInfo as EventInfo;
            if (eventInfo != null)
                return new EventNodeProxy(eventInfo.AsProxy());

            return null;
        }
    }
}
