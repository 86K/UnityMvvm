using System;
using System.Collections.Generic;

namespace Fusion.Mvvm
{
    public class ExpressionSourceProxyFactory : TypedSourceProxyFactory<ExpressionSourceDescription>
    {
        private readonly ISourceProxyFactory _factory;
        private readonly IExpressionPathFinder _pathFinder;

        public ExpressionSourceProxyFactory(ISourceProxyFactory factory, IExpressionPathFinder pathFinder)
        {
            _factory = factory;
            _pathFinder = pathFinder;
        }

        protected override bool TryCreateProxy(object source, ExpressionSourceDescription description, out ISourceProxy proxy)
        {
            proxy = null;
            var expression = description.Expression;
            List<ISourceProxy> list = new List<ISourceProxy>();
            List<Path> paths = _pathFinder.FindPaths(expression);
            foreach (Path path in paths)
            {
                if (!path.IsStatic)
                {
                    if (source == null)
                        continue; //ignore the path

                    if (path[0] is MemberNode memberNode)
                    {
                        if(memberNode.MemberInfo != null && memberNode.MemberInfo.DeclaringType != null &&
                            !memberNode.MemberInfo.DeclaringType.IsInstanceOfType(source))
                        {
                            continue; //ignore the path
                        }
                    }
                }

                ISourceProxy innerProxy = _factory.CreateProxy(source, new ObjectSourceDescription() { Path = path });
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
                    proxy = (ISourceProxy)Activator.CreateInstance(typeof(ExpressionSourceProxy<,>).MakeGenericType(parameterType, returnType),
                        source, del, list);
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