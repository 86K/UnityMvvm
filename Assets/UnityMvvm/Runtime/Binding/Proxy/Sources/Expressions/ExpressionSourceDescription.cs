using System;
using System.Reflection;
using System.Linq.Expressions;

namespace Fusion.Mvvm
{
    public class ExpressionSourceDescription : SourceDescription
    {
        private LambdaExpression _expression;

        private Type _returnType;

        public LambdaExpression Expression
        {
            get => _expression;
            set
            {
                _expression = value;

                Type[] types = _expression.GetType().GetGenericArguments();
                var delType = types[0];

                if (!typeof(Delegate).IsAssignableFrom(delType))
                    throw new NotSupportedException();

                MethodInfo info = delType.GetMethod("Invoke");

                if (info != null)
                {
                    _returnType = info.ReturnType;

                    ParameterInfo[] parameters = info.GetParameters();
                    IsStatic = parameters.Length <= 0;
                }
            }
        }

        public Type ReturnType => _returnType;

        public override string ToString()
        {
            return _expression == null ? "Expression:null" : "Expression:" + _expression;
        }
    }
}