using Blish_HUD;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Kenedia.Modules.Core.Models
{
    public class DeepObservableDictionary<TKey, TValue> : Dictionary<TKey, TValue>, IDisposable
        where TValue : INotifyPropertyChanged
    {
        private bool _disposed;

        public new TValue this[TKey key]
        {
            get => base[key];
            set => OnValueChanged(key, base.ContainsKey(key) ? base[key] : default, value);
        }

        public DeepObservableDictionary()
        {

        }

        public event EventHandler<PropertyChangedEventArgs> ItemChanged;
        public event EventHandler<ValueChangedEventArgs<TValue>> CollectionChanged;

        private void OnValueChanged(object sender, PropertyChangedEventArgs e)
        {
            ItemChanged?.Invoke(sender, e);
        }

        private void OnValueChanged(TKey key, TValue v, TValue value)
        {
            if (value?.Equals(v) is true) return;
            if (v != null)
            {
                v.PropertyChanged -= Value_PropertyChanged;
            }

            base[key] = value;
            CollectionChanged?.Invoke(this, new(v, value));

            if (value != null)
            {
                value.PropertyChanged += Value_PropertyChanged;
            }
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            foreach (var kvp in this)
            {
                kvp.Value.PropertyChanged -= Value_PropertyChanged;
            }
        }

        private void Value_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            OnValueChanged(sender, e);
        }
    }
}
