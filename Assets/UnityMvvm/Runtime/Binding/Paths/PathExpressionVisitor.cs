using System;
using System.Reflection;
using System.Linq.Expressions;
using System.Collections.Generic;
using System.Linq;

namespace Fusion.Mvvm
{
    public class PathExpressionVisitor
    {
        private readonly List<Path> _paths = new List<Path>();

        public List<Path> Paths => _paths;

        public  Expression Visit(Expression expression)
        {
            if (expression == null)
                return null;

            if (expression is BinaryExpression bin)
                return VisitBinary(bin);

            if (expression is ConditionalExpression cond)
                return VisitConditional(cond);

            if (expression is ConstantExpression constant)
                return VisitConstant(constant);

            if (expression is LambdaExpression lambda)
                return VisitLambda(lambda);

            if (expression is ListInitExpression listInit)
                return VisitListInit(listInit);

            if (expression is MemberExpression member)
                return VisitMember(member);

            if (expression is MemberInitExpression memberInit)
                return VisitMemberInit(memberInit);

            if (expression is MethodCallExpression methodCall)
                return VisitMethodCall(methodCall);

            if (expression is NewExpression newExpr)
                return VisitNew(newExpr);

            if (expression is NewArrayExpression newArrayExpr)
                return VisitNewArray(newArrayExpr);

            if (expression is ParameterExpression param)
                return VisitParameter(param);

            if (expression is TypeBinaryExpression typeBinary)
                return VisitTypeBinary(typeBinary);

            if (expression is UnaryExpression unary)
                return VisitUnary(unary);

            if (expression is InvocationExpression invocation)
                return VisitInvocation(invocation);

            throw new NotSupportedException("Expressions of type " + expression.Type + " are not supported.");
        }

        private void Visit(IList<Expression> nodes)
        {
            if (nodes == null || nodes.Count <= 0)
                return;

            foreach (Expression expression in nodes)
                Visit(expression);
        }

        private  Expression VisitLambda(LambdaExpression node)
        {
            Visit(node.Parameters.Select(p => (Expression)p).ToList());
            return Visit(node.Body);
        }

        private  Expression VisitBinary(BinaryExpression node)
        {
            if (node.NodeType == ExpressionType.ArrayIndex)
            {
                Visit(ParseMemberPath(node, null, _paths));
            }
            else
            {
                List<Expression> list = new List<Expression>() { node.Left, node.Right, node.Conversion };
                Visit(list);
            }
            return null;
        }

        private  Expression VisitConditional(ConditionalExpression node)
        {
            List<Expression> list = new List<Expression>() { node.IfFalse, node.IfTrue, node.Test };
            Visit(list);
            return null;
        }

        private  Expression VisitConstant(ConstantExpression node)
        {
            return null;
        }

        private  void VisitElementInit(ElementInit init)
        {
            if (init == null)
                return;

            Visit(init.Arguments);
        }

        private  Expression VisitListInit(ListInitExpression node)
        {
            foreach (ElementInit init in node.Initializers)
            {
                VisitElementInit(init);
            }
            return Visit(node.NewExpression);
        }

        private  Expression VisitMember(MemberExpression node)
        {
            Visit(ParseMemberPath(node, null, _paths));
            return null;
        }

        private  Expression VisitInvocation(InvocationExpression node)
        {
            Visit(node.Arguments);
            return Visit(node.Expression);
        }

        private  Expression VisitMemberInit(MemberInitExpression expr)
        {
            return Visit(expr.NewExpression);
        }
        
        private  Expression VisitMethodCall(MethodCallExpression node)
        {
            Visit(ParseMemberPath(node, null, _paths));
            return null;
        }

        private  Expression VisitNew(NewExpression expr)
        {
            Visit(expr.Arguments);
            return null;
        }

        private  Expression VisitNewArray(NewArrayExpression node)
        {
            Visit(node.Expressions);
            return null;
        }

        private  Expression VisitParameter(ParameterExpression node)
        {
            return null;
        }

        private  Expression VisitTypeBinary(TypeBinaryExpression node)
        {
            return Visit(node.Expression);
        }

        private  Expression VisitUnary(UnaryExpression node)
        {
            return Visit(node.Operand);
        }

        private static Expression ConvertMemberAccessToConstant(Expression argument)
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

        private IList<Expression> ParseMemberPath(Expression expression, Path path, IList<Path> list)
        {
            if (expression.NodeType != ExpressionType.MemberAccess && expression.NodeType != ExpressionType.Call && expression.NodeType != ExpressionType.ArrayIndex)
                throw new Exception();

            List<Expression> result = new List<Expression>();

            Expression current = expression;
            while (current != null && (current is MemberExpression || current is MethodCallExpression || current is BinaryExpression || current is ParameterExpression || current is ConstantExpression))
            {
                if (current is MemberExpression me)
                {
                    if (path == null)
                    {
                        path = new Path();
                        list.Add(path);
                    }

                    var field = me.Member as FieldInfo;
                    if (field != null)
                    {
                        //static or instance
                        path.Prepend(new MemberNode(field));
                    }

                    var property = me.Member as PropertyInfo;
                    if (property != null)
                    {
                        //static or instance
                        path.Prepend(new MemberNode(property));
                    }

                    current = me.Expression;
                }
                else if (current is MethodCallExpression mc)
                {
                    if (mc.Method.Name.Equals("get_Item") && mc.Arguments.Count == 1)
                    {
                        if (path == null)
                        {
                            path = new Path();
                            list.Add(path);
                        }

                        var argument = mc.Arguments[0];
                        if (!(argument is ConstantExpression))
                            argument = ConvertMemberAccessToConstant(argument);

                        object value = (argument as ConstantExpression).Value;
                        if (value is string s)
                        {
                            path.PrependIndexed(s);
                        }
                        else if (value is Int32 i)
                        {
                            path.PrependIndexed(i);
                        }

                        current = mc.Object;
                    }
                    else
                    {
                        current = null;
                        result.AddRange(mc.Arguments);
                        result.Add(mc.Object);
                    }
                }
                else if (current is BinaryExpression binary)
                {
                    if (binary.NodeType == ExpressionType.ArrayIndex)
                    {
                        if (path == null)
                        {
                            path = new Path();
                            list.Add(path);
                        }

                        var left = binary.Left;
                        var right = binary.Right;
                        if (!(right is ConstantExpression))
                            right = ConvertMemberAccessToConstant(right);

                        object value = (right as ConstantExpression).Value;
                        if (value is string s)
                        {
                            path.PrependIndexed(s);
                        }
                        else if (value is Int32 i)
                        {
                            path.PrependIndexed(i);
                        }

                        current = left;
                    }
                    else
                    {
                        current = null;
                    }
                }
                else if (current is ParameterExpression)
                {
                    current = null;
                }
                else if (current is ConstantExpression)
                {
                    current = null;
                }
            }

            if (current != null)
                result.Add(current);

            return result;
        }
    }
}
