

using System;

namespace Fusion.Mvvm
{
    public class ParameterWrapBase
    {
        protected readonly ICommandParameter commandParameter;
        public ParameterWrapBase(ICommandParameter commandParameter)
        {
            this.commandParameter = commandParameter ?? throw new ArgumentNullException("commandParameter");
        }

        protected virtual object GetParameterValue()
        {
            return commandParameter.GetValue();
        }

        protected virtual Type GetParameterValueType()
        {
            return commandParameter.GetValueType();
        }
    }
}
