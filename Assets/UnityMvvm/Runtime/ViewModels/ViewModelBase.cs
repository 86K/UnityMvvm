

using System;
using System.Linq.Expressions;
using System.ComponentModel;
using System.Collections.Generic;
using UnityEngine;

namespace Fusion.Mvvm
{
    public abstract class ViewModelBase : ObservableObject, IViewModel
    {
        private IMessenger messenger;

        public ViewModelBase() : this(null)
        {
        }

        public ViewModelBase(IMessenger messenger)
        {
            this.messenger = messenger;
        }

        public virtual IMessenger Messenger
        {
            get => messenger;
            set => messenger = value;
        }

        protected void Broadcast<T>(T oldValue, T newValue, string propertyName)
        {
            try
            {
                var messenger = Messenger;
                if (messenger != null)
                    messenger.Publish(new PropertyChangedMessage<T>(this, oldValue, newValue, propertyName));
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Set property '{propertyName}', broadcast messages failure.Exception:{e}");
            }
        }

        /// <summary>
        /// Set the specified propertyName, field, newValue and broadcast.
        /// </summary>
        /// <param name="field">Field.</param>
        /// <param name="newValue">New value.</param>
        /// <param name="propertyExpression">Expression of property name.</param>
        /// <param name="broadcast">If set to <c>true</c> broadcast.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        protected bool Set<T>(ref T field, T newValue, Expression<Func<T>> propertyExpression, bool broadcast)
        {
            if (EqualityComparer<T>.Default.Equals(field, newValue))
                return false;

            var oldValue = field;
            field = newValue;
            var propertyName = ParserPropertyName(propertyExpression);
            RaisePropertyChanged(propertyName);

            if (broadcast)
                Broadcast(oldValue, newValue, propertyName);
            return true;
        }

        /// <summary>
        ///  Set the specified propertyName, field, newValue and broadcast.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="field"></param>
        /// <param name="newValue"></param>
        /// <param name="propertyName"></param>
        /// <param name="broadcast"></param>
        /// <returns></returns>
        protected bool Set<T>(ref T field, T newValue, string propertyName, bool broadcast)
        {
            if (EqualityComparer<T>.Default.Equals(field, newValue))
                return false;

            var oldValue = field;
            field = newValue;
            RaisePropertyChanged(propertyName);

            if (broadcast)
                Broadcast(oldValue, newValue, propertyName);
            return true;
        }

        /// <summary>
        ///  Set the specified propertyName, field, newValue and broadcast.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="field"></param>
        /// <param name="newValue"></param>
        /// <param name="eventArgs"></param>
        /// <param name="broadcast"></param>
        /// <returns></returns>
        protected bool Set<T>(ref T field, T newValue, PropertyChangedEventArgs eventArgs, bool broadcast)
        {
            if (EqualityComparer<T>.Default.Equals(field, newValue))
                return false;

            var oldValue = field;
            field = newValue;
            RaisePropertyChanged(eventArgs);

            if (broadcast)
                Broadcast(oldValue, newValue, eventArgs.PropertyName);
            return true;
        }

        #region IDisposable Support
        ~ViewModelBase()
        {
            Dispose(false);
        }

        protected virtual void Dispose(bool disposing)
        {
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}

