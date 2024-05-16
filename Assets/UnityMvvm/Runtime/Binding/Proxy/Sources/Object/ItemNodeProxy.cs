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

using Loxodon.Framework.Binding.Reflection;
using System;
using System.Collections;
using System.Text.RegularExpressions;

using INotifyCollectionChanged = System.Collections.Specialized.INotifyCollectionChanged;
using NotifyCollectionChangedAction = System.Collections.Specialized.NotifyCollectionChangedAction;
using NotifyCollectionChangedEventArgs = System.Collections.Specialized.NotifyCollectionChangedEventArgs;

namespace Loxodon.Framework.Binding.Proxy.Sources.Object
{
    public abstract class ItemNodeProxy<TKey> : NotifiableSourceProxyBase, IObtainable, IModifiable, INotifiable
    {
        protected TKey key;
        protected IProxyItemInfo itemInfo;
        protected bool isList;
        protected Regex regex;

        public ItemNodeProxy(ICollection source, TKey key, IProxyItemInfo itemInfo) : base(source)
        {
            this.key = key;
            isList = (source is IList);

            this.itemInfo = itemInfo;

            if (this.source != null && this.source is INotifyCollectionChanged)
            {
                var sourceCollection = this.source as INotifyCollectionChanged;
                sourceCollection.CollectionChanged += OnCollectionChanged;
            }

            if (!isList)
            {
                regex = new Regex(@"\[" + this.key + ",", RegexOptions.IgnorePatternWhitespace);
            }
        }

        public override Type Type => itemInfo.ValueType;

        public override TypeCode TypeCode => itemInfo.ValueTypeCode;

        protected abstract void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e);

        public virtual object GetValue()
        {
            return itemInfo.GetValue(source, key);
        }

        public virtual TValue GetValue<TValue>()
        {
            if (!typeof(TValue).IsAssignableFrom(itemInfo.ValueType))
                throw new InvalidCastException();

            var proxy = itemInfo as IProxyItemInfo<TKey, TValue>;
            if (proxy != null)
                return proxy.GetValue(source, key);

            return (TValue)itemInfo.GetValue(source, key);
        }

        public virtual void SetValue(object value)
        {
            itemInfo.SetValue(source, key, value);
        }

        public virtual void SetValue<TValue>(TValue value)
        {
            var proxy = itemInfo as IProxyItemInfo<TKey, TValue>;
            if (proxy != null)
            {
                proxy.SetValue(source, key, value);
                return;
            }

            itemInfo.SetValue(source, key, value);
        }

        #region IDisposable Support    
        private bool disposedValue = false;

        protected override void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (source != null && source is INotifyCollectionChanged)
                {
                    var sourceCollection = source as INotifyCollectionChanged;
                    sourceCollection.CollectionChanged -= OnCollectionChanged;
                }
                disposedValue = true;
                base.Dispose(disposing);
            }
        }
        #endregion
    }

    public class IntItemNodeProxy : ItemNodeProxy<int>
    {
        public IntItemNodeProxy(ICollection source, int key, IProxyItemInfo itemInfo) : base(source, key, itemInfo)
        {
        }

        protected override void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (isList)
            {
                //IList or Array
                switch (e.Action)
                {
                    case NotifyCollectionChangedAction.Reset:
                        RaiseValueChanged();
                        break;
                    case NotifyCollectionChangedAction.Remove:
                    case NotifyCollectionChangedAction.Replace:
                        if (key == e.OldStartingIndex || key == e.NewStartingIndex)
                            RaiseValueChanged();
                        break;
                    case NotifyCollectionChangedAction.Move:
                        if (key == e.OldStartingIndex || key == e.NewStartingIndex)
                            RaiseValueChanged();
                        break;
                    case NotifyCollectionChangedAction.Add:
                        int endIndex = e.NewItems != null ? e.NewStartingIndex + e.NewItems.Count : e.NewStartingIndex + 1;
                        if (key >= e.NewStartingIndex && key < endIndex)
                            RaiseValueChanged();
                        break;
                    default:
                        break;
                }
            }
            else
            {
                //IDictionary
                if (e.Action == NotifyCollectionChangedAction.Reset)
                {
                    RaiseValueChanged();
                    return;
                }

                if (e.NewItems != null && e.NewItems.Count > 0)
                {
                    foreach (var item in e.NewItems)
                    {
                        if (regex.IsMatch(item.ToString()))
                        {
                            RaiseValueChanged();
                            return;
                        }
                    }
                }

                if (e.OldItems != null && e.OldItems.Count > 0)
                {
                    foreach (var item in e.OldItems)
                    {
                        if (regex.IsMatch(item.ToString()))
                        {
                            RaiseValueChanged();
                            return;
                        }
                    }
                }
            }
        }


    }

    public class StringItemNodeProxy : ItemNodeProxy<string>
    {
        public StringItemNodeProxy(ICollection source, string key, IProxyItemInfo itemInfo) : base(source, key, itemInfo)
        {
        }

        protected override void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            //IDictionary
            if (e.Action == NotifyCollectionChangedAction.Reset)
            {
                RaiseValueChanged();
                return;
            }

            if (e.NewItems != null && e.NewItems.Count > 0)
            {
                foreach (var item in e.NewItems)
                {
                    if (regex.IsMatch(item.ToString()))
                    {
                        RaiseValueChanged();
                        return;
                    }
                }
            }

            if (e.OldItems != null && e.OldItems.Count > 0)
            {
                foreach (var item in e.OldItems)
                {
                    if (regex.IsMatch(item.ToString()))
                    {
                        RaiseValueChanged();
                        return;
                    }
                }
            }
        }
    }
}
