using System;
using System.Linq.Expressions;
using System.Reflection;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Collections.Generic;

namespace Fusion.Mvvm
{
    [Serializable]
    public abstract class ObservableObject : INotifyPropertyChanged
    {
        private static readonly Dictionary<string, PropertyChangedEventArgs> _PropertyChangedEventArgs = new Dictionary<string, PropertyChangedEventArgs>();

        private static PropertyChangedEventArgs GetPropertyChangedEventArgs(string propertyName)
        {
            if (propertyName == null)
                return new PropertyChangedEventArgs(null);

            if (_PropertyChangedEventArgs.TryGetValue(propertyName, out var eventArgs))
                return eventArgs;

            eventArgs = new PropertyChangedEventArgs(propertyName);
            _PropertyChangedEventArgs[propertyName] = eventArgs;
            return eventArgs;
        }

        private readonly object _lock = new object();
        private PropertyChangedEventHandler _propertyChanged;

        public event PropertyChangedEventHandler PropertyChanged
        {
            add
            {
                lock (_lock)
                {
                    _propertyChanged += value;
                }
            }
            remove
            {
                lock (_lock)
                {
                    _propertyChanged -= value;
                }
            }
        }
        
        /// <summary>
        /// Raises the PropertyChanging event.
        /// </summary>
        /// <param name="propertyName">Property name.</param>
        protected virtual void RaisePropertyChanged(string propertyName = null)
        {
            RaisePropertyChanged(GetPropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Raises the PropertyChanging event.
        /// </summary>
        /// <param name="eventArgs">Property changed event.</param>
        protected virtual void RaisePropertyChanged(PropertyChangedEventArgs eventArgs)
        {
            try
            {
                if (_propertyChanged != null)
                    _propertyChanged(this, eventArgs);
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogWarning($"Set property '{eventArgs.PropertyName}', raise PropertyChanged failure.Exception:{e}");
            }
        }

        /// <summary>
        /// Raises the PropertyChanging event.
        /// </summary>
        /// <param name="eventArgs"></param>
        protected virtual void RaisePropertyChanged(params PropertyChangedEventArgs[] eventArgs)
        {
            foreach (var args in eventArgs)
            {
                try
                {
                    if (_propertyChanged != null)
                        _propertyChanged(this, args);
                }
                catch (Exception e)
                {
                    UnityEngine.Debug.LogWarning($"Set property '{args.PropertyName}', raise PropertyChanged failure.Exception:{e}");
                }
            }
        }

        protected virtual string ParserPropertyName(LambdaExpression propertyExpression)
        {
            if (propertyExpression == null)
                throw new ArgumentNullException("propertyExpression");

            if (!(propertyExpression.Body is MemberExpression body))
                throw new ArgumentException("Invalid argument", "propertyExpression");

            var property = body.Member as PropertyInfo;
            if (property == null)
                throw new ArgumentException("Argument is not a property", "propertyExpression");

            return property.Name;
        }
        
        /// <summary>
        /// Set the specified propertyExpression, field and newValue.
        /// </summary>
        /// <param name="propertyExpression">Property expression.</param>
        /// <param name="field">Field.</param>
        /// <param name="newValue">New value.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        protected bool Set<T>(ref T field, T newValue, Expression<Func<T>> propertyExpression)
        {
            if (EqualityComparer<T>.Default.Equals(field, newValue))
                return false;

            field = newValue;
            var propertyName = ParserPropertyName(propertyExpression);
            RaisePropertyChanged(propertyName);
            return true;
        }

        /// <summary>
        ///  Set the specified propertyName, field, newValue.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="field"></param>
        /// <param name="newValue"></param>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        protected bool Set<T>(ref T field, T newValue, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, newValue))
                return false;

            field = newValue;
            RaisePropertyChanged(propertyName);
            return true;
        }

        /// <summary>
        ///  Set the specified propertyName, field, newValue.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="field"></param>
        /// <param name="newValue"></param>
        /// <param name="eventArgs"></param>
        /// <returns></returns>
        protected bool Set<T>(ref T field, T newValue, PropertyChangedEventArgs eventArgs)
        {
            if (EqualityComparer<T>.Default.Equals(field, newValue))
                return false;

            field = newValue;
            RaisePropertyChanged(eventArgs);
            return true;
        }
    }
}