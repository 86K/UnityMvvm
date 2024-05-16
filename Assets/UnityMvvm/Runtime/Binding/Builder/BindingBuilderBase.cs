/*
 * MIT License
 *
 * Copyright (c) 2018 Clark Yang
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy of 
 * this software and associated documentation files (the "Software"), to deal in 
 * the Software without restriction, including without limitation the rights to 
 * use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies 
 * of the Software, and to permit persons to whom the Software is furnished to do so, 
 * subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in all 
 * copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE 
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, 
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE 
 * SOFTWARE.
 */

using System;
using System.Linq.Expressions;
using Loxodon.Framework.Binding.Paths;
using Loxodon.Framework.Binding.Proxy.Sources;
using Loxodon.Framework.Binding.Proxy.Sources.Text;
using Loxodon.Framework.Binding.Proxy.Sources.Object;
using Loxodon.Framework.Binding.Proxy.Sources.Expressions;
using Loxodon.Framework.Binding.Converters;
using Loxodon.Framework.Binding.Contexts;
using Loxodon.Framework.Contexts;
using Loxodon.Framework.Binding.Parameters;

namespace Loxodon.Framework.Binding.Builder
{
    public class BindingBuilderBase : IBindingBuilder
    {
        //private static readonly ILog log = LogManager.GetLogger(typeof(BindingBuilderBase));

        private bool builded = false;
        private object scopeKey;
        private readonly object target;
        private readonly IBindingContext context;
        protected BindingDescription description;

        private IPathParser pathParser;
        private IConverterRegistry converterRegistry;

        protected IPathParser PathParser => pathParser ?? (pathParser = Context.GetApplicationContext().GetService<IPathParser>());

        protected IConverterRegistry ConverterRegistry => converterRegistry ?? (converterRegistry = Context.GetApplicationContext().GetService<IConverterRegistry>());


        public BindingBuilderBase(IBindingContext context, object target)
        {
            this.context = context ?? throw new ArgumentNullException("context");
            this.target = target ?? throw new ArgumentNullException("target", "Failed to create data binding, the bound UI control cannot be null.");

            description = new BindingDescription();
            description.Mode = BindingMode.Default;
        }

        protected void SetLiteral(object value)
        {
            if (description.Source != null)
                throw new BindingException("You cannot set the source path of a Fluent binding more than once");

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
                throw new BindingException("You cannot set the source path of a Fluent binding more than once");

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
                throw new BindingException("You cannot set the source path of a Fluent binding more than once");

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
                throw new BindingException("You cannot set the source path of a Fluent binding more than once");

            description.Source = new ExpressionSourceDescription()
            {
                Expression = expression
            };
        }

        protected void SetExpression<T, TResult>(Expression<Func<T, TResult>> expression)
        {
            if (description.Source != null)
                throw new BindingException("You cannot set the source path of a Fluent binding more than once");

            description.Source = new ExpressionSourceDescription()
            {
                Expression = expression
            };
        }

        protected void SetExpression(LambdaExpression expression)
        {
            if (description.Source != null)
                throw new BindingException("You cannot set the source path of a Fluent binding more than once");

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
                throw new BindingException("You cannot set the source path of a Fluent binding more than once");
            description.Source = source;
        }

        public void SetDescription(BindingDescription bindingDescription)
        {
            description.Mode = bindingDescription.Mode;
            description.TargetName = bindingDescription.TargetName;
            description.TargetType = bindingDescription.TargetType;
            description.UpdateTrigger = bindingDescription.UpdateTrigger;
            description.Converter = bindingDescription.Converter;
            description.Source = bindingDescription.Source;
        }

        protected IConverter ConverterByName(string name)
        {
            return ConverterRegistry.Find(name);
        }

        protected void CheckBindingDescription()
        {
            if (string.IsNullOrEmpty(description.TargetName))
                throw new BindingException("TargetName is null!");

            if (description.Source == null)
                throw new BindingException("Source description is null!");
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
            catch (BindingException e)
            {
                throw e;
            }
            catch (Exception e)
            {
                throw new BindingException(e, "An exception occurred while building the data binding for {0}.", description.ToString());
            }
        }
    }
}
