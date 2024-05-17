using System;
using System.Linq.Expressions;

namespace Fusion.Mvvm
{
    /// <summary>
    /// For和To必须成对一起使用!!!
    /// </summary>
    /// <typeparam name="TTarget">一般就是UIView的继承类。</typeparam>
    /// <typeparam name="TSource">一般就是ViewModel的继承类。</typeparam>
    public class BindingBuilder<TTarget, TSource> : BindingBuilderBase where TTarget : class
    {
        public BindingBuilder(IBindingContext context, TTarget target) : base(context, target)
        {
            _targetDescription.TargetType = typeof(TTarget);
        }

        /// <summary>
        /// 绑定目标描述的targetName和updateTrigger。目标是View。
        /// </summary>
        /// <param name="targetName"></param>
        /// <param name="updateTrigger"></param>
        /// <returns></returns>
        BindingBuilder<TTarget, TSource> ForInternal(string targetName, string updateTrigger = null)
        {
            _targetDescription.TargetName = targetName;
            _targetDescription.UpdateTrigger = updateTrigger;
            return this;
        }
        
        public BindingBuilder<TTarget, TSource> For<TResult>(Expression<Func<TTarget, TResult>> memberExpression, string updateTrigger = null)
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
        
        [Obsolete("Never used.")]
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
            _targetDescription.Converter = converter;
            return this;
        }
    }
}
