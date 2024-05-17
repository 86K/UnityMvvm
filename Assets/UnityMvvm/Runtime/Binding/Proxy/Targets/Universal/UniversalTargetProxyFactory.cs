using System;
using System.Reflection;

namespace Fusion.Mvvm
{
    public class UniversalTargetProxyFactory : ITargetProxyFactory
    {
        private readonly IPathParser _pathParser;

        public UniversalTargetProxyFactory(IPathParser pathParser)
        {
            _pathParser = pathParser;
        }

        public ITargetProxy CreateProxy(object target, TargetDescription description)
        {
            IProxyType type = description.TargetType != null ? description.TargetType.AsProxy() : target.GetType().AsProxy();
            if (TargetNameUtil.IsCollection(description.TargetName))
                return CreateItemProxy(target, type, description);

            IProxyMemberInfo memberInfo = type.GetMember(description.TargetName);
            if (memberInfo == null)
                memberInfo = type.GetMember(description.TargetName, BindingFlags.Instance | BindingFlags.NonPublic);

            if (memberInfo == null)
                throw new MissingMemberException(type.Type.FullName, description.TargetName);

            if (memberInfo is IProxyPropertyInfo propertyInfo)
            {
                var valueType = propertyInfo.ValueType;
                if (typeof(IObservableProperty).IsAssignableFrom(valueType))
                {
                    object observableValue = propertyInfo.GetValue(target);
                    if (observableValue == null)
                        throw new NullReferenceException(
                            $"The \"{propertyInfo.Name}\" property is null in class \"{propertyInfo.DeclaringType.Name}\".");

                    return new ObservableTargetProxy(target, (IObservableProperty)observableValue);
                }

                if (typeof(IInteractionAction).IsAssignableFrom(valueType))
                {
                    object interactionAction = propertyInfo.GetValue(target);
                    if (interactionAction == null)
                        return null;

                    return new InteractionTargetProxy(target, (IInteractionAction)interactionAction);
                }

                return new PropertyTargetProxy(target, propertyInfo);
            }

            if (memberInfo is IProxyFieldInfo fieldInfo)
            {
                var valueType = fieldInfo.ValueType;
                if (typeof(IObservableProperty).IsAssignableFrom(valueType))
                {
                    object observableValue = fieldInfo.GetValue(target);
                    if (observableValue == null)
                        throw new NullReferenceException($"The \"{fieldInfo.Name}\" field is null in class \"{fieldInfo.DeclaringType.Name}\".");

                    return new ObservableTargetProxy(target, (IObservableProperty)observableValue);
                }

                if (typeof(IInteractionAction).IsAssignableFrom(valueType))
                {
                    object interactionAction = fieldInfo.GetValue(target);
                    if (interactionAction == null)
                        return null;

                    return new InteractionTargetProxy(target, (IInteractionAction)interactionAction);
                }

                return new FieldTargetProxy(target, fieldInfo);
            }

            if (memberInfo is IProxyEventInfo eventInfo)
                return new EventTargetProxy(target, eventInfo);

            if (memberInfo is IProxyMethodInfo methodInfo)
                return new MethodTargetProxy(target, methodInfo);

            return null;
        }

        private ITargetProxy CreateItemProxy(object target, IProxyType type, TargetDescription description)
        {
            Path path = _pathParser.Parse(description.TargetName);
            if (path.Count < 1 || path.Count > 2)
                return null;

            IndexedNode indexNode = null;
            object collectionTarget = null;
            if (path.Count == 1)
            {
                indexNode = (IndexedNode)path[0];
                collectionTarget = target;
            }

            if (path.Count == 2)
            {
                indexNode = (IndexedNode)path[1];
                MemberNode memberNode = (MemberNode)path[0];
                collectionTarget = GetCollectionTarget(type, target, memberNode.Name);
                if (collectionTarget == null)
                    throw new NullReferenceException(
                        $"Unable to bind the \"{description}\". The value of the Property or Field named \"{memberNode.Name}\" cannot be null.");
            }

            IProxyType proxyType = collectionTarget?.GetType().AsProxy();
            IProxyItemInfo itemInfo = proxyType?.GetItem();
            if (itemInfo == null)
                throw new MissingMemberException(proxyType?.Type.FullName, "Item");

            if (indexNode is IntegerIndexedNode intNode)
            {
                return new ItemTargetProxy<int>(collectionTarget, intNode.Value, itemInfo);
            }

            if (indexNode is StringIndexedNode stringNode)
            {
                return new ItemTargetProxy<string>(collectionTarget, stringNode.Value, itemInfo);
            }

            return null;
        }

        private static object GetCollectionTarget(IProxyType type, object target, string name)
        {
            IProxyPropertyInfo propertyInfo = type.GetProperty(name, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            if (propertyInfo != null)
                return propertyInfo.GetValue(target);

            IProxyFieldInfo fieldInfo = type.GetField(name, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            if (fieldInfo != null)
                return fieldInfo.GetValue(target);

            throw new MissingMemberException(type.Type.FullName, name);
        }
    }
}