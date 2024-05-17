using System;
using System.Collections;
using System.Text.RegularExpressions;
using INotifyCollectionChanged = System.Collections.Specialized.INotifyCollectionChanged;
using NotifyCollectionChangedAction = System.Collections.Specialized.NotifyCollectionChangedAction;
using NotifyCollectionChangedEventArgs = System.Collections.Specialized.NotifyCollectionChangedEventArgs;

namespace Fusion.Mvvm
{
    public abstract class ItemNodeProxy<TKey> : NotifiableSourceProxyBase, IObtainable, IModifiable
    {
        protected readonly TKey _key;
        private readonly IProxyItemInfo _itemInfo;
        protected readonly bool _isList;
        protected readonly Regex _regex;

        protected ItemNodeProxy(ICollection source, TKey key, IProxyItemInfo itemInfo) : base(source)
        {
            _key = key;
            _isList = source is IList;

            _itemInfo = itemInfo;

            if (Source != null && Source is INotifyCollectionChanged sourceCollection)
            {
                sourceCollection.CollectionChanged += OnCollectionChanged;
            }

            if (!_isList)
            {
                _regex = new Regex(@"\[" + _key + ",", RegexOptions.IgnorePatternWhitespace);
            }
        }

        public override Type Type => _itemInfo.ValueType;

        public override TypeCode TypeCode => _itemInfo.ValueTypeCode;

        protected abstract void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e);

        public virtual object GetValue()
        {
            return _itemInfo.GetValue(Source, _key);
        }

        public virtual TValue GetValue<TValue>()
        {
            if (!typeof(TValue).IsAssignableFrom(_itemInfo.ValueType))
                throw new InvalidCastException();

            if (_itemInfo is IProxyItemInfo<TKey, TValue> proxy)
                return proxy.GetValue(Source, _key);

            return (TValue)_itemInfo.GetValue(Source, _key);
        }

        public virtual void SetValue(object value)
        {
            _itemInfo.SetValue(Source, _key, value);
        }

        public virtual void SetValue<TValue>(TValue value)
        {
            if (_itemInfo is IProxyItemInfo<TKey, TValue> proxy)
            {
                proxy.SetValue(Source, _key, value);
                return;
            }

            _itemInfo.SetValue(Source, _key, value);
        }

        #region IDisposable Support

        private bool disposedValue;

        protected override void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (Source != null && Source is INotifyCollectionChanged sourceCollection)
                {
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
            if (_isList)
            {
                //IList or Array
                switch (e.Action)
                {
                    case NotifyCollectionChangedAction.Reset:
                        RaiseValueChanged();
                        break;
                    case NotifyCollectionChangedAction.Remove:
                    case NotifyCollectionChangedAction.Replace:
                        if (_key == e.OldStartingIndex || _key == e.NewStartingIndex)
                            RaiseValueChanged();
                        break;
                    case NotifyCollectionChangedAction.Move:
                        if (_key == e.OldStartingIndex || _key == e.NewStartingIndex)
                            RaiseValueChanged();
                        break;
                    case NotifyCollectionChangedAction.Add:
                        int endIndex = e.NewItems != null ? e.NewStartingIndex + e.NewItems.Count : e.NewStartingIndex + 1;
                        if (_key >= e.NewStartingIndex && _key < endIndex)
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
                        if (_regex.IsMatch(item.ToString()))
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
                        if (_regex.IsMatch(item.ToString()))
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
                    if (_regex.IsMatch(item.ToString()))
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
                    if (_regex.IsMatch(item.ToString()))
                    {
                        RaiseValueChanged();
                        return;
                    }
                }
            }
        }
    }
}