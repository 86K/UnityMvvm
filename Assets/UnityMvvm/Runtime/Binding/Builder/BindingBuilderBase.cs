using System;
using System.Linq.Expressions;

namespace Fusion.Mvvm
{
    public class BindingBuilderBase : IBindingBuilder
    {
        private bool builded;
        private object scopeKey;
        private readonly object target;
        private readonly IBindingContext context;
        protected readonly TargetDescription description;

        private IPathParser pathParser;
        private IConverterRegistry converterRegistry;

        protected IPathParser PathParser => pathParser ??= Context.GetApplicationContext().GetService<IPathParser>();

        private IConverterRegistry ConverterRegistry => converterRegistry ??= Context.GetApplicationContext().GetService<IConverterRegistry>();


        protected BindingBuilderBase(IBindingContext context, object target)
        {
            this.context = context ?? throw new ArgumentNullException("context");
            this.target = target ?? throw new ArgumentNullException("target", "Failed to create data binding, the bound UI control cannot be null.");

            description = new TargetDescription
            {
                Mode = BindingMode.Default
            };
        }

        protected void SetLiteral(object value)
        {
            if (description.Source != null)
                throw new Exception("You cannot set the source path of a Fluent binding more than once");

            description.Source = new LiteralSourceDescription()
            {
                Literal = value
            };
        }

        protected void SetMode(BindingMode mode)
        {
            description.Mode = mode;
        }

        protected void SetScopeKey(object scopeKey)
        {
            this.scopeKey = scopeKey;
        }

        protected void SetMemberPath(string pathText)
        {
            Path path = PathParser.Parse(pathText);
            SetMemberPath(path);
        }

        protected void SetMemberPath(Path path)
        {
            if (description.Source != null)
                throw new Exception("You cannot set the source path of a Fluent binding more than once");

            if (path == null)
                throw new ArgumentException("the path is null.");

            if (path.IsStatic)
                throw new ArgumentException("Need a non-static path in here.");

            description.Source = new ObjectSourceDescription()
            {
                Path = path
            };
        }

        protected void SetStaticMemberPath(string pathText)
        {
            Path path = PathParser.ParseStaticPath(pathText);
            SetStaticMemberPath(path);
        }

        protected void SetStaticMemberPath(Path path)
        {
            if (description.Source != null)
                throw new Exception("You cannot set the source path of a Fluent binding more than once");

            if (path == null)
                throw new ArgumentException("the path is null.");

            if (!path.IsStatic)
                throw new ArgumentException("Need a static path in here.");

            description.Source = new ObjectSourceDescription()
            {
                Path = path
            };
        }

        protected void SetExpression<TResult>(Expression<Func<TResult>> expression)
        {
            if (description.Source != null)
                throw new Exception("You cannot set the source path of a Fluent binding more than once");

            description.Source = new ExpressionSourceDescription()
            {
                Expression = expression
            };
        }

        protected void SetExpression<T, TResult>(Expression<Func<T, TResult>> expression)
        {
            if (description.Source != null)
                throw new Exception("You cannot set the source path of a Fluent binding more than once");

            description.Source = new ExpressionSourceDescription()
            {
                Expression = expression
            };
        }

        [Obsolete("Never used..")]
        protected void SetExpression(LambdaExpression expression)
        {
            if (description.Source != null)
                throw new Exception("You cannot set the source path of a Fluent binding more than once");

            description.Source = new ExpressionSourceDescription()
            {
                Expression = expression
            };
        }

        protected void SetCommandParameter(object parameter)
        {
            description.CommandParameter = parameter;
            description.Converter = new ParameterWrapConverter(new ConstantCommandParameter(parameter));
        }

        protected void SetCommandParameter<T>(T parameter)
        {
            description.CommandParameter = parameter;
            description.Converter = new ParameterWrapConverter<T>(new ConstantCommandParameter<T>(parameter));
        }

        protected void SetCommandParameter<TParam>(Func<TParam> parameter)
        {
            description.CommandParameter = parameter;
            description.Converter = new ParameterWrapConverter<TParam>(new ExpressionCommandParameter<TParam>(parameter));
        }

        protected void SetSourceDescription(SourceDescription source)
        {
            if (description.Source != null)
                throw new Exception("You cannot set the source path of a Fluent binding more than once");
            description.Source = source;
        }

        public void SetDescription(TargetDescription targetDescription)
        {
            description.Mode = targetDescription.Mode;
            description.TargetName = targetDescription.TargetName;
            description.TargetType = targetDescription.TargetType;
            description.UpdateTrigger = targetDescription.UpdateTrigger;
            description.Converter = targetDescription.Converter;
            description.Source = targetDescription.Source;
        }

        protected IConverter ConverterByName(string name)
        {
            return ConverterRegistry.Find(name);
        }

        private void CheckBindingDescription()
        {
            if (string.IsNullOrEmpty(description.TargetName))
                throw new Exception("TargetName is null!");

            if (description.Source == null)
                throw new Exception("Source description is null!");
        }

        public void Build()
        {
            try
            {
                if (builded)
                    return;

                CheckBindingDescription();
                context.Add(target, description, scopeKey);
                builded = true;
            }
            catch (Exception e)
            {
                throw new Exception($"An exception occurred while building the data binding for {description}.\n Exception:\n{e}");
            }
        }
    }
}
