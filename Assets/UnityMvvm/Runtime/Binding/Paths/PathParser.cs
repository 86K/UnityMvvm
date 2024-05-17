using System;
using System.Reflection;
using System.Linq.Expressions;

namespace Fusion.Mvvm
{
    public class PathParser : IPathParser
    {
        public Path Parse(string pathText)
        {
            return TextPathParser.Parse(pathText);
        }

        public Path Parse(LambdaExpression expression)
        {
            if (expression == null)
                throw new ArgumentNullException("expression");

            Path path = new Path();
            if (expression.Body is MemberExpression body)
            {
                Parse(body, path);
                return path;
            }

            if (expression.Body is MethodCallExpression method)
            {
                Parse(method, path);
                return path;
            }

            if (expression.Body is UnaryExpression unary && unary.NodeType == ExpressionType.Convert)
            {
                Parse(unary.Operand, path);
                return path;
            }

            if (expression.Body is BinaryExpression binary && binary.NodeType == ExpressionType.ArrayIndex)
            {
                Parse(binary, path);
                return path;
            }

            return path;
        }

        private void Parse(Expression expression, Path path)
        {
            if (expression == null || !(expression is MemberExpression || expression is MethodCallExpression || expression is BinaryExpression))
                return;

            if (expression is MemberExpression memberExpression)
            {
                var memberInfo = memberExpression.Member;
                if (memberInfo.IsStatic())
                {
                    path.Prepend(new MemberNode(memberInfo));
                    return;
                }

                path.Prepend(new MemberNode(memberInfo));
                if (memberExpression.Expression != null)
                    Parse(memberExpression.Expression, path);
                return;
            }

            if (expression is MethodCallExpression methodCallExpression)
            {
                if (methodCallExpression.Method.Name.Equals("get_Item") && methodCallExpression.Arguments.Count == 1)
                {
                    var argument = methodCallExpression.Arguments[0];
                    if (!(argument is ConstantExpression))
                        argument = ConvertMemberAccessToConstant(argument);

                    object value = (argument as ConstantExpression)?.Value;
                    if (value is string s)
                    {
                        path.PrependIndexed(s);
                    }
                    else if (value is Int32 i)
                    {
                        path.PrependIndexed(i);
                    }

                    if (methodCallExpression.Object != null)
                        Parse(methodCallExpression.Object, path);
                    return;
                }

                if (methodCallExpression.Method.Name.Equals("CreateDelegate"))
                {
                    var info = GetDelegateMethodInfo(methodCallExpression);
                    if (info == null)
                        throw new ArgumentException($"Invalid expression:{expression}");

                    if (info.IsStatic)
                    {
                        path.Prepend(new MemberNode(info));
                        return;
                    }

                    path.Prepend(new MemberNode(info));
                    Parse(methodCallExpression.Arguments[1], path);
                    return;
                }

                if (methodCallExpression.Method.ReturnType == typeof(void))
                {
                    var info = methodCallExpression.Method;
                    if (info.IsStatic)
                    {
                        path.Prepend(new MemberNode(info));
                        return;
                    }

                    path.Prepend(new MemberNode(info));
                    if (methodCallExpression.Object != null)
                        Parse(methodCallExpression.Object, path);
                    return;
                }

                throw new ArgumentException($"Invalid expression:{expression}");
            }

            if (expression is BinaryExpression binaryExpression)
            {
                if (binaryExpression.NodeType == ExpressionType.ArrayIndex)
                {
                    var left = binaryExpression.Left;
                    var right = binaryExpression.Right;
                    if (!(right is ConstantExpression))
                        right = ConvertMemberAccessToConstant(right);

                    object value = (right as ConstantExpression)?.Value;
                    if (value is string s)
                    {
                        path.PrependIndexed(s);
                    }
                    else if (value is int i)
                    {
                        path.PrependIndexed(i);
                    }

                    Parse(left, path);
                    return;
                }

                throw new ArgumentException($"Invalid expression:{expression}");
            }
        }

        public string ParseMemberName(LambdaExpression expression)
        {
            if (expression == null)
                throw new ArgumentNullException("expression");
            return ParseMemberName0(expression.Body);
        }

        private string ParseMemberName0(Expression expression)
        {
            if (expression == null || !(expression is MemberExpression || expression is MethodCallExpression || expression is UnaryExpression))
                return null;

            if (expression is MethodCallExpression methodCallExpression)
            {
                if (methodCallExpression.Method.Name.Equals("get_Item") && methodCallExpression.Arguments.Count == 1)
                {
                    string temp = null;
                    var argument = methodCallExpression.Arguments[0];
                    if (!(argument is ConstantExpression))
                        argument = ConvertMemberAccessToConstant(argument);

                    object value = (argument as ConstantExpression)?.Value;
                    if (value is string strIndex)
                    {
                        temp = $"[\"{strIndex}\"]";
                    }
                    else if (value is int intIndex)
                    {
                        temp = $"[{intIndex}]";
                    }

                    if (!(methodCallExpression.Object is MemberExpression memberExpression) || !(memberExpression.Expression is ParameterExpression))
                        return temp;

                    return ParseMemberName0(memberExpression) + temp;
                }

                return methodCallExpression.Method.Name;
            }

            if (expression is UnaryExpression unaryExpression && unaryExpression.NodeType == ExpressionType.Convert)
            {
                if (unaryExpression.Operand is MethodCallExpression methodCall && methodCall.Method.Name.Equals("CreateDelegate"))
                {
                    var info = GetDelegateMethodInfo(methodCall);
                    if (info != null)
                        return info.Name;
                }

                throw new ArgumentException($"Invalid expression:{expression}");
            }

            if (!(expression is MemberExpression body) || !(body.Expression is ParameterExpression))
                throw new ArgumentException($"Invalid expression:{expression}");

            return body.Member.Name;
        }
        
        MethodInfo GetDelegateMethodInfo(MethodCallExpression expression)
        {
            var target = expression.Object;
            var arguments = expression.Arguments;
            if (target == null)
            {
                foreach (var expr in arguments)
                {
                    if (!(expr is ConstantExpression constantExpression))
                        continue;

                    var value = constantExpression.Value;
                    if (value is MethodInfo info)
                        return info;
                }

                return null;
            }

            if (target is ConstantExpression constantExpression1)
            {
                var value = constantExpression1.Value;
                if (value is MethodInfo info)
                    return info;
            }

            return null;
        }
        
        static Expression ConvertMemberAccessToConstant(Expression argument)
        {
            if (argument is ConstantExpression)
                return argument;

            var boxed = Expression.Convert(argument, typeof(object));
#if UNITY_IOS || ENABLE_IL2CPP
            var fun = (Func<object[], object>)Expression.Lambda<Func<object>>(boxed).DynamicCompile();
            var constant = fun(new object[] { });
#else
            var fun = Expression.Lambda<Func<object>>(boxed).Compile();
            var constant = fun();
#endif

            return Expression.Constant(constant);
        }
    }
}