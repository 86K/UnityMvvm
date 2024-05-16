

using System;
using System.Collections.Generic;

namespace Fusion.Mvvm
{
    public class ExpressionSourceProxyFactory : TypedSourceProxyFactory<ExpressionSourceDescription>
    {
        private readonly ISourceProxyFactory factory;
        private readonly IExpressionPathFinder pathFinder;
        public ExpressionSourceProxyFactory(ISourceProxyFactory factory, IExpressionPathFinder pathFinder)
        {
            this.factory = factory;
            this.pathFinder = pathFinder;
        }

        protected override bool TryCreateProxy(object source, ExpressionSourceDescription description, out ISourceProxy proxy)
        {
            proxy = null;
            var expression = description.Expression;
            List<ISourceProxy> list = new List<ISourceProxy>();
            List<Path> paths = pathFinder.FindPaths(expression);
            foreach (Path path in paths)
            {
                if (!path.IsStatic)
                {
                    if (source == null)
                        continue;//ignore the path

                    MemberNode memberNode = path[0] as MemberNode;
                    if (memberNode != null && memberNode.MemberInfo != null && !memberNode.MemberInfo.DeclaringType.IsAssignableFrom(source.GetType()))
                        continue;//ignore the path
                }

                ISourceProxy innerProxy = factory.CreateProxy(source, new ObjectSourceDescription() { Path = path });
                if (innerProxy != null)
                    list.Add(innerProxy);
            }

#if UNITY_IOS || ENABLE_IL2CPP
            Func<object[], object> del = expression.DynamicCompile();
            proxy = new ExpressionSourceProxy(description.IsStatic ? null : source, del, description.ReturnType, list);
#else
            try
            {
                var del = expression.Compile();
                Type returnType = del.ReturnType();
                Type parameterType = del.ParameterType();
                if (parameterType != null)
                {
                    proxy = (ISourceProxy)Activator.CreateInstance(typeof(ExpressionSourceProxy<,>).MakeGenericType(parameterType, returnType), source, del, list);
                }
                else
                {
                    proxy = (ISourceProxy)Activator.CreateInstance(typeof(ExpressionSourceProxy<>).MakeGenericType(returnType), del, list);
                }
            }
            catch (Exception)
            {
                //JIT Exception
                Func<object[], object> del = expression.DynamicCompile();
                proxy = new ExpressionSourceProxy(description.IsStatic ? null : source, del, description.ReturnType, list);
            }
#endif
            if (proxy != null)
                return true;

            return false;
        }
    }
}
