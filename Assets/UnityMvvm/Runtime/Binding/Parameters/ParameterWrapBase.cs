using System;

namespace Fusion.Mvvm
{
    public class ParameterWrapBase
    {
        private readonly ICommandParameter _commandParameter;

        protected ParameterWrapBase(ICommandParameter commandParameter)
        {
            _commandParameter = commandParameter ?? throw new ArgumentNullException("commandParameter");
        }

        protected object GetParameterValue()
        {
            return _commandParameter.GetValue();
        }

        protected Type GetParameterValueType()
        {
            return _commandParameter.GetValueType();
        }
    }
}