using System;

namespace Fusion.Mvvm
{
    public class ExpressionCommandParameter<TParam> : ICommandParameter<TParam>
    {
        private readonly Func<TParam> _expression;

        public ExpressionCommandParameter(Func<TParam> expression)
        {
            _expression = expression;
        }

        object ICommandParameter.GetValue()
        {
            return GetValue();
        }

        public Type GetValueType()
        {
            return typeof(TParam);
        }

        public TParam GetValue()
        {
            return _expression();
        }
    }
}