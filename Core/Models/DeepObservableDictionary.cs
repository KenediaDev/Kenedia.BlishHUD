﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Kenedia.Modules.Core.Models
{
    public delegate void DictionaryItemChangedEventHandler<TKey, TValue>(object sender, DictionaryItemChangedEventArgs<TKey, TValue> e);

    public delegate void ValueChangedEventHandler<T>(object sender, ValueChangedEventArgs<T> e);

    public class ValueChangedEventArgs<TValue> : EventArgs
    {
        public ValueChangedEventArgs(TValue? oldValue, TValue? newValue, string? propertyName = null)
        {
            PropertyName = propertyName ?? string.Empty;
            OldValue = oldValue;
            NewValue = newValue;
        }

        public string PropertyName { get; set; } = string.Empty;

        public TValue? OldValue { get; set; }

        public TValue? NewValue { get; set; }
    }

    public class DictionaryItemChangedEventArgs<TKey, TValue> : EventArgs
    {
        public DictionaryItemChangedEventArgs(TKey key, TValue? oldValue, TValue? newValue)
        {
            OldValue = oldValue;
            NewValue = newValue;
            Key = key;
        }

        public TValue? OldValue { get; set; }

        public TValue? NewValue { get; set; }

        public TKey Key { get; set; }
    }

    public class DeepObservableDictionary<TKey, TValue> : Dictionary<TKey, TValue>, IDisposable
        where TValue : INotifyPropertyChanged
    {
        private bool _isDisposed;

        public new TValue this[TKey key]
        {
            get => base[key];
            set => OnValueChanged(key, ContainsKey(key) ? base[key] : default, value);
        }

        public DeepObservableDictionary()
        {
            
        }

        public event EventHandler<DictionaryItemChangedEventArgs<TKey, TValue>> ValueChanged;
        public event EventHandler<PropertyChangedEventArgs> ItemChanged;
        public event EventHandler<PropertyChangedEventArgs> CollectionChanged;

        private void OnItemProperty_Changed(object sender, PropertyChangedEventArgs e)
        {
            ItemChanged?.Invoke(sender, e);
        }

        private void OnValueChanged(TKey key, TValue v, TValue value, [CallerMemberName] string propName = null)
        {
            if (value?.Equals(v) is true) return;
            if (v is not null)
            {
                v.PropertyChanged -= ItemProperty_Changed;
            }

            if (value is not null)
            {
                value.PropertyChanged += ItemProperty_Changed;
            }

            base[key] = value;
            ValueChanged?.Invoke(this, new(key, v, value));
            CollectionChanged?.Invoke(this, new(propName));
        }

        public void Dispose()
        {
            if (_isDisposed) return;
            _isDisposed = true;

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
            ValueChanged?.Invoke(this, new(key, default, value));
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
        
        public virtual void Wipe()
        {
            foreach (var key in Keys.ToList())
            {
                this[key] = default;
            }
        }

        protected void ItemProperty_Changed(object sender, PropertyChangedEventArgs e)
        {;
            OnItemProperty_Changed(sender, e);
        }
    }
}
