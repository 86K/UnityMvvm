

using System;
using System.Reflection;
using System.Linq.Expressions;

namespace Fusion.Mvvm
{
    public class ExpressionSourceDescription : SourceDescription
    {
        private LambdaExpression expression;

        private Type returnType;

        public ExpressionSourceDescription()
        {
        }

        public LambdaExpression Expression
        {
            get => expression;
            set
            {
                expression = value;

                Type[] types = expression.GetType().GetGenericArguments();
                var delType = types[0];

                if (!typeof(Delegate).IsAssignableFrom(delType))
                    throw new NotSupportedException();

                MethodInfo info = delType.GetMethod("Invoke");

                returnType = info.ReturnType;

                ParameterInfo[] parameters = info.GetParameters();
                IsStatic = (parameters == null || parameters.Length <= 0) ? true : false;
            }
        }

        public Type ReturnType => returnType;

        public override string ToString()
        {
            return expression == null ? "Expression:null" : "Expression:" + expression.ToString();
        }
    }
}
