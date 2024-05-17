using System;
using System.Linq.Expressions;

namespace Fusion.Mvvm
{
    public class BindingBuilder<TTarget, TSource> : BindingBuilderBase where TTarget : class
    {
        public BindingBuilder(IBindingContext context, TTarget target) : base(context, target)
        {
            description.TargetType = typeof(TTarget);
        }

        BindingBuilder<TTarget, TSource> ForInternal(string targetName, string updateTrigger = null)
        {
            description.TargetName = targetName;
            description.UpdateTrigger = updateTrigger;
            return this;
        }

        public BindingBuilder<TTarget, TSource> For<TResult>(Expression<Func<TTarget, TResult>> memberExpression)
        {
            string targetName = PathParser.ParseMemberName(memberExpression);

            return ForInternal(targetName);
        }

        public BindingBuilder<TTarget, TSource> For<TResult>(Expression<Func<TTarget, TResult>> memberExpression, string updateTrigger)
        {
            string targetName = PathParser.ParseMemberName(memberExpression);
            return ForInternal(targetName, updateTrigger);
        }

        public BindingBuilder<TTarget, TSource> For<TResult, TEvent>(Expression<Func<TTarget, TResult>> memberExpression, Expression<Func<TTarget, TEvent>> updateTriggerExpression)
        {
            string targetName = PathParser.ParseMemberName(memberExpression);
            string updateTrigger = PathParser.ParseMemberName(updateTriggerExpression);
            return ForInternal(targetName, updateTrigger);
        }

        public BindingBuilder<TTarget, TSource> For(Expression<Func<TTarget, EventHandler<InteractionEventArgs>>> memberExpression)
        {
            string targetName = PathParser.ParseMemberName(memberExpression);
            OneWayToSource();
            
            return ForInternal(targetName);
        }

        public BindingBuilder<TTarget, TSource> To(string path)
        {
            SetMemberPath(path);
            return this;
        }

        public BindingBuilder<TTarget, TSource> To<TResult>(Expression<Func<TSource, TResult>> path)
        {
            SetMemberPath(PathParser.Parse(path));
            return this;
        }

        public BindingBuilder<TTarget, TSource> To<TParameter>(Expression<Func<TSource, Action<TParameter>>> path)
        {
            SetMemberPath(PathParser.Parse(path));
            return this;
        }

        public BindingBuilder<TTarget, TSource> To(Expression<Func<TSource, Action>> path)
        {
            SetMemberPath(PathParser.Parse(path));
            return this;
        }

        public BindingBuilder<TTarget, TSource> ToExpression<TResult>(Expression<Func<TSource, TResult>> expression)
        {
            SetExpression(expression);
            OneWay();
            return this;
        }
        
        public BindingBuilder<TTarget, TSource> CommandParameter<T>(T parameter)
        {
            SetCommandParameter(parameter);
            return this;
        }

        public BindingBuilder<TTarget, TSource> CommandParameter<TParam>(Func<TParam> parameter)
        {
            SetCommandParameter(parameter);
            return this;
        }

        public BindingBuilder<TTarget, TSource> WithConversion(string converterName)
        {
            var converter = ConverterByName(converterName);
            return WithConversion(converter);
        }

        public BindingBuilder<TTarget, TSource> WithConversion(IConverter converter)
        {
            description.Converter = converter;
            return this;
        }

        public BindingBuilder<TTarget, TSource> WithScopeKey(object scopeKey)
        {
            SetScopeKey(scopeKey);
            return this;
        }
    }

    public class BindingBuilder<TTarget> : BindingBuilderBase where TTarget : class
    {
        public BindingBuilder(IBindingContext context, TTarget target) : base(context, target)
        {
            description.TargetType = typeof(TTarget);
        }

        BindingBuilder<TTarget> ForInternal(string targetPropertyName, string updateTrigger = null)
        {
            description.TargetName = targetPropertyName;
            description.UpdateTrigger = updateTrigger;
            return this;
        }

        public BindingBuilder<TTarget> For<TResult>(Expression<Func<TTarget, TResult>> memberExpression)
        {
            string targetName = PathParser.ParseMemberName(memberExpression);
            return ForInternal(targetName);
        }

        public BindingBuilder<TTarget> For<TResult>(Expression<Func<TTarget, TResult>> memberExpression, string updateTrigger)
        {
            string targetName = PathParser.ParseMemberName(memberExpression);
            return ForInternal(targetName, updateTrigger);
        }

        public BindingBuilder<TTarget> For<TResult, TEvent>(Expression<Func<TTarget, TResult>> memberExpression, Expression<Func<TTarget, TEvent>> updateTriggerExpression)
        {
            string targetName = PathParser.ParseMemberName(memberExpression);
            string updateTrigger = PathParser.ParseMemberName(updateTriggerExpression);
            return ForInternal(targetName, updateTrigger);
        }

        public BindingBuilder<TTarget> For(Expression<Func<TTarget, EventHandler<InteractionEventArgs>>> memberExpression)
        {
            string targetName = PathParser.ParseMemberName(memberExpression);
            OneWayToSource();
            return ForInternal(targetName);
        }

        public BindingBuilder<TTarget> To(string path)
        {
            SetStaticMemberPath(path);
            OneWay();
            return this;
        }

        public BindingBuilder<TTarget> To<TResult>(Expression<Func<TResult>> path)
        {
            SetStaticMemberPath(PathParser.ParseStaticPath(path));
            OneWay();
            return this;
        }

        public BindingBuilder<TTarget> To<TParameter>(Expression<Func<Action<TParameter>>> path)
        {
            SetStaticMemberPath(PathParser.ParseStaticPath(path));
            return this;
        }

        public BindingBuilder<TTarget> To(Expression<Func<Action>> path)
        {
            SetStaticMemberPath(PathParser.ParseStaticPath(path));
            return this;
        }

        public BindingBuilder<TTarget> ToValue(object value)
        {
            SetLiteral(value);
            return this;
        }

        public BindingBuilder<TTarget> ToExpression<TResult>(Expression<Func<TResult>> expression)
        {
            SetExpression(expression);
            OneWay();
            return this;
        }

        public BindingBuilder<TTarget> CommandParameter<T>(T parameter)
        {
            SetCommandParameter(parameter);
            return this;
        }

        public BindingBuilder<TTarget> CommandParameter<TParam>(Func<TParam> parameter)
        {
            SetCommandParameter(parameter);
            return this;
        }

        public BindingBuilder<TTarget> WithConversion(string converterName)
        {
            var converter = ConverterByName(converterName);
            return WithConversion(converter);
        }

        public BindingBuilder<TTarget> WithConversion(IConverter converter)
        {
            description.Converter = converter;
            return this;
        }

        public BindingBuilder<TTarget> WithScopeKey(object scopeKey)
        {
            SetScopeKey(scopeKey);
            return this;
        }
    }
}
