using System;
using System.Linq.Expressions;

namespace Fusion.Mvvm
{
    public class BindingBuilderBase : IBindingBuilder
    {
        private bool _isBuilded;
        private readonly object _target;
        private readonly IBindingContext _bindingContext;
        protected readonly TargetDescription _targetDescription;

        private IPathParser _pathParser;
        private IConverterRegistry _converterRegistry;

        protected IPathParser PathParser => _pathParser ??= Context.GetGlobalContext().GetService<IPathParser>();

        private IConverterRegistry ConverterRegistry => _converterRegistry ??= Context.GetGlobalContext().GetService<IConverterRegistry>();
        
        protected BindingBuilderBase(IBindingContext context, object target)
        {
            _bindingContext = context ?? throw new ArgumentNullException("context");
            _target = target ?? throw new ArgumentNullException("target", "Failed to create data binding, the bound UI control cannot be null.");

            _targetDescription = new TargetDescription
            {
                Mode = BindingMode.Default
            };
        }
        
        public void TwoWay()
        {
            SetMode(BindingMode.TwoWay);
        }

        public void OneWay()
        {
            SetMode(BindingMode.OneWay);
        }

        public void OneWayToSource()
        {
            SetMode(BindingMode.OneWayToSource);
        }

        public void OneTime()
        {
            SetMode(BindingMode.OneTime);
        }

        void SetMode(BindingMode mode)
        {
            _targetDescription.Mode = mode;
        }

        protected void SetMemberPath(Path path)
        {
            if (_targetDescription.Source != null)
                throw new Exception("You cannot set the source path of a Fluent binding more than once");

            if (path == null)
                throw new ArgumentException("the path is null.");

            // NOTE：这个条件有必要吗？
            if (path.IsStatic)
                throw new ArgumentException("Need a non-static path in here.");

            _targetDescription.Source = new ObjectSourceDescription()
            {
                Path = path
            };
        }

        protected void SetExpression<T, TResult>(Expression<Func<T, TResult>> expression)
        {
            if (_targetDescription.Source != null)
                throw new Exception("You cannot set the source path of a Fluent binding more than once");

            _targetDescription.Source = new ExpressionSourceDescription()
            {
                Expression = expression
            };
        }

        protected void SetCommandParameter<T>(T parameter)
        {
            _targetDescription.CommandParameter = parameter;
            _targetDescription.Converter = new ParameterWrapConverter<T>(new ConstantCommandParameter<T>(parameter));
        }

        protected void SetCommandParameter<TParam>(Func<TParam> parameter)
        {
            _targetDescription.CommandParameter = parameter;
            _targetDescription.Converter = new ParameterWrapConverter<TParam>(new ExpressionCommandParameter<TParam>(parameter));
        }
        
        protected IConverter ConverterByName(string name)
        {
            return ConverterRegistry.Find(name);
        }

        private void CheckBindingDescription()
        {
            if (string.IsNullOrEmpty(_targetDescription.TargetName))
                throw new Exception("TargetName is null!");

            if (_targetDescription.Source == null)
                throw new Exception("Source description is null!");
        }

        public void Build()
        {
            try
            {
                if (_isBuilded)
                    return;

                CheckBindingDescription();
                _bindingContext.Add(_target, _targetDescription);
                _isBuilded = true;
            }
            catch (Exception e)
            {
                throw new Exception($"An exception occurred while building the data binding for {_targetDescription}.\n Exception:\n{e}");
            }
        }
    }
}
