using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Kenedia.Modules.Core.Models
{
    public class DeepObservableDictionary<TKey, TValue> : Dictionary<TKey, TValue>, IDisposable
        where TValue : INotifyPropertyChanged
    {
        private bool _disposed;

        public new TValue this[TKey key]
        {
            get => base[key];
            set => OnValueChanged(key, ContainsKey(key) ? base[key] : default, value);
        }

        public DeepObservableDictionary()
        {

        }

        public event EventHandler<PropertyChangedEventArgs> ItemChanged;
        public event EventHandler<PropertyChangedEventArgs> CollectionChanged;

        private void OnValueChanged(object sender, PropertyChangedEventArgs e)
        {
            ItemChanged?.Invoke(sender, e);
        }

        private void OnValueChanged(TKey key, TValue v, TValue value, [CallerMemberName] string propName = null)
        {
            if (value?.Equals(v) is true) return;
            if (v != null)
            {
                v.PropertyChanged -= ItemProperty_Changed;
            }

            base[key] = value;
            CollectionChanged?.Invoke(this, new(propName));

            if (value != null)
            {
                value.PropertyChanged += ItemProperty_Changed;
            }
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            foreach (var kvp in this)
            {
                kvp.Value.PropertyChanged -= ItemProperty_Changed;
            }
        }

        public new void Add(TKey key, TValue value)
        {
            if (ContainsKey(key)) return;
            value.PropertyChanged += ItemProperty_Changed;

            base.Add(key, value);
            CollectionChanged?.Invoke(this, new("Items"));
        }

        public new bool Remove(TKey key)
        {
            if (ContainsKey(key))
            {
                this[key].PropertyChanged -= ItemProperty_Changed;
            }

            return base.Remove(key);
        }

        protected void ItemProperty_Changed(object sender, PropertyChangedEventArgs e)
        {;
            OnValueChanged(sender, e);
        }
    }
}
